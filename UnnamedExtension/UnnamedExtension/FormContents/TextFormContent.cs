using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Text.Json.Nodes;

namespace UnnamedExtension.FormContents
{
    internal sealed partial class TextFormContent : FormContent
    {
        public event EventHandler<string>? OnSubmit;

        public TextFormContent(string templateJson)
        {
            TemplateJson = templateJson;
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            var formInput = JsonNode.Parse(inputs)?.AsObject();
            if (formInput == null)
            {
                return CommandResult.GoHome();
            }

            OnSubmit?.Invoke(this, formInput["name"]?.ToString() ?? string.Empty);

            return CommandResult.GoHome();
        }
    }
}
