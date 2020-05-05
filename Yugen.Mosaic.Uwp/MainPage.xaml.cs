using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Yugen.Mosaic.Uwp.Helpers;

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
            InitializeComponent();
            ExtendToTitleBar();
        }

        private void ExtendToTitleBar()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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

            OnboardingHelper.Init(frameworkElements);

            ViewModel.ShowTeachingTip();
        }
    }
}
