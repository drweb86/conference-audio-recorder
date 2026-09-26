using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ConferenceAudioRecorder.Views;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _model = new();

    public SettingsView()
    {
        InitializeComponent();
        DataContext = _model;
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
}

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private string _saveRecordingToFolder = App.Controller.Model.Settings.SaveRecordingToFolder;

    public event PropertyChangedEventHandler PropertyChanged;

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

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
