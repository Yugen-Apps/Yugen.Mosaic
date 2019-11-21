using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yugen.Mosaic.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPageConcept : Page
    {
        public MainViewModel ViewModel { get; set; } = new MainViewModel();
        public MainPageConcept()
        {
            this.InitializeComponent();
            ExtendToTitleBar();
        }

        private void ExtendToTitleBar()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //Active:
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;

            //Inactive:
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AddMasterUI.Visibility = Visibility.Visible;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AddMasterUI.Visibility = (ViewModel.MasterBpmSource != null) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.AddMasterButton_Click(sender, e);
            AddMasterUI.Visibility = Visibility.Collapsed;
        }

    }
}
