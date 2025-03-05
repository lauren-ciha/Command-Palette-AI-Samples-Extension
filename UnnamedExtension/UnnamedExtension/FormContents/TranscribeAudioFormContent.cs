using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Threading.Tasks;
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

        public TranscribeAudioFormContent(AudioACWrapper ac)
        {
            adaptiveCard = ac;
            TemplateJson = adaptiveCard.ToJson();
            mediaCapture = new MediaCapture();
            audioStream = new InMemoryRandomAccessStream();
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                adaptiveCard.SetRecordButtonTitle("Stop Recording");
                var settings = new MediaCaptureInitializationSettings
                {
                    StreamingCaptureMode = StreamingCaptureMode.Audio
                };

                await mediaCapture.InitializeAsync(settings);

                var profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
                await mediaCapture.StartRecordToStreamAsync(profile, audioStream);

                isRecording = true;
                adaptiveCard.SetRecordButtonTitle("Stop Recording");
                adaptiveCard.SetMarkdownText("Recording... Click 'Stop Recording' to finish.");
            }
            catch (Exception ex)
            {
                ShowToastMessage("Error starting recording: " + ex.Message);
            }
        }

        private async Task StopRecordingAsync()
        {
            try
            {
                await mediaCapture.StopRecordAsync();
                isRecording = false;
                adaptiveCard.SetRecordButtonTitle("Record");

                // Save the audio to a file
                audioFile = await SaveAudioToFileAsync();

                // Transcribe the audio (this is a placeholder, replace with actual transcription logic)
                string transcriptionResult = "Transcription result goes here.";

                adaptiveCard.SetMarkdownText(transcriptionResult);

                // Pass the file path to the OnSubmit() function
                SubmitForm(audioFile.Path);
            }
            catch (Exception ex)
            {
                ShowToastMessage("Error stopping recording: " + ex.Message);
            }
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
            GC.SuppressFinalize(this);
        }
    }
}
