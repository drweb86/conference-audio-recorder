using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ConferenceAudioRecorder.Views;

public partial class RecordingView : UserControl
{
    private readonly RecordingViewModel _model = new();

    public RecordingView()
    {
        InitializeComponent();
        DataContext = _model;
        InputPopup.DataContext = _model;
        OutputPopup.DataContext = _model;
        _model.Refresh();
    }

    public void RefreshProperties() => _model.Refresh();

    public bool FocusRecordButton() => RecordButton.Focus();

    private async void OnRecordClick(object sender, RoutedEventArgs e)
    {
        if (_model.IsBusy)
            return;

        try
        {
            if (App.Controller.IsAudioRecording)
            {
                _model.IsBusy = true;
                await Task.Run(App.Controller.Stop);
            }
            else
            {
                App.Controller.Start();
            }
        }
        catch (Exception ex)
        {
            App.Controller.Services.Log.Error(ex);
            await MessageDialog.Show(this, ex.Message, Localization.Strings.Get("GotIt"));
        }
        finally
        {
            _model.IsBusy = false;
            _model.NotifyRecordingState();
        }
    }

    private void OnSeeRecordingsClick(object sender, RoutedEventArgs e)
    {
        App.Controller.OpenOutputFolder();
    }

    private async void OnShowHelp(object sender, RoutedEventArgs e)
    {
        foreach (var key in new[] { "Help1", "Help2", "Help3", "Help4" })
        {
            await MessageDialog.Show(this, Localization.Strings.Get(key), Localization.Strings.Get("GotIt"));
        }
    }

    private void OnToggleInput(object sender, RoutedEventArgs e)
    {
        _model.AudioInputDevice = App.Controller.ToggleAudioInputDevice();
    }

    private void OnToggleOutput(object sender, RoutedEventArgs e)
    {
        _model.AudioOutputDevice = App.Controller.ToggleAudioOutputDevice();
    }

    private void OnOpenInputMenu(object sender, RoutedEventArgs e)
    {
        InputPopup.IsOpen = !InputPopup.IsOpen;
    }

    private void OnOpenOutputMenu(object sender, RoutedEventArgs e)
    {
        OutputPopup.IsOpen = !OutputPopup.IsOpen;
    }

    private void OnInputSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!_model.IsRefreshing)
            InputPopup.IsOpen = false;
    }

    private void OnOutputSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!_model.IsRefreshing)
            OutputPopup.IsOpen = false;
    }
}

public sealed class RecordingViewModel : INotifyPropertyChanged
{
    private bool _refreshing;
    private bool _busy;
    private List<string> _inputDevices;
    private List<string> _outputDevices;
    private string _audioInputDevice;
    private string _audioOutputDevice;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsRefreshing => _refreshing;

    public bool IsAudioRecording => App.Controller.IsAudioRecording;

    public bool IsBusy
    {
        get => _busy;
        set
        {
            _busy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAudioRecordingEnabled));
            OnPropertyChanged(nameof(IsAudioInputDeviceEnabled));
            OnPropertyChanged(nameof(IsAudioOutputDeviceEnabled));
        }
    }

    public bool IsAudioInputDeviceEnabled =>
        !IsAudioRecording && !IsBusy && InputDevices != null && InputDevices.Count > 0;

    public bool IsAudioOutputDeviceEnabled =>
        !IsAudioRecording && !IsBusy && OutputDevices != null && OutputDevices.Count > 0;

    public bool IsAudioRecordingEnabled =>
        !IsBusy && (AudioInputDevice != null || AudioOutputDevice != null);

    public List<string> InputDevices
    {
        get => _inputDevices;
        set
        {
            _inputDevices = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAudioInputDeviceEnabled));
        }
    }

    public List<string> OutputDevices
    {
        get => _outputDevices;
        set
        {
            _outputDevices = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAudioOutputDeviceEnabled));
        }
    }

    public string AudioInputDevice
    {
        get => _audioInputDevice;
        set
        {
            if (_audioInputDevice == value)
                return;

            _audioInputDevice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAudioRecordingEnabled));
            if (!_refreshing)
                App.Controller.UpdateAudioInputDevice(value);
        }
    }

    public string AudioOutputDevice
    {
        get => _audioOutputDevice;
        set
        {
            if (_audioOutputDevice == value)
                return;

            _audioOutputDevice = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAudioRecordingEnabled));
            if (!_refreshing)
                App.Controller.UpdateAudioOutputDevice(value);
        }
    }

    public void Refresh()
    {
        _refreshing = true;
        try
        {
            InputDevices = App.Controller.Model.InputDevices;
            _audioInputDevice = App.Controller.Model.Settings.AudioInputDevice;
            OnPropertyChanged(nameof(AudioInputDevice));

            OutputDevices = App.Controller.Model.OutputDevices;
            _audioOutputDevice = App.Controller.Model.Settings.AudioOutputDevice;
            OnPropertyChanged(nameof(AudioOutputDevice));
            NotifyRecordingState();
        }
        finally
        {
            _refreshing = false;
        }
    }

    public void NotifyRecordingState()
    {
        OnPropertyChanged(nameof(IsAudioRecording));
        OnPropertyChanged(nameof(IsAudioRecordingEnabled));
        OnPropertyChanged(nameof(IsAudioInputDeviceEnabled));
        OnPropertyChanged(nameof(IsAudioOutputDeviceEnabled));
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
