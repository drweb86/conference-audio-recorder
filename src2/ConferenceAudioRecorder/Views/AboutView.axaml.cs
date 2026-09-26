using Avalonia.Controls;
using Avalonia.Interactivity;
using ConferenceAudioRecorder.Legal;

namespace ConferenceAudioRecorder.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
    }

    private void OnOpenLogsFolder(object sender, RoutedEventArgs e) => App.Controller.OpenLogsFolder();

    private void OnOpenSupportLink(object sender, RoutedEventArgs e) => App.Controller.OpenSupportLink();

    private void OnOpenLicense(object sender, RoutedEventArgs e) => Show(LegalDocumentKind.License);

    private void OnOpenThirdParty(object sender, RoutedEventArgs e) => Show(LegalDocumentKind.ThirdParty);

    private void OnOpenPrivacy(object sender, RoutedEventArgs e) => Show(LegalDocumentKind.Privacy);

    private void Show(LegalDocumentKind kind)
    {
        if (TopLevel.GetTopLevel(this) is MainWindow window)
            window.ShowDocument(kind);
    }
}
