using Microsoft.Extensions.DependencyInjection;
using Yugen.Mosaic.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.Views.Dialogs
{
    public sealed partial class SettingsDialog
    {
        public SettingsDialog()
        {
            InitializeComponent();

            DataContext = App.Current.Services.GetService<SettingsViewModel>();
        }

        private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;
    }
}