// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnnamedExtension.FormContents;

namespace UnnamedExtension;

internal sealed partial class TextContentPage : ContentPage
{
    FormContent _formContent;

    public TextContentPage(TextFormContent formContent)
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Text Content Page";
        Name = "Open";
        _formContent = formContent;

        if (_formContent is TextFormContent textFormContent)
        {
            textFormContent.OnSubmit += TextFormContent_OnSubmit;
        }
    }

    private void TextFormContent_OnSubmit(object? sender, string inputs)
    {
        var statusMessage = new StatusMessage
        {
            Message = inputs,
            State = MessageState.Success,
        };
        var toast = new ToastStatusMessage(statusMessage);
        toast.Show();
    }

    public override IContent[] GetContent()
    {
        return
        [
            _formContent
        ];
    }
}
