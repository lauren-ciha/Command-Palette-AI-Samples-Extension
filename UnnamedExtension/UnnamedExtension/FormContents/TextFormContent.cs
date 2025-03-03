using AIDevGallery.Sample.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Extensions.AI;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using UnnamedExtension.AI;

namespace UnnamedExtension.FormContents
{
    internal sealed partial class TextFormContent : FormContent
    {
        public event EventHandler<string>? OnSubmit;
        private IChatClient? chatClient;
        private CancellationTokenSource? cts;

        public TextFormContent(string templateJson)
        {
            TemplateJson = templateJson;
            chatClient = CreateChatClientAsync().Result;
            if (chatClient == null)
            {
                throw new InvalidOperationException("Failed to create chat client.");
            }
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            var formInput = JsonNode.Parse(inputs)?.AsObject();
            if (formInput == null)
            {
                return CommandResult.ShowToast(new ToastArgs
                {
                    Message = "Invalid input",
                    Result = CommandResult.KeepOpen(),
                });
            }

            OnSubmit?.Invoke(this, formInput["name"]?.ToString() ?? string.Empty);

            return CommandResult.KeepOpen();
        }

#pragma warning disable CA1822 // Mark members as static
        public async Task<IChatClient?> CreateChatClientAsync()
#pragma warning restore CA1822 // Mark members as static
        {
            return await GenAIModel.CreateAsync(System.IO.Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Models", @"cpu-int4-rtn-block-32-acc-level-4"), new LlmPromptTemplate
            {
                System = "<|system|>\n{{CONTENT}}<|end|>\n",
                User = "<|user|>\n{{CONTENT}}<|end|>\n",
                Assistant = "<|assistant|>\n{{CONTENT}}<|end|>\n",
                Stop = ["<|system|>", "<|user|>", "<|assistant|>", "<|end|>"]
            });
        }

    }
}
