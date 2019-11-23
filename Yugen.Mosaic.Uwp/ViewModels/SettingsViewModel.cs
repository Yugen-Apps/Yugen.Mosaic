using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.ViewModels;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class SettingsViewModel: BaseViewModel
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        private ICommand _switchThemeCommand;

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

        public string AppName
        {
            get
            {
                //This may change if the app gets localized
                return "Yugen Mosaic";
            }
        }
        public string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
        public string Publisher
        {
            get
            {
                var package = Package.Current;
                return package.PublisherDisplayName;
            }
        }
    }
}
