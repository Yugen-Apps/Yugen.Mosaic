using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yugen.Mosaic.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            ExtendToTitleBar();
        }

        private void ExtendToTitleBar()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private async void SettingsClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SettingsDialog d = new SettingsDialog();
            await d.ShowAsync();
        }
    }
}
