using System;
using ConferenceAudioRecorder.Commands;
using ConferenceAudioRecorder.Model;
using ConferenceAudioRecorder.Services;

namespace ConferenceAudioRecorder.Controller;

internal class AudioRecorderToolController : IDisposable
{
    private readonly object _gate = new();
    private bool _disposed;

    public readonly ServiceContainer Services;
    public readonly AudioRecorderToolModel Model;
    public EventHandler UpdatedAudioDevices;

    public AudioRecorderToolController(HDE.Platform.Logging.ILog log)
    {
        Model = new AudioRecorderToolModel();
        Services = new ServiceContainer(log);
        Services.Audio.DevicesChanged += OnAudioDeviceChanged;
    }

    public void Initialize()
    {
        lock (_gate)
        {
            new InitializeCommand().Execute(this);
        }

        UpdatedAudioDevices?.Invoke(this, EventArgs.Empty);
    }

    public void Start()
    {
        lock (_gate)
        {
            if (IsAudioRecording)
                return;

            Services.AudioRecorderService.StartRecording(
                Model.Settings.AudioInputDevice,
                Model.Settings.AudioOutputDevice,
                RecordingProfile.FromSettings(Model.Settings));
        }
    }

    public bool IsAudioRecording => Services.AudioRecorderService.IsAudioRecording;

    public void Stop()
    {
        lock (_gate)
        {
            if (!IsAudioRecording)
                return;

            Services.AudioRecorderService.StopRecording(Model.Settings.SaveRecordingToFolder);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Services.Audio.DevicesChanged -= OnAudioDeviceChanged;
        if (IsAudioRecording)
            Stop();

        Services.Dispose();
    }

    public void OpenOutputFolder() => new OpenOutputFolderCommand().Execute(this);

    public void OpenLogsFolder() => new OpenLogsFolderCommand().Execute(this);

    public void OpenSupportLink() => new OpenSupportLinkCommand().Execute(this);

    public void OpenLicenseLink() => new OpenLicenseLinkCommand().Execute(this);

    public void OpenPrivatePolicyLink() => new OpenPrivatePolicyLinkCommand().Execute(this);

    public void UpdateAudioInputDevice(string audioInputDevice)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateAudioInputDevice(this, audioInputDevice);
    }

    public void UpdateAudioOutputDevice(string audioOutputDevice)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateAudioOutputDevice(this, audioOutputDevice);
    }

    public void UpdateSaveRecordingToFolder(string saveRecordingToFolder)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateSaveRecordingToFolder(this, saveRecordingToFolder);
    }

    public void UpdateRecordingSampleRate(int sampleRate)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateRecordingSampleRate(this, sampleRate);
    }

    public void UpdateEncodingBitRate(int bitRate)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateEncodingBitRate(this, bitRate);
    }

    public void UpdateTheme(string theme)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateTheme(this, theme);
    }

    public void UpdateDocumentLanguage(string languageCode)
    {
        lock (_gate)
            new UpdateSettingsCommand().UpdateDocumentLanguage(this, languageCode);
    }

    public string ToggleAudioInputDevice()
    {
        lock (_gate)
            return new ToggleAudioInputDeviceCommand().Execute(this);
    }

    public string ToggleAudioOutputDevice()
    {
        lock (_gate)
            return new ToggleAudioOutputDeviceCommand().Execute(this);
    }

    private void OnAudioDeviceChanged(object sender, EventArgs e)
    {
        Initialize();
    }
}
