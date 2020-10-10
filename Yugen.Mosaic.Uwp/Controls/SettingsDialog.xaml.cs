using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Yugen.Mosaic.Uwp.ViewModels;

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

        public ICommand HideCommand => _hideCommand ?? (_hideCommand = new RelayCommand(Hide));

        private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    }
}