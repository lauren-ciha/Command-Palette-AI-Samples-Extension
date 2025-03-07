using AdaptiveCards;
using AIDevGallery.Sample.Utils;
using AISamplesExtension.Helpers;
using AISamplesExtension.Templates;
using Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Shell;

namespace FormContents
{
    public partial class ImposterKittensFormContent : FormContent, IDisposable
    {
        private AdaptiveCard adaptiveCard;
        private string realImagePath = string.Empty;
        private string aiImagePath = string.Empty;
        private bool isLeftImageAI;
        private CancellationTokenSource cts = new();
        private StableDiffusion? stableDiffusion;
        private bool isCanceling;
        private Task? inferenceTask;
        private string genImagePath = string.Empty;
        private bool modelReady;

        // Events
        public event EventHandler<bool>? IsPageLoadingChanged;
        public event EventHandler<string>? OnGuessSubmit;
        public event EventHandler? OnImagesLoaded;
        public event EventHandler? OnImageGeneratorLoaded;
        public event EventHandler<NotificationRaisedEventArgs>? OnNotificationRaised;

        public ImposterKittensFormContent(string templateJson)
        {
            IsPageLoadingChanged?.Invoke(this, !modelReady);
            Task.Run(() => LoadStableDiffusion());
            TemplateJson = templateJson;

            // Initialize the adaptive card from the JSON string
            var parseResult = AdaptiveCard.FromJson(templateJson);
            adaptiveCard = parseResult.Card;
            if (adaptiveCard.Body.Count < 3)
            {
                NotificationRaisedEventArgs args = new NotificationRaisedEventArgs("Adaptive card body does not contain enough elements.", MessageState.Error);
                OnNotificationRaised?.Invoke(this, args);
            }
        }

        public AdaptiveCard GetAdapativeCard()
        {
            return adaptiveCard;
        }

        private async Task LoadStableDiffusion()
        {
            var hardwareAccelerator = HardwareAccelerator.CPU;
            var parentFolder = System.IO.Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Models", @"onnx");
            await Task.Run(() =>
            {
                stableDiffusion = new StableDiffusion(parentFolder, hardwareAccelerator);
            });

            modelReady = true;
            Debug.WriteLine("modelReady is true in ImposterKittensFormContent");
            OnImageGeneratorLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void LoadImages(string leftImagePath, string rightImagePath, bool isLeftImageAI)
        {
            try
            {
                // Notify that we're loading
                IsPageLoadingChanged?.Invoke(this, true);

                this.isLeftImageAI = isLeftImageAI;

                // Update the images in the adaptive card
                var body = adaptiveCard.Body;

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

                TemplateJson = adaptiveCard.ToJson();

                OnImagesLoaded?.Invoke(this, EventArgs.Empty);
                IsPageLoadingChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs($"Error loading images: {ex.Message}", MessageState.Error));
                IsPageLoadingChanged?.Invoke(this, false);
            }
        }

        public async Task LoadAIGeneratedImage()
        {
            try
            {
                // Notify that we're loading
                IsPageLoadingChanged?.Invoke(this, true);

                if (!modelReady)
                {
                    OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs("Model is not ready. Please try again later.", MessageState.Error));
                    IsPageLoadingChanged?.Invoke(this, false);
                    return;
                }

                // Generate an AI kitten image
                string aiKittenImagePath = await GenerateKittenImage();

                if (string.IsNullOrEmpty(aiKittenImagePath))
                {
                    OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs("Failed to generate AI image. Please try again.", MessageState.Error));
                    IsPageLoadingChanged?.Invoke(this, false);
                    return;
                }

                // Get a real kitten image path from predefined set
                string realKittenImagePath = GetRealKittenImagePath();

                // Randomly decide whether to put AI image on left or right
                bool placeAIOnLeft = new Random().Next(2) == 0;

                // Load the images into the card
                if (placeAIOnLeft)
                {
                    LoadImages(aiKittenImagePath, realKittenImagePath, true);
                }
                else
                {
                    LoadImages(realKittenImagePath, aiKittenImagePath, false);
                }
            }
            catch (Exception ex)
            {
                OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs($"Error loading AI-generated image: {ex.Message}", MessageState.Error));
                IsPageLoadingChanged?.Invoke(this, false);
            }
        }

        private async Task<string> GenerateKittenImage()
        {
            string generatedImagePath = string.Empty;
            string kittenPrompt = "realistic kitten"; // Fixed prompt for kitten images

            if (!modelReady || stableDiffusion == null)
            {
                Debug.WriteLine("StableDiffusion model is not ready or null");
                return generatedImagePath;
            }

            if (inferenceTask != null)
            {
                Debug.WriteLine("Inference task is already running. Canceling previous task.");
                cts.Cancel();
                isCanceling = true;
                await inferenceTask;
                isCanceling = false;
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);
            CancellationToken token = linkedCts.Token;

            inferenceTask = Task.Run(
                async () =>
                {
                    try
                    {
                        var result = stableDiffusion.Inference(kittenPrompt, token);
                        Debug.WriteLine($"Inference result: {result}");

                        if (result is Bitmap image)
                        {
                            string uniqueId = DateTime.Now.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            string filePath = Path.Join(
                                Windows.ApplicationModel.Package.Current.InstalledLocation.Path,
                                "Assets",
                                $"AI_Kitten_{uniqueId}.png");

                            ImageHelper.SaveBitmapAsPng(image, filePath);
                            generatedImagePath = filePath;
                        }
                        else
                        {
                            Debug.WriteLine("Inference did not return a valid image");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        if (timeoutCts.IsCancellationRequested)
                        {
                            Debug.WriteLine("Image generation timed out");
                            OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs("Image generation timed out. Please try again.", MessageState.Info));
                        }
                        else
                        {
                            Debug.WriteLine("Inference task was canceled");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error during inference: {ex}");
                        OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs($"Error during inference: {ex.Message}", MessageState.Error));
                    }
                },
                token);

            await inferenceTask;
            inferenceTask = null;

            return generatedImagePath;
        }

        private static string GetRealKittenImagePath()
        {
            try
            {
                // Get the base path to the Assets/Kittens directory
                string kittensDirectory = Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "Kittens");

                // Check if directory exists
                if (!Directory.Exists(kittensDirectory))
                {
                    Debug.WriteLine($"Kittens directory not found: {kittensDirectory}");

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(kittensDirectory);

                    // Return a fallback image path since there are no images yet
                    return Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "placeholder.png");
                }

                // Get all image files from the directory (supporting common image formats)
                var imageFiles = Directory.GetFiles(kittensDirectory, "*.*")
                    .Where(file =>
                        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                // Check if we have any image files
                if (imageFiles.Length == 0)
                {
                    Debug.WriteLine("No image files found in Kittens directory");
                    return Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "placeholder.png");
                }

                // Pick a random file from the array
                int randomIndex = new Random().Next(imageFiles.Length);
                string selectedImagePath = imageFiles[randomIndex];

                Debug.WriteLine($"Selected kitten image: {selectedImagePath}");
                return selectedImagePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetRealKittenImagePath: {ex.Message}");
                return Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "placeholder.png");
            }
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            try
            {
                var selectedImage = ParseJsonData(data);

                if (!string.IsNullOrEmpty(selectedImage))
                {
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
                OnNotificationRaised?.Invoke(this, new NotificationRaisedEventArgs($"Error processing selection: {ex.Message}", MessageState.Error));
            }

            return CommandResult.KeepOpen();
        }

        public static string ParseJsonData(string data)
        {
            try
            {
                var actionData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);
                var selectedImage = actionData != null ? actionData["selectedImage"] : null;
                return selectedImage ?? string.Empty;
            }
            catch
            {
                Debug.WriteLine("Error parsing JSON data");
                return string.Empty;
            }
        }

        public void Dispose()
        {
            cts.Dispose();
            stableDiffusion?.Dispose();
        }
    }
}
