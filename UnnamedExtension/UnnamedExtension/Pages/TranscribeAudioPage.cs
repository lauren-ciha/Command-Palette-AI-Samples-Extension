using FormContents;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;

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
            _contents.Add(_formContent);
        }
        public override IContent[] GetContent() => _contents.ToArray();
    }
}
