using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Standard.Commands;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private const string STORE_REVIEWFORMAT = "ms-windows-store:REVIEW?PFN={0}";
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private ICommand _launchRateAndReviewCommand;
        private ICommand _switchThemeCommand;
        //This may change if the app gets localized
        public string AppName => "Yugen Mosaic";

        public string AppVersion => SystemHelper.AppVersion;

        public string[] Collaborator => new[] { "Leisvan", "Yoshi" };

        public ElementTheme ElementTheme
        {
            get => _elementTheme;
            set => Set(ref _elementTheme, value);
        }

        public string Publisher => SystemHelper.Publisher;

        public ICommand LaunchRateAndReviewCommand => _launchRateAndReviewCommand ?? (_launchRateAndReviewCommand = new AsyncRelayCommand(LaunchRateAndReviewCommandBehavior));
        public ICommand SwitchThemeCommand => _switchThemeCommand ?? (_switchThemeCommand = new AsyncRelayCommand<ElementTheme>(SwitchThemeCommandBehavior));

        private async Task LaunchRateAndReviewCommandBehavior() => await Launcher.LaunchUriAsync(new Uri(string.Format(STORE_REVIEWFORMAT, Package.Current.Id.FamilyName)));

        private async Task SwitchThemeCommandBehavior(ElementTheme param)
        {
            ElementTheme = param;
            await ThemeSelectorService.SetThemeAsync(param);
        }
    }
}