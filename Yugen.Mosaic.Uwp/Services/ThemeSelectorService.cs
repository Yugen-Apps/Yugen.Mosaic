﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Toolkit.Standard.Helpers;

namespace Yugen.Mosaic.Uwp.Services
{
    public static class ThemeSelectorService
    {
        private const string SettingsKey = "AppBackgroundRequestedTheme";
        private const string DARK_THEME_BCKG = "#FF000000";
        private const string LIGHT_THEME_BCKG = "#FFFFFFFF";

        public static ElementTheme Theme { get; set; } = ElementTheme.Default;

        public static async Task InitializeAsync()
        {
            var theme = await LoadThemeFromSettingsAsync();
            await SetThemeAsync(theme);
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
            await SaveThemeInSettingsAsync(Theme);

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //Active
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = GetThemeResource<Color>(theme, "TitleBarButtonForeground");
            titleBar.ButtonHoverBackgroundColor = GetThemeResource<Color>(theme, "TitleBarButtonHoverBackground");
            titleBar.ButtonHoverForegroundColor = GetThemeResource<Color>(theme, "TitleBarButtonHoverForeground");

            //Inactive
            titleBar.InactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = GetThemeResource<Color>(theme, "TitleBarButtonForeground");
        }

        public static async Task SetRequestedThemeAsync()
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = Theme;
                    }
                });
            }
        }

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            ElementTheme cacheTheme = ElementTheme.Default;
            string themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, theme.ToString());
        }

        private static T GetThemeResource<T>(ElementTheme theme, string resKey)
        {
            bool isLightTheme = (theme == ElementTheme.Default)
                ? (IsSystemThemeLight())
                : (theme == ElementTheme.Light);
            string themeKey = isLightTheme ? "Light" : "Dark";
            var themeDictionary = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[themeKey];
            return (T)themeDictionary[resKey];
        }

        private static bool IsSystemThemeLight()
        {
            var DefaultTheme = new UISettings();
            var uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
            return uiTheme == LIGHT_THEME_BCKG;
        }
    }
}