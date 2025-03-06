// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AIDevGallery.Sample.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnnamedExtension.AI;
using UnnamedExtension.FormContents;

namespace UnnamedExtension;

internal sealed partial class GenerateTextPage : ContentPage, IDisposable
{
    TextFormContent _formContent;
    private IChatClient? chatClient;
    private CancellationTokenSource? cts;
    List<IContent> _contents;

    public GenerateTextPage(TextFormContent formContent)
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Generate Page";
        Name = "Open";
        _formContent = formContent;
        _formContent.OnSubmit += TextFormContent_OnSubmit;
        _formContent.IsLoadingChanged += (sender, isLoading) =>
        {
            IsLoading = isLoading;
        };
        GenAIModel.InitializeGenAI();
        _contents = new List<IContent>();
        _contents.Add(_formContent);

        chatClient = GetChatClientAsync().Result;
    }

    private async Task<IChatClient> GetChatClientAsync()
    {
        if (chatClient == null)
        {
            chatClient = await GenAIModel.CreateAsync(System.IO.Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Models", @"cpu-int4-rtn-block-32-acc-level-4"), new LlmPromptTemplate
            {
                System = "<|system|>\n{{CONTENT}}<|end|>\n",
                User = "<|user|>\n{{CONTENT}}<|end|>\n",
                Assistant = "<|assistant|>\n{{CONTENT}}<|end|>\n",
                Stop = ["<|system|>", "<|user|>", "<|assistant|>", "<|end|>"]
            });
        }
        return chatClient;
    }

    public async Task<string> GenerateText(string topic)
    {
        var message = string.Empty;
        if (chatClient == null)
        {
            return "chatClient is null";
        }

        string systemPrompt = "You generate text based on a user-provided topic. Respond with only the generated content and no extraneous text.";
        string userPrompt = "Generate text based on the topic: " + topic;

        cts = new CancellationTokenSource();

        await foreach (var messagePart in chatClient.GetStreamingResponseAsync(
            [
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt)
            ],
            null,
            cts.Token))
        {
            message += messagePart;
        }

        cts?.Dispose();
        cts = null;

        return message;
    }

    private void TextFormContent_OnSubmit(object? sender, string inputs)
    {
        if (_contents.Count > 1)
        {
            _contents.RemoveAt(1);
            RaiseItemsChanged(_contents.Count);
        }

        try
        {
            var response = new MarkdownContent();
            response.Body = GenerateText(inputs).Result;
            _contents.Add(response);
            RaiseItemsChanged(_contents.Count);
            IsLoading = false;

        }
        catch (Exception ex)
        {
            IsLoading = false;
            var statusMessage = new StatusMessage
            {
                Message = $"Error: {ex.Message}",
                State = MessageState.Error,
            };
            ToastStatusMessage toast = new ToastStatusMessage(statusMessage);
            toast.Show();
        }
    }

    public override IContent[] GetContent()
    {
        return _contents.ToArray();
    }

    private void CleanUp()
    {
        CancelGeneration();
        chatClient?.Dispose();
    }

    private void CancelGeneration()
    {
        IsLoading = false;
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    public void Dispose()
    {
        chatClient?.Dispose();
        cts?.Dispose();
        _formContent.OnSubmit -= TextFormContent_OnSubmit;
        _formContent.IsLoadingChanged -= (sender, isLoading) =>
        {
            IsLoading = isLoading;
        };
        CleanUp();
    }
}
