using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Yugen.Mosaic.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.Controls
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        private ICommand _hideCommand;

        public WhatsNewDialog()
        {
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;

            InitializeComponent();

            DataContext = AppContainer.Services.GetService<WhatsNewViewModel>();
        }

        public ICommand HideCommand => _hideCommand ?? (_hideCommand = new RelayCommand(Hide));

        private WhatsNewViewModel ViewModel => (WhatsNewViewModel)DataContext;
    }
}
