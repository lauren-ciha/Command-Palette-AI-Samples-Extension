﻿using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AISamplesExtension.AI;

namespace AIDevGallery.Sample.Utils;

#pragma warning disable CsWinRT1028 // Class is not marked partial
internal class GenAIModel : IChatClient
#pragma warning restore CsWinRT1028 // Class is not marked partial
{
    private const string TEMPLATE_PLACEHOLDER = "{{CONTENT}}";

    private const int DefaultTopK = 50;
    private const float DefaultTopP = 0.9f;
    private const float DefaultTemperature = 1;
    private const int DefaultMinLength = 0;
    private const int DefaultMaxLength = 1024;
    private const bool DefaultDoSample = false;

    private readonly ChatClientMetadata _metadata;
    private Model? _model;
    private Tokenizer? _tokenizer;
    private LlmPromptTemplate? _template;
    private static readonly SemaphoreSlim _createSemaphore = new(1, 1);
    private static OgaHandle? _ogaHandle;

    private static ChatOptions GetDefaultChatOptions()
    {
        return new ChatOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                { "min_length", DefaultMinLength },
                { "do_sample", DefaultDoSample },
            },
            MaxOutputTokens = DefaultMaxLength,
            Temperature = DefaultTemperature,
            TopP = DefaultTopP,
            TopK = DefaultTopK,
        };
    }

    private GenAIModel(string modelDir)
    {
        _metadata = new ChatClientMetadata("GenAIChatClient", new Uri($"file:///{modelDir}"));
    }

    public static async Task<GenAIModel?> CreateAsync(string modelDir, LlmPromptTemplate? template = null, CancellationToken cancellationToken = default)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var model = new GenAIModel(modelDir);
#pragma warning restore CA2000 // Dispose objects before losing scope

        var lockAcquired = false;
        try
        {
            // ensure we call CreateAsync one at a time to avoid fun issues
            await _createSemaphore.WaitAsync(cancellationToken);
            lockAcquired = true;
            cancellationToken.ThrowIfCancellationRequested();
            await model.InitializeAsync(modelDir, cancellationToken);
        }
        catch
        {
            model?.Dispose();
            return null;
        }
        finally
        {
            if (lockAcquired)
            {
                _createSemaphore.Release();
            }
        }

        model._template = template;
        return model;
    }

    public static void InitializeGenAI()
    {
        _ogaHandle = new OgaHandle();
    }

    [MemberNotNullWhen(true, nameof(_model), nameof(_tokenizer))]
    public bool IsReady => _model != null && _tokenizer != null;

    public void Dispose()
    {
        _model?.Dispose();
        _tokenizer?.Dispose();
        _ogaHandle?.Dispose();
    }

    private string GetPrompt(IList<ChatMessage> history)
    {
        if (!history.Any())
        {
            return string.Empty;
        }

        if (_template == null)
        {
            return string.Join(". ", history);
        }

        StringBuilder prompt = new();

        string systemMsgWithoutSystemTemplate = string.Empty;

        for (var i = 0; i < history.Count; i++)
        {
            var message = history[i];
            if (message.Role == ChatRole.System)
            {
                // ignore system prompts that aren't at the beginning
                if (i == 0)
                {
                    if (string.IsNullOrWhiteSpace(_template.System))
                    {
                        systemMsgWithoutSystemTemplate = message.Text ?? string.Empty;
                    }
                    else
                    {
                        prompt.Append(_template.System.Replace(TEMPLATE_PLACEHOLDER, message.Text));
                    }
                }
            }
            else if (message.Role == ChatRole.User)
            {
                string msgText = message.Text ?? string.Empty;
                if (i == 1 && !string.IsNullOrWhiteSpace(systemMsgWithoutSystemTemplate))
                {
                    msgText = $"{systemMsgWithoutSystemTemplate} {msgText}";
                }

                prompt.Append(string.IsNullOrWhiteSpace(_template.User) ?
                    msgText :
                    _template.User.Replace(TEMPLATE_PLACEHOLDER, msgText));
            }
            else if (message.Role == ChatRole.Assistant)
            {
                prompt.Append(string.IsNullOrWhiteSpace(_template.Assistant) ?
                    message.Text :
                    _template.Assistant.Replace(TEMPLATE_PLACEHOLDER, message.Text));
            }
        }

        if (!string.IsNullOrWhiteSpace(_template.Assistant))
        {
            var substringIndex = _template.Assistant.IndexOf(TEMPLATE_PLACEHOLDER, StringComparison.InvariantCulture);
            prompt.Append(_template.Assistant[..substringIndex]);
        }

        return prompt.ToString();
    }

    public Task<ChatResponse> GetResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        GetStreamingResponseAsync(chatMessages, options, cancellationToken).ToChatResponseAsync(cancellationToken: cancellationToken);

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IList<ChatMessage> chatMessages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var prompt = GetPrompt(chatMessages);

        if (!IsReady)
        {
            throw new InvalidOperationException("Model is not ready");
        }

        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

        using var generatorParams = new GeneratorParams(_model);

        using var sequences = _tokenizer.Encode(prompt);

        void TransferMetadataValue(string propertyName, object defaultValue)
        {
            object? val = null;
            options?.AdditionalProperties?.TryGetValue(propertyName, out val);

            val ??= defaultValue;

            if (val is int intVal)
            {
                generatorParams.SetSearchOption(propertyName, intVal);
            }
            else if (val is float floatVal)
            {
                generatorParams.SetSearchOption(propertyName, floatVal);
            }
            else if (val is bool boolVal)
            {
                generatorParams.SetSearchOption(propertyName, boolVal);
            }
        }

        if (options != null)
        {
            TransferMetadataValue("min_length", DefaultMinLength);
            TransferMetadataValue("do_sample", DefaultDoSample);
            generatorParams.SetSearchOption("temperature", (double)(options?.Temperature ?? DefaultTemperature));
            generatorParams.SetSearchOption("top_p", (double)(options?.TopP ?? DefaultTopP));
            generatorParams.SetSearchOption("top_k", options?.TopK ?? DefaultTopK);
        }

        generatorParams.SetSearchOption("max_length", (options?.MaxOutputTokens ?? DefaultMaxLength) + sequences[0].Length);
        generatorParams.TryGraphCaptureWithMaxBatchSize(1);

        using var tokenizerStream = _tokenizer.CreateStream();
        using var generator = new Generator(_model, generatorParams);
        generator.AppendTokenSequences(sequences);
        StringBuilder stringBuilder = new();
        bool stopTokensAvailable = _template != null && _template.Stop != null && _template.Stop.Length > 0;
        while (!generator.IsDone())
        {
            string part;
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                generator.GenerateNextToken();
                part = tokenizerStream.Decode(generator.GetSequence(0)[^1]);

                if (cancellationToken.IsCancellationRequested && stopTokensAvailable)
                {
                    part = _template!.Stop!.Last();
                }

                stringBuilder.Append(part);

                if (stopTokensAvailable)
                {
                    var str = stringBuilder.ToString();
                    if (_template!.Stop!.Any(str.Contains))
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                break;
            }

            yield return new()
            {
                Role = ChatRole.Assistant,
                Text = part,
            };
        }
    }

    private Task InitializeAsync(string modelDir, CancellationToken cancellationToken = default)
    {
        return Task.Run(
            () =>
            {
                _model = new Model(modelDir);
                cancellationToken.ThrowIfCancellationRequested();
                _tokenizer = new Tokenizer(_model);
            },
            cancellationToken);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return
            serviceKey is not null ? null :
            serviceType == typeof(ChatClientMetadata) ? _metadata :
            _model is not null && serviceType?.IsInstanceOfType(_model) is true ? _model :
            _tokenizer is not null && serviceType?.IsInstanceOfType(_tokenizer) is true ? _tokenizer :
            serviceType?.IsInstanceOfType(this) is true ? this :
            serviceType?.IsInstanceOfType(typeof(ChatOptions)) is true ? GetDefaultChatOptions() :
            null;
    }
}