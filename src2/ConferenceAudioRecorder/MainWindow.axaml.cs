using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ConferenceAudioRecorder.Legal;
using ConferenceAudioRecorder.Localization;
using ConferenceAudioRecorder.Views;

namespace ConferenceAudioRecorder;

public partial class MainWindow : Window
{
    private bool _navFocusReady;

    public MainWindow()
    {
        InitializeComponent();
        Title = Strings.Get("ConferenceAudioRecorder");
        PageHost.Content = new RecordingView();
        NavPane.AddHandler(InputElement.GotFocusEvent, OnNavGotFocus, RoutingStrategies.Bubble, true);
        NavPane.AddHandler(InputElement.LostFocusEvent, OnNavLostFocus, RoutingStrategies.Bubble, true);
        App.Controller.UpdatedAudioDevices += OnUpdatedAudioDevices;
        Closed += OnClosed;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        FocusRecording();
        _navFocusReady = true;
    }

    public void ShowAbout()
    {
        AboutNav.IsChecked = true;
        if (PageHost.Content is AboutView)
            return;

        PageHost.Content = new AboutView();
    }

    public void ShowDocument(LegalDocumentKind kind)
    {
        AboutNav.IsChecked = true;
        PageHost.Content = new LegalDocumentView(kind);
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
        {
            FocusRecording();
            return;
        }

        PageHost.Content = new RecordingView();
        Dispatcher.UIThread.Post(FocusRecording, DispatcherPriority.Input);
    }

    private void OnShowSettings(object sender, RoutedEventArgs e)
    {
        if (PageHost.Content is SettingsView)
            return;

        PageHost.Content = new SettingsView();
    }

    private void OnShowAbout(object sender, RoutedEventArgs e)
    {
        ShowAbout();
    }

    private void OnNavGotFocus(object sender, FocusChangedEventArgs e)
    {
        if (_navFocusReady)
            SetNavExpanded(true);
    }

    private void OnNavLostFocus(object sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
            var inside = focused != null && NavPane.IsVisualAncestorOf(focused);
            SetNavExpanded(inside);
        }, DispatcherPriority.Input);
    }

    private void SetNavExpanded(bool expanded)
    {
        NavPane.Width = expanded ? 188 : 56;
        RecordingLabel.IsVisible = expanded;
        SettingsLabel.IsVisible = expanded;
        AboutLabel.IsVisible = expanded;
    }

    private void FocusRecording()
    {
        if (PageHost.Content is RecordingView recording && recording.FocusRecordButton())
            return;

        Focus();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        App.Controller.UpdatedAudioDevices -= OnUpdatedAudioDevices;
        App.Controller.Dispose();
    }
}
