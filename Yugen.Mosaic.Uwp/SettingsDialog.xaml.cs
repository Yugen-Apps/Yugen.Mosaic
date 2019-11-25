using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yugen.Mosaic.Uwp
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            this.InitializeComponent();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}
