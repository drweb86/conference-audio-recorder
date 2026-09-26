namespace ConferenceAudioRecorder.Services;

internal interface ICaptureSession
{
    string WaveFile { get; }

    void Stop();
}
