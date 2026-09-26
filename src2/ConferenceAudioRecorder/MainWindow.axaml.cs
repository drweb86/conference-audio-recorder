using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using ConferenceAudioRecorder.Legal;
using ConferenceAudioRecorder.Localization;
using ConferenceAudioRecorder.Views;

namespace ConferenceAudioRecorder;

public partial class MainWindow : Window
{
    private bool _navExpanded;

    public MainWindow()
    {
        InitializeComponent();
        Title = Strings.Get("ConferenceAudioRecorder");
        PageHost.Content = new RecordingView();
        App.Controller.UpdatedAudioDevices += OnUpdatedAudioDevices;
        Closed += OnClosed;
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
        ShowAbout();
    }

    private void OnToggleNav(object sender, RoutedEventArgs e)
    {
        SetNavExpanded(!_navExpanded);
    }

    private void SetNavExpanded(bool expanded)
    {
        _navExpanded = expanded;
        NavPane.Classes.Set("collapsed", !expanded);
        NavPane.Width = expanded ? 220 : 56;
        RecordingLabel.IsVisible = expanded;
        SettingsLabel.IsVisible = expanded;
        AboutLabel.IsVisible = expanded;
        NavToggleIcon.Data = expanded ? AppIcons.ChevronLeft : AppIcons.ChevronRight;

        var contentAlignment = expanded ? HorizontalAlignment.Left : HorizontalAlignment.Center;
        var itemAlignment = expanded ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
        var compact = !expanded;
        NavToggle.HorizontalAlignment = itemAlignment;
        NavToggle.HorizontalContentAlignment = contentAlignment;
        NavToggle.Classes.Set("compact", compact);
        RecordingNav.HorizontalAlignment = itemAlignment;
        RecordingNav.HorizontalContentAlignment = contentAlignment;
        RecordingNav.Classes.Set("compact", compact);
        SettingsNav.HorizontalAlignment = itemAlignment;
        SettingsNav.HorizontalContentAlignment = contentAlignment;
        SettingsNav.Classes.Set("compact", compact);
        AboutNav.HorizontalAlignment = itemAlignment;
        AboutNav.HorizontalContentAlignment = contentAlignment;
        AboutNav.Classes.Set("compact", compact);
        RecordingItem.HorizontalAlignment = contentAlignment;
        SettingsItem.HorizontalAlignment = contentAlignment;
        AboutItem.HorizontalAlignment = contentAlignment;
    }

    private void OnClosed(object sender, EventArgs e)
    {
        App.Controller.UpdatedAudioDevices -= OnUpdatedAudioDevices;
        App.Controller.Dispose();
    }
}
