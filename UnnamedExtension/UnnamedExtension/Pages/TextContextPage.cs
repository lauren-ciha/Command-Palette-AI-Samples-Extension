// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnnamedExtension.Templates;

namespace UnnamedExtension;

internal sealed partial class TextContentPage : ContentPage
{
    FormContent _formContent;

    public TextContentPage(Template template)
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Text Content Page";
        Name = "Open";
        _formContent = CreateFormContent(template);
    }

    public override IContent[] GetContent()
    {
        return
        [
            _formContent
        ];
    }

#pragma warning disable CA1822 // Mark members as static
    private FormContent CreateFormContent(Template template)
#pragma warning restore CA1822 // Mark members as static
    {
        var formContent = new FormContent();
        formContent.TemplateJson = template.TemplateJson;
        return formContent;
    }
}
