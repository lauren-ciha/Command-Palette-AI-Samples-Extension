using FormContents;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pages
{
    internal sealed class ImposterKittensPage : ContentPage
    {
        private readonly ImposterKittensFormContent _formContent;
        private readonly List<IContent> _contents;
        private readonly Random _random = new Random();

        public ImposterKittensPage(ImposterKittensFormContent formContent)
        {
            Icon = new IconInfo("\uE8D1"); // Cat icon
            Title = "Imposter Kittens";
            Name = "Open";

            _formContent = formContent;
            _formContent.IsPageLoadingChanged += FormContent_IsPageLoadingChanged;
            _formContent.OnGuessSubmit += FormContent_OnGuessSubmit;

            _contents = new List<IContent>();
            _contents.Add(_formContent);

            // Initially load some images
            LoadNewImages();
        }

        private void FormContent_IsPageLoadingChanged(object? sender, bool isLoading)
        {
            IsLoading = isLoading;
        }

        private void FormContent_OnGuessSubmit(object? sender, string selectedImagePath)
        {
            // Determine if the selected image is AI-generated
            // For now, this is a stub implementation
            bool isSelectedAI = IsImageAIGenerated(selectedImagePath);

            if (isSelectedAI)
            {
                var statusMessage = new StatusMessage
                {
                    Message = "Correct!",
                    State = MessageState.Success,
                };
                new ToastStatusMessage(statusMessage).Show();
            }
            else
            {
                var statusMessage = new StatusMessage
                {
                    Message = "Incorrect! Try again!",
                    State = MessageState.Error,
                };
                new ToastStatusMessage(statusMessage).Show();
            }

            // Load new images for next round
            LoadNewImages();
        }

        private bool IsImageAIGenerated(string imagePath)
        {
            // Stub implementation - in a real app, you would determine this based 
            // on the actual image properties or metadata
            Debug.WriteLine($"Checking if image is AI-generated: {imagePath}");

            // For now, just check if the filename contains "AI" for demonstration
            return imagePath.Contains("AI", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadNewImages()
        {
            try
            {
                // This is a stub implementation
                // In a real app, you would load actual images from a repository

                // Randomly decide which side will have the AI-generated image
                bool isLeftImageAI = _random.Next(2) == 0;

                // Example file paths - in a real app these would point to actual images
                string leftPath = isLeftImageAI
                    ? "file:///C:/SampleImages/AI-Kitten1.png"
                    : "file:///C:/SampleImages/Real-Kitten1.png";

                string rightPath = isLeftImageAI
                    ? "file:///C:/SampleImages/Real-Kitten2.png"
                    : "file:///C:/SampleImages/AI-Kitten2.png";

                _formContent.LoadImages(leftPath, rightPath, isLeftImageAI);

                // Notify that the content has changed
                RaiseItemsChanged(_contents.Count);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading images: {ex}");

                var statusMessage = new StatusMessage
                {
                    Message = $"Error loading images: {ex.Message}",
                    State = MessageState.Error,
                };
                new ToastStatusMessage(statusMessage).Show();
            }
        }

        public override IContent[] GetContent()
        {
            return _contents.ToArray();
        }
    }
}
