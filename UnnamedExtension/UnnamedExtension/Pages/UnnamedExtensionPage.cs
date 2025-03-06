// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FormContents;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using UnnamedExtension.FormContents;
using UnnamedExtension.Templates;

namespace UnnamedExtension;

internal sealed partial class UnnamedExtensionPage : ListPage
{
    private Lazy<GenerateTextPage> _generateTextPage;
    private Lazy<GenerateImagePage> _generateImagePage;
    private Lazy<TranscribeAudioPage> _transcribeAudioPage;
    private Lazy<TextFormContent> _textFormContent;

    public UnnamedExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "UnnamedExtension";
        Name = "Open";

        // lazy initialize TextFormContent
        _textFormContent = new Lazy<TextFormContent>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var textFormContent = new TextFormContent(new TemplateLoader().LoadTemplate("TextPageTemplate.json", false));
            stopwatch.Stop();
            Debug.WriteLine($"TextFormContent construction time: {stopwatch.ElapsedMilliseconds} ms");
            return textFormContent;
        });

        // Initialize lazy loading for pages and models
        _generateTextPage = new Lazy<GenerateTextPage>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var page = new GenerateTextPage(_textFormContent.Value);
            stopwatch.Stop();
            Debug.WriteLine($"GenerateTextPage construction time: {stopwatch.ElapsedMilliseconds} ms");
            return page;
        });

        _generateImagePage = new Lazy<GenerateImagePage>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var page = new GenerateImagePage(_textFormContent.Value);
            stopwatch.Stop();
            Debug.WriteLine($"GenerateImagePage construction time: {stopwatch.ElapsedMilliseconds} ms");
            return page;
        });

        _transcribeAudioPage = new Lazy<TranscribeAudioPage>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var page = new TranscribeAudioPage(new TranscribeAudioFormContent(new AudioACWrapper()));
            stopwatch.Stop();
            Debug.WriteLine($"TranscribeAudioPage construction time: {stopwatch.ElapsedMilliseconds} ms");
            return page;
        });
    }

    public override IListItem[] GetItems()
    {
        var stopwatch = Stopwatch.StartNew(); // Start timing

        var items = new IListItem[]
        {
            new ListItem(_generateTextPage.Value) { Title = "Generate Text Page" },
            new ListItem(_generateImagePage.Value) { Title = "Generate Image Page" },
            new ListItem(_transcribeAudioPage.Value) { Title = "Transcribe Audio Page" },
        };

        stopwatch.Stop(); // Stop timing
        Debug.WriteLine($"GetItems() method execution time: {stopwatch.ElapsedMilliseconds} ms");

        return items;
    }
}
