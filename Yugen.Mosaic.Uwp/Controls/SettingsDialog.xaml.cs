using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Yugen.Mosaic.Uwp.ViewModels;
using Yugen.Toolkit.Standard.Mvvm.Input;

namespace Yugen.Mosaic.Uwp.Controls
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private ICommand _hideCommand;

        public SettingsDialog()
        {
            InitializeComponent();

            DataContext = AppContainer.Services.GetService<SettingsViewModel>();
        }

        private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public ICommand HideCommand => _hideCommand ?? (_hideCommand = new RelayCommand(Hide));
    }
}