﻿using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Yugen.Toolkit.Standard.Mvvm;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.Services;

namespace Yugen.Mosaic.Uwp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IThemeSelectorService _themeSelectorService;
        private ElementTheme _elementTheme;

        public SettingsViewModel(IThemeSelectorService themeSelectorService)
        {
            _themeSelectorService = themeSelectorService;

            _elementTheme = _themeSelectorService.Theme;

            SwitchThemeCommand = new AsyncRelayCommand<ElementTheme>(SwitchThemeCommandBehavior);
        }

        public string AppVersion => SystemHelper.AppVersion;

        public string Publisher => SystemHelper.Publisher;

        public string RateAndReviewUri => SystemHelper.RateAndReviewUri;

        public ElementTheme ElementTheme
        {
            get => _elementTheme;
            set => SetProperty(ref _elementTheme, value);
        }

        public ICommand SwitchThemeCommand { get; }

        private async Task SwitchThemeCommandBehavior(ElementTheme param)
        {
            ElementTheme = param;
            await _themeSelectorService.SetThemeAsync(param);
        }
    }
}