using AdaptiveCards;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FormContents
{
    public class ImposterKittensFormContent : FormContent
    {
        private AdaptiveCard adaptiveCard;
        private string realImagePath = string.Empty;
        private string aiImagePath = string.Empty;
        private bool isLeftImageAI;

        // Events
        public event EventHandler<bool>? IsPageLoadingChanged;
        public event EventHandler<string>? OnGuessSubmit;

        public ImposterKittensFormContent()
        {
            CreateAdaptiveCard();
        }

        private void CreateAdaptiveCard()
        {
            adaptiveCard = new AdaptiveCard("1.5")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Imposter Kittens",
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Click on the kitten that is AI-generated",
                        Size = AdaptiveTextSize.Medium,
                        Weight = AdaptiveTextWeight.Bolder
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "1",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Id = "leftImage",
                                        Url = new Uri("https://placeholder.com/300"),
                                        Size = AdaptiveImageSize.Large,
                                        SelectAction = new AdaptiveSubmitAction
                                        {
                                            Id = "leftImageAction",
                                            Title = "Select Left Image",
                                            Data = "{\"selectedImage\":\"left\"}"
                                        }
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = "1",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Id = "rightImage",
                                        Url = new Uri("https://placeholder.com/300"),
                                        Size = AdaptiveImageSize.Large,
                                        SelectAction = new AdaptiveSubmitAction
                                        {
                                            Id = "rightImageAction",
                                            Title = "Select Right Image",
                                            Data = "{\"selectedImage\":\"right\"}"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            TemplateJson = adaptiveCard.ToJson();
        }

        public void LoadImages(string leftImagePath, string rightImagePath, bool isLeftImageAI)
        {
            try
            {
                // Notify that we're loading
                IsPageLoadingChanged?.Invoke(this, true);

                this.isLeftImageAI = isLeftImageAI;
                
                // Update the images in the adaptive card
                var columnSet = (AdaptiveColumnSet)adaptiveCard.Body[2];
                
                var leftColumn = columnSet.Columns[0];
                var leftImage = (AdaptiveImage)leftColumn.Items[0];
                leftImage.Url = new Uri(leftImagePath);

                var rightColumn = columnSet.Columns[1];
                var rightImage = (AdaptiveImage)rightColumn.Items[0];
                rightImage.Url = new Uri(rightImagePath);

                // Store paths for reference
                if (isLeftImageAI)
                {
                    aiImagePath = leftImagePath;
                    realImagePath = rightImagePath;
                }
                else
                {
                    realImagePath = leftImagePath;
                    aiImagePath = rightImagePath;
                }

                // Update the template JSON
                TemplateJson = adaptiveCard.ToJson();

                // Notify that we're done loading
                IsPageLoadingChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                ShowToastMessage($"Error loading images: {ex.Message}", MessageState.Error);
                IsPageLoadingChanged?.Invoke(this, false);
            }
        }

        public async Task LoadAIGeneratedImage()
        {
            // Stub method for now - would eventually generate or fetch AI images
            IsPageLoadingChanged?.Invoke(this, true);
            
            // Simulate some work
            await Task.Delay(1000);
            
            IsPageLoadingChanged?.Invoke(this, false);
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            try
            {
                // Parse the JSON data to determine which image was selected
                var actionData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);
                
                if (actionData != null && actionData.selectedImage != null)
                {
                    string selectedImage = actionData.selectedImage.ToString();
                    
                    // Determine if the selection was correct
                    bool isAISelected = (selectedImage == "left" && isLeftImageAI) || 
                                       (selectedImage == "right" && !isLeftImageAI);
                    
                    // Raise event with the selected image path
                    string selectedImagePath = selectedImage == "left" ? 
                        (isLeftImageAI ? aiImagePath : realImagePath) : 
                        (isLeftImageAI ? realImagePath : aiImagePath);
                    
                    OnGuessSubmit?.Invoke(this, selectedImagePath);
                }
            }
            catch (Exception ex)
            {
                ShowToastMessage($"Error processing selection: {ex.Message}", MessageState.Error);
            }

            return CommandResult.KeepOpen();
        }

        private void ShowToastMessage(string message, MessageState state)
        {
            var statusMessage = new StatusMessage
            {
                Message = message,
                State = state,
            };
            var toast = new ToastStatusMessage(statusMessage);
            toast.Show();
        }
    }
}
