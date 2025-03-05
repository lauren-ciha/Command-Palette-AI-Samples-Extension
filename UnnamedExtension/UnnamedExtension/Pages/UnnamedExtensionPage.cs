// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Pages;
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
            { "title", "Generate Text" },
            { "placeholder", "Enter a topic to generate text on..." },
            { "id", "name" },
            { "validation", ".*" },
            { "error", "Name cannot be empty" }
        };

        var textContentTemplate = new Template(new TemplateLoader(), "D:\\fhl\\UnnamedExtension\\UnnamedExtension\\Templates\\TextPageTemplate.json", replacements);
        var formContent = new TextFormContent(textContentTemplate.TemplateJson);
        return [
            new ListItem(new GenerateTextPage(formContent)) { Title = "Generate Text Page" },
            new ListItem(new GenerateImagePage(formContent)) { Title = "Generate Image Page" },
            new ListItem(new TranscribeAudioPage()) { Title = "Transcribe Audio Page" },
        ];
    }
}
