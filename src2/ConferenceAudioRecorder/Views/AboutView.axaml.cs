using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ConferenceAudioRecorder.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
    }

    private void OnOpenLogsFolder(object sender, RoutedEventArgs e) => App.Controller.OpenLogsFolder();

    private void OnOpenSupportLink(object sender, RoutedEventArgs e) => App.Controller.OpenSupportLink();

    private void OnOpenLicenseLink(object sender, RoutedEventArgs e) => App.Controller.OpenLicenseLink();

    private void OnOpenPrivatePolicyLink(object sender, RoutedEventArgs e) => App.Controller.OpenPrivatePolicyLink();
}
