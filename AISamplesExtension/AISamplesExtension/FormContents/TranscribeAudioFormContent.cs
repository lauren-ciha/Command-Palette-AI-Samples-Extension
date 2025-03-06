using AdaptiveCards;
using AIDevGallery.Sample.Utils;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace FormContents
{
    public partial class TranscribeAudioFormContent : FormContent, IDisposable
    {
        private AudioACWrapper adaptiveCard;
        private bool isRecording;
        private MediaCapture mediaCapture;
        private InMemoryRandomAccessStream? audioStream;
        private StorageFile? audioFile;
        private WaveInEvent? waveIn;
        private MemoryStream? memoryStream;
        private CancellationTokenSource cts = new();
        private System.Timers.Timer? recordingTimer;
        private WhisperWrapper Whisper => lazyWhisper.Value;
        public event EventHandler<string>? OnSubmit;

        public TranscribeAudioFormContent(AudioACWrapper ac)
        {
            adaptiveCard = ac;
            TemplateJson = adaptiveCard.ToJson();
            mediaCapture = new MediaCapture();
            audioStream = new InMemoryRandomAccessStream();
        }

        private Lazy<WhisperWrapper> lazyWhisper = new Lazy<WhisperWrapper>(() =>
        {
            return WhisperWrapper.CreateAsync(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Models", "whisper_small_int8_cpu_ort_1.18.0.onnx")).Result;
        });

        private async Task StartRecordingAsync()
        {
            try
            {
                memoryStream = new MemoryStream();
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1)
                };
                waveIn.DataAvailable += (s, a) =>
                {
                    memoryStream.Write(a.Buffer, 0, a.BytesRecorded);
                };
                waveIn.StartRecording();

                recordingTimer = new System.Timers.Timer(30000);
                recordingTimer.Elapsed += OnRecordingTimerElapsed;
                recordingTimer.AutoReset = false;
                recordingTimer.Start();

                isRecording = true;
                adaptiveCard.SetRecordButtonTitle("Stop Recording");
                adaptiveCard.SetMarkdownText("Recording... Click 'Stop Recording' to finish.");
                OnSubmit?.Invoke(this, adaptiveCard.ToJson());
            }
            catch (IOException ioEx)
            {
                ShowToastMessage("I/O error starting recording: " + ioEx.Message);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                ShowToastMessage("Access error starting recording: " + uaEx.Message);
            }
            catch (Exception ex)
            {
                ShowToastMessage("Error starting recording: " + ex.Message);
            }
        }

        private async void OnRecordingTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await StopRecordingAsync();
        }

        private async Task StopRecordingAsync()
        {
            try
            {
                recordingTimer?.Stop();
                recordingTimer?.Dispose();
                recordingTimer = null;

                waveIn?.StopRecording();
                waveIn?.Dispose();
                waveIn = null;

                isRecording = false;
                adaptiveCard.SetRecordButtonTitle("Record");

                // Transcribe the audio
                string transcriptionResult = await TranscribeAudio();

                adaptiveCard.SetMarkdownText(transcriptionResult);

                OnSubmit?.Invoke(this, adaptiveCard.ToJson());
            }
            catch (Exception ex)
            {
                ShowToastMessage("Error stopping recording: " + ex.Message);
            }
            finally
            {
                memoryStream?.Dispose();
                memoryStream = null;
            }
        }

        private async Task<string> TranscribeAudio()
        {
            if (memoryStream == null)
            {
                return "No audio recorded";
            }

            var audioData = memoryStream.ToArray();
            var sourceLanguage = "English"; // Assuming English for simplicity
            if (sourceLanguage == null || !WhisperWrapper.LanguageCodes.ContainsKey(sourceLanguage))
            {
                return "Invalid language selected";
            }

            cts = new CancellationTokenSource();

            var transcribedChunks = await Whisper.TranscribeAsync(audioData, sourceLanguage, WhisperWrapper.TaskType.Transcribe, false, cts.Token);

            return transcribedChunks;
        }

        private async Task<StorageFile> SaveAudioToFileAsync()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            var file = await storageFolder.CreateFileAsync("recordedAudio.mp3", CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                if (audioStream == null)
                {
                    throw new InvalidOperationException("Audio stream is null.");
                }
                audioStream.Seek(0);
                await RandomAccessStream.CopyAndCloseAsync(audioStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
            }

            return file;
        }

#pragma warning disable CA1822 // Mark members as static
        private void ShowToastMessage(string message)
#pragma warning restore CA1822 // Mark members as static
        {
            var status = new StatusMessage
            {
                Message = message,
                State = MessageState.Error,
            };
            var toast = new ToastStatusMessage(status);
            toast.Show();
        }

        public override ICommandResult SubmitForm(string inputs, string data)
        {
            var actionData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(data);
            var action = actionData != null ?? actionData["action"];

            if (action)
            {
                if (isRecording)
                {
                    _ = StopRecordingAsync();
                }
                else
                {
                    _ = StartRecordingAsync();
                }
            }

            return CommandResult.KeepOpen();
        }

        public void Dispose()
        {
            if (audioStream != null)
            {
                audioStream.Dispose();
                audioStream = null;
            }
            cts?.Cancel();
            cts?.Dispose();
            Whisper?.Dispose();
            waveIn?.Dispose();
            memoryStream?.Dispose();
            recordingTimer?.Stop();
            recordingTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
