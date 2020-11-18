using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.Controls
{
    public sealed partial class WhatsNewDialog
    {
        public WhatsNewDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;

            InitializeComponent();

            DataContext = App.Current.Services.GetService<WhatsNewViewModel>();
        }

        private WhatsNewViewModel ViewModel => (WhatsNewViewModel)DataContext;
    }
}
