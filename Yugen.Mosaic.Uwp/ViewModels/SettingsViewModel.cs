using System;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Standard.Commands;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class SettingsViewModel: BaseViewModel
    {
        private const string STORE_REVIEWFORMAT = "ms-windows-store:REVIEW?PFN={0}";
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private ICommand _switchThemeCommand;
        private ICommand _launchRateAndReviewCommand;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }
            set { Set(ref _elementTheme, value); }
        }
        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }
                return _switchThemeCommand;
            }
        }
        public ICommand LaunchRateAndReviewCommand
        {
            get
            {
                if (_launchRateAndReviewCommand == null)
                {
                    _launchRateAndReviewCommand = new RelayCommand(async () =>
                    {
                        await Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format(STORE_REVIEWFORMAT, Package.Current.Id.FamilyName)));
                    });
                }
                return _launchRateAndReviewCommand;
            }
        }

        //This may change if the app gets localized
        public string AppName => "Yugen Mosaic";

        public string AppVersion => SystemHelper.AppVersion;

        public string Publisher => SystemHelper.Publisher;

        public string[] Collaborator => new[] { "Leisvan", "Yoshi" };
    }
}
