using System;
using System.Collections.Generic;

namespace ConferenceAudioRecorder.Services;

internal interface IAudioBackend : IDisposable
{
    IReadOnlyList<string> GetInputDevices();

    IReadOnlyList<string> GetOutputDevices();

    string GetDefaultInputDevice();

    string GetDefaultOutputDevice();

    event EventHandler DevicesChanged;

    ICaptureSession StartInputCapture(string deviceName, int sampleRate, int channels);

    ICaptureSession StartOutputCapture(string deviceName, int sampleRate, int channels);
}
