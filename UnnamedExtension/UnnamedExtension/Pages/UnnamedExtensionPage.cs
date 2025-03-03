// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using UnnamedExtension.FormContents;
using UnnamedExtension.Templates;

namespace UnnamedExtension;

internal sealed partial class UnnamedExtensionPage : ListPage
{
    public UnnamedExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "UnnamedExtension";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var replacements = new Dictionary<string, string>
        {
            { "title", "Sample Title" },
            { "label", "Name" },
            { "id", "name" },
            { "validation", "^[a-zA-Z]+$" },
            { "error", "Name must be alphabetic" }
        };

        var textContentTemplate = new Template(new TemplateLoader(), "D:\\fhl\\UnnamedExtension\\UnnamedExtension\\Templates\\TextPageTemplate.json", replacements);
        var formContent = new TextFormContent(textContentTemplate.TemplateJson);
        return [
            new ListItem(new TextContentPage(formContent)) { Title = "Text Content Page" },
            new ListItem(new GenerateTextPage(formContent)) { Title = "Generate Text Page" }
        ];
    }
}
