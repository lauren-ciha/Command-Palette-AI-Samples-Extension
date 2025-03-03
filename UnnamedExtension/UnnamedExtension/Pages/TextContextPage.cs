// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace UnnamedExtension;

internal sealed partial class TextContentPage : ContentPage
{
    public TextContentPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Text Content Page";
        Name = "Open";
    }

    public override IContent[] GetContent()
    {
        throw new System.NotImplementedException();
    }
}
