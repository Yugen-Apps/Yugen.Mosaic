using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Yugen.Mosaic.Uwp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            TitleBarHelper.ExtendToTitleBar();
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

            ViewModel.InitOnboarding(frameworkElements);
        }
    }
}