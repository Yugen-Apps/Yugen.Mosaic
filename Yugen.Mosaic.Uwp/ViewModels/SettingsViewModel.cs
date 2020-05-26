using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Standard.Commands;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private ICommand _switchThemeCommand;

        public string AppVersion => SystemHelper.AppVersion;
        public string Publisher => SystemHelper.Publisher;
        public string RateAndReviewUri => SystemHelper.RateAndReviewUri;

        public ElementTheme ElementTheme
        {
            get => _elementTheme;
            set => Set(ref _elementTheme, value);
        }

        public ICommand SwitchThemeCommand => _switchThemeCommand ?? (_switchThemeCommand = new AsyncRelayCommand<ElementTheme>(SwitchThemeCommandBehavior));

        private async Task SwitchThemeCommandBehavior(ElementTheme param)
        {
            ElementTheme = param;
            await ThemeSelectorService.SetThemeAsync(param);
        }
    }
}