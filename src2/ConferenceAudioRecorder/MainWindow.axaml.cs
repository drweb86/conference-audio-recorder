using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ConferenceAudioRecorder.Localization;
using ConferenceAudioRecorder.Views;

namespace ConferenceAudioRecorder;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = Strings.Get("ConferenceAudioRecorder");
        PageHost.Content = new RecordingView();
        App.Controller.UpdatedAudioDevices += OnUpdatedAudioDevices;
        Closed += OnClosed;
    }

    private void OnUpdatedAudioDevices(object sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (PageHost.Content is RecordingView recording)
                recording.RefreshProperties();
        });
    }

    private void OnShowRecording(object sender, RoutedEventArgs e)
    {
        if (PageHost.Content is RecordingView)
            return;

        PageHost.Content = new RecordingView();
    }

    private void OnShowSettings(object sender, RoutedEventArgs e)
    {
        if (PageHost.Content is SettingsView)
            return;

        PageHost.Content = new SettingsView();
    }

    private void OnShowAbout(object sender, RoutedEventArgs e)
    {
        if (PageHost.Content is AboutView)
            return;

        PageHost.Content = new AboutView();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        App.Controller.UpdatedAudioDevices -= OnUpdatedAudioDevices;
        App.Controller.Dispose();
    }
}
