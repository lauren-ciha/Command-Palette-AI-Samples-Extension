using AdaptiveCards;

namespace FormContents
{
    public class AudioACWrapper
    {
        private AdaptiveCard adaptiveCard;
        private AdaptiveTextBlock markdownTextBlock;
        private AdaptiveSubmitAction recordButton;

        public AudioACWrapper()
        {
            markdownTextBlock = new AdaptiveTextBlock
            {
                Text = "Press the Record button to start transcribing your audio.",
                Style = AdaptiveTextBlockStyle.Heading,
                Wrap = true
            };

            recordButton = new AdaptiveSubmitAction
            {
                Title = "Record",
                Id = "recordButton",
                Data = "Record"
            };

            adaptiveCard = new AdaptiveCard("1.6")
            {
                Body = { markdownTextBlock },
                Actions = { recordButton }
            };
        }

        public void SetMarkdownText(string text)
        {
            markdownTextBlock.Text = text;
        }

        public void SetRecordButtonTitle(string title)
        {
            recordButton.Title = title;
        }

        public string ToJson()
        {
            return adaptiveCard.ToJson();
        }
    }
}

    