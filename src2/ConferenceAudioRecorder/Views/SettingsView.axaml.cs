using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ConferenceAudioRecorder.Localization;

namespace ConferenceAudioRecorder.Views;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _model = new();

    public SettingsView()
    {
        InitializeComponent();
        DataContext = _model;
        ThemeBox.ItemsSource = _model.Themes;
        ThemeBox.SelectedItem = _model.SelectedTheme;
    }

    private void OnOpenRecordingsFolder(object sender, RoutedEventArgs e)
    {
        App.Controller.OpenOutputFolder();
    }

    private async void OnChangeFolder(object sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top == null)
            return;

        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = Localization.Strings.Get("ChangeFolder")
        });

        if (folders.Count == 0)
            return;

        var path = folders[0].TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path))
            _model.SaveRecordingToFolder = path;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeBox.SelectedItem is ThemeChoice choice)
            _model.SelectedTheme = choice;
    }
}

public sealed class ThemeChoice
{
    public ThemeChoice(string id, string label)
    {
        Id = id;
        Label = label;
    }

    public string Id { get; }
    public string Label { get; }

    public override string ToString() => Label;
}

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private string _saveRecordingToFolder = App.Controller.Model.Settings.SaveRecordingToFolder;
    private ThemeChoice _selectedTheme;
    private bool _ready;

    public event PropertyChangedEventHandler PropertyChanged;

    public IReadOnlyList<ThemeChoice> Themes { get; } =
    [
        new ThemeChoice(ThemeApplicator.Dark, Strings.Get("ThemeDark")),
        new ThemeChoice(ThemeApplicator.Light, Strings.Get("ThemeLight")),
        new ThemeChoice(ThemeApplicator.FollowSystem, Strings.Get("ThemeSystem"))
    ];

    public ThemeChoice SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (value == null || value == _selectedTheme)
                return;

            _selectedTheme = value;
            OnPropertyChanged();
            if (_ready)
            {
                ThemeApplicator.Apply(value.Id);
                App.Controller.UpdateTheme(value.Id);
            }
        }
    }

    public string SaveRecordingToFolder
    {
        get => _saveRecordingToFolder;
        set
        {
            if (_saveRecordingToFolder == value)
                return;

            _saveRecordingToFolder = value;
            OnPropertyChanged();
            App.Controller.UpdateSaveRecordingToFolder(value);
        }
    }

    public SettingsViewModel()
    {
        var saved = App.Controller.Model.Settings.Theme;
        if (string.IsNullOrWhiteSpace(saved))
            saved = ThemeApplicator.Dark;

        foreach (var theme in Themes)
        {
            if (string.Equals(theme.Id, saved, System.StringComparison.OrdinalIgnoreCase))
            {
                _selectedTheme = theme;
                break;
            }
        }

        _selectedTheme ??= Themes[0];
        _ready = true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
