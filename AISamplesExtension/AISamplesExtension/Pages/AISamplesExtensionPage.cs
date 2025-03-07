// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using FormContents;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Pages;
using System;
using System.Diagnostics;
using AISamplesExtension.FormContents;
using AISamplesExtension.Templates;
using System.Collections.Generic;
using Windows.Graphics.Printing.PrintSupport;

namespace AISamplesExtension;

internal sealed partial class AISamplesExtensionPage : ListPage
{
    private Lazy<GenerateTextPage> _generateTextPage;
    private Lazy<GenerateImagePage> _generateImagePage;
    private Lazy<TranscribeAudioPage> _transcribeAudioPage;
    private Lazy<ImposterKittensPage> _imposterKittensPage; // Added for Imposter Kittens
    private Lazy<TextFormContent> _textFormContent;
    private Lazy<TextFormContent> _imageFormContent;
    private Lazy<ImposterKittensFormContent> _imposterKittensFormContent;

    public AISamplesExtensionPage()
    {
        Icon = new IconInfo("\uE99A");
        Title = "AI Samples Extension";
        Name = "Open";

        // lazy initilaize ImposterKittensFormContent
        _imposterKittensFormContent = new Lazy<ImposterKittensFormContent>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var formContent = new ImposterKittensFormContent(new TemplateLoader().LoadTemplate("ImposterKittensTemplate.json", true, null));
            stopwatch.Stop();
            Debug.WriteLine($"ImposterKittensFormContent construction time: {stopwatch.ElapsedMilliseconds} ms");
            return formContent;
        });

        // lazy initialize TextFormContent
        _textFormContent = new Lazy<TextFormContent>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var textTemplateReplacements = new Dictionary<string, string>
            {
                { "title", "Generate Text" },
                { "placeholder", "Enter a topic to generate text on..." },
                { "id", "name" },
                { "validation", ".*" },
                { "error", "Prompt cannot be empty" }
            };
            var textFormContent = new TextFormContent(new TemplateLoader().LoadTemplate("TextPageTemplate.json", false, textTemplateReplacements));
            stopwatch.Stop();
            Debug.WriteLine($"TextFormContent construction time: {stopwatch.ElapsedMilliseconds} ms");
            return textFormContent;
        });

        // lazy initialize TextFormContent
        _imageFormContent = new Lazy<TextFormContent>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var imageTemplateReplacements = new Dictionary<string, string>
            {
                { "title", "Generate Image (Note: this may take a while)" },
                { "placeholder", "Describe an image to generate..." },
                { "id", "name" },
                { "validation", ".*" },
                { "error", "Prompt cannot be empty" }
            };
            var imageFormContent = new TextFormContent(new TemplateLoader().LoadTemplate("TextPageTemplate.json", false, imageTemplateReplacements));
            stopwatch.Stop();
            Debug.WriteLine($"TextFormContent construction time: {stopwatch.ElapsedMilliseconds} ms");
            return imageFormContent;
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
            var page = new GenerateImagePage(_imageFormContent.Value);
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

        // Initialize lazy loading for Imposter Kittens page
        _imposterKittensPage = new Lazy<ImposterKittensPage>(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var page = new ImposterKittensPage(_imposterKittensFormContent.Value);
            stopwatch.Stop();
            Debug.WriteLine($"ImposterKittensPage construction time: {stopwatch.ElapsedMilliseconds} ms");
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
            new ListItem(_imposterKittensPage.Value) { Title = "Imposter Kittens Game" }, // Added the new page to the items
        };

        stopwatch.Stop(); // Stop timing
        Debug.WriteLine($"GetItems() method execution time: {stopwatch.ElapsedMilliseconds} ms");

        return items;
    }
}
