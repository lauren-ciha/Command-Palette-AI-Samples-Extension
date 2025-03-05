using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Pages
{
    public partial class TranscribeAudioPage : ContentPage
    {
        public TranscribeAudioPage()
        {
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
            Title = "Trascribe Audio Page";
            Name = "Open";
        }
        public override IContent[] GetContent() => new IContent[]
            {
                new FormContent()
            };
    }
}
