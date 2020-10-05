using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Yugen.Mosaic.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            TitleBarHelper.ExtendToTitleBar();

            DataContext = AppContainer.Services.GetService<MainViewModel>();
        }

        private MainViewModel ViewModel => (MainViewModel)DataContext;

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            var frameworkElements = new FrameworkElement[]
            {
                MasterImageGrid,
                AddTilesButton,
                TilePropertiesGrid,
                MosaicTypeComboBox,
                OutputPropertiesGrid,
                GenerateButton,
                SaveButton
            };

            ViewModel.InitOnboarding(frameworkElements);
        }
    }
}