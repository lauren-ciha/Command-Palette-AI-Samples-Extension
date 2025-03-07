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
            _formContent.OnImagesLoaded += FormContent_OnImagesLoaded;
            _formContent.OnImageGeneratorLoaded += _formContent_OnImageGeneratorLoaded;

            _contents = new List<IContent>();
            _contents.Add(_formContent);
        }

        private void _formContent_OnImageGeneratorLoaded(object? sender, EventArgs e)
        {
            LoadNewImages();
        }

        private void FormContent_OnImagesLoaded(object? sender, EventArgs e)
        {
            Debug.WriteLine("Images loaded in ImposterKittensFormContent");

            // Notify the UI to refresh and show the updated adaptive card with images
            RaiseItemsChanged(_contents.Count);
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
                // Instead of loading static images, use the LoadAIGeneratedImage method
                // which will generate a new AI image and get a random real kitten image
                _ = _formContent.LoadAIGeneratedImage();

                // Note: We don't need to call RaiseItemsChanged here anymore
                // since it will be called by the FormContent_OnImagesLoaded event handler
                // when the images are actually loaded.
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
