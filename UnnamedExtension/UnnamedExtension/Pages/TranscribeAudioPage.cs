using FormContents;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pages
{
    public partial class TranscribeAudioPage : ContentPage
    {
        TranscribeAudioFormContent _formContent;
        List<IContent> _contents = new List<IContent>();

        public TranscribeAudioPage(TranscribeAudioFormContent transcribeAudioFormContent)
        {
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
            Title = "Trascribe Audio Page";
            Name = "Open";
            _formContent = transcribeAudioFormContent;
            _formContent.OnSubmit += FormContent_OnSubmit;
            _contents.Add(_formContent);
        }

        private void FormContent_OnSubmit(object? sender, string json)
        {
            Debug.WriteLine("TranscribeAudioPage: FormContent_OnSubmit");
            _formContent.TemplateJson = json;
            RaiseItemsChanged(_contents.Count);
        }

        public override IContent[] GetContent() => _contents.ToArray();
    }
}
