// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AIDevGallery.Sample.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.AI;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnnamedExtension.AI;
using UnnamedExtension.FormContents;

namespace UnnamedExtension;

internal sealed partial class GenerateTextPage : ContentPage
{
    TextFormContent _formContent;
    private IChatClient? chatClient;
    private CancellationTokenSource? cts;
    string message = string.Empty;

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
        try
        {
            var toastThis = GenerateText(inputs).Result;
            IsLoading = false;
            var statusMessage = new StatusMessage
            {
                Message = toastThis,
                State = MessageState.Success,
            };
            ToastStatusMessage toast = new ToastStatusMessage(statusMessage);
            toast.Show();
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
        finally
        {
            CleanUp();
        }
    }

    public override IContent[] GetContent()
    {
        return
        [
            _formContent
        ];
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
}
