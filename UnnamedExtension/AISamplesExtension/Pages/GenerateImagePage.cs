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
using System.Text.Json;
using UnnamedExtension.Templates;

namespace Pages
{
    internal sealed partial class GenerateImagePage : ContentPage, IDisposable
    {
        private TextFormContent _textFormContent;
        private List<IContent> _contents;
        private TemplateLoader _templateLoader = new();
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

            Task.Run(() => LoadStableDiffusion());
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
            if (modelReady == false)
            {
                IsLoading = false;
                var statusMessage = new StatusMessage
                {
                    Message = "Model is not ready. Please try again.",
                    State = MessageState.Info,
                };
                ToastStatusMessage toast = new ToastStatusMessage(statusMessage);
                toast.Show();
                return;
            }

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
                        _contents.Insert(1, response);
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
            var stopwatch = Stopwatch.StartNew();
            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Generating image with prompt: {inputs}");
            if (!modelReady || isCanceling)
            {
                Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Model is not ready or is canceling.");
                return;
            }

            if (inferenceTask != null)
            {
                Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Inference task is already running. Canceling the previous task.");
                cts.Cancel();
                isCanceling = true;
                await inferenceTask;
                isCanceling = false;
                return;
            }

            if (stableDiffusion == null)
            {
                Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - StableDiffusion is not initialized.");
                return;
            }
            if (string.IsNullOrEmpty(inputs))
            {
                Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Prompt is empty.");
                return;
            }
            prompt = inputs;

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, timeoutCts.Token);
            CancellationToken token = linkedCts.Token;

            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Token received. Prompt: {prompt}");
            inferenceTask = Task.Run(
                async () =>
                {
                    try
                    {
                        var res = stableDiffusion!.Inference(prompt, token);
                        Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Inference result: {res}");
                        if (res is Bitmap image)
                        {
                            string filePath = System.IO.Path.Join(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", $"{prompt}.png");
                            SaveBitmapAsPng(image, filePath);
                            genImagePath = filePath;
                        }
                        else
                        {
                            throw new ArgumentException("The inference did not return a valid image.");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        if (timeoutCts.IsCancellationRequested)
                        {
                            var statusMessage = new StatusMessage
                            {
                                Message = "Image generation timed out. Please try again.",
                                State = MessageState.Info,
                            };
                            ToastStatusMessage toast = new ToastStatusMessage(statusMessage);
                            toast.Show();
                        }
                        else
                        {
                            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Inference task was canceled.");
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex);
                    }
                },
                token);

            await inferenceTask;
            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Inference task completed. Setting to null");
            inferenceTask = null;
            stopwatch.Stop();
            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - DoStableDiffusion completed in {stopwatch.ElapsedMilliseconds} ms");
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
