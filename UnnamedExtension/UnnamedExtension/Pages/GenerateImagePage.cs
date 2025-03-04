using AIDevGallery.Sample.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using UnnamedExtension.FormContents;
using System.Diagnostics;
using System.IO;

namespace Pages
{
    internal sealed partial class GenerateImagePage : ContentPage, IDisposable
    {
        private TextFormContent _textFormContent;
        private List<IContent> _contents;
        private string prompt = string.Empty;
        private bool modelReady;
        private CancellationTokenSource cts = new();
        private StableDiffusion? stableDiffusion;
        private bool isCanceling;
        private Task? inferenceTask;
        private string genImagePath;

        public GenerateImagePage(TextFormContent textFormContent)
        {
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
            Title = "Generate Image";
            Name = "Open";
            _textFormContent = textFormContent;
            _textFormContent.OnSubmit += TextFormContent_OnSubmit;
            _textFormContent.IsLoadingChanged += (sender, isLoading) =>
            {
                IsLoading = isLoading;
            };
            _contents = new List<IContent>();
            _contents.Add(_textFormContent);
            LoadStableDiffusion().Wait();
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
            Debug.WriteLine("modelReady is true");
        }

        private async void TextFormContent_OnSubmit(object? sender, object inputs)
        {
            if (inputs is Exception ex)
            {
                HandleError(ex);
            }
            else if (inputs is string inputsString)
            {
                try
                {
                    var response = new MarkdownContent();
                    await DoStableDiffusion(inputsString);
                    if (genImagePath != null)
                    {
                        string imagePath = genImagePath;
                        response.Body = $"See image below:\nImagePath:{imagePath}\n\n![Generated Image]({imagePath})";
                        _contents.Add(response);
                        RaiseItemsChanged(_contents.Count);
                    }
                    else
                    {
                        response.Body = "Image generation failed.";
                    }
                    IsLoading = false;
                }
                catch (Exception exception)
                {
                    HandleError(exception);
                }
            }
        }

        private void HandleError(Exception ex)
        {
            IsLoading = false;
            var statusMessage = new StatusMessage
            {
                Message = $"Error: {ex.Message}, StackTrace: {ex.StackTrace}",
                State = MessageState.Error,
            };
            ToastStatusMessage toast = new ToastStatusMessage(statusMessage);
            toast.Show();
        }

        private async Task DoStableDiffusion(string inputs)
        {
            Debug.WriteLine($"Generating image with prompt: {inputs}");
            if (!modelReady || isCanceling)
            {
                Debug.WriteLine("Model is not ready or is canceling.");
                return;
            }

            if (inferenceTask != null)
            {
                Debug.WriteLine("Inference task is already running. Canceling the previous task.");
                cts.Cancel();
                isCanceling = true;
                await inferenceTask;
                isCanceling = false;
                return;
            }

            if (stableDiffusion == null)
            {
                Debug.WriteLine("StableDiffusion is not initialized.");
                return;
            }
            if (string.IsNullOrEmpty(inputs))
            {
                Debug.WriteLine("Prompt is empty.");
                return;
            }
            prompt = inputs;

            CancellationToken token = CancelGenerationAndGetNewToken();
            Debug.WriteLine($"Token received. Prompt: {prompt}");
            inferenceTask = Task.Run(
                async () =>
                {
                    try
                    {
                        var res = stableDiffusion!.Inference(prompt, token);
                        Debug.WriteLine($"Inference result: {res}");
                        if (res is Bitmap image)
                        {
                            string filePath = System.IO.Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "GeneratedImage.png");
                            SaveBitmapAsPng(image, filePath);
                            genImagePath = filePath;
                        }
                        else
                        {
                            throw new ArgumentException("The inference did not return a valid image.");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is not OperationCanceledException)
                        {
                            HandleError(ex);
                        }
                    }
                },
                token);

            await inferenceTask;
            Debug.WriteLine("Inference task completed. Setting to null");
            inferenceTask = null;

        }

        private CancellationToken CancelGenerationAndGetNewToken()
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
            return cts.Token;
        }

        private void CleanUp()
        {
            cts?.Cancel();
            cts?.Dispose();
            stableDiffusion?.Dispose();
        }

        public void Dispose()
        {
            CleanUp();
        }

        public override IContent[] GetContent()
        {
            return _contents.ToArray();
        }

        public static byte[] ConvertBitmapToPngBytes(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Save the bitmap to the memory stream in PNG format
                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }

        public static void SaveBitmapAsPng(Bitmap bitmap, string filePath)
        {
            bitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
