using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Text.Json.Nodes;

namespace UnnamedExtension.FormContents
{
    internal sealed partial class TextFormContent : FormContent
    {
        public event EventHandler<string>? OnSubmit;
        public event EventHandler<bool>? IsLoadingChanged;

        public TextFormContent(string templateJson)
        {
            TemplateJson = templateJson;
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            IsLoadingChanged?.Invoke(this, true);
            try
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
                else
                {
                    OnSubmit?.Invoke(this, formInput["name"]?.ToString() ?? string.Empty);
                }

                return CommandResult.KeepOpen();
            }
            catch (Exception ex)
            {
                return CommandResult.ShowToast(new ToastArgs
                {
                    Message = $"Error: {ex.Message}",
                    Result = CommandResult.KeepOpen(),
                });
            }
        }
    }
}
