using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Yugen.Mosaic.Uwp.Extensions;

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
            ElementTheme theme = await LoadThemeFromSettingsAsync();
            await SetThemeAsync(theme);
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
            await SaveThemeInSettingsAsync(Theme);

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

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
            foreach (CoreApplicationView view in CoreApplication.Views)
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
            var themeName = await SettingsHelper.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme) => await SettingsHelper.WriteAsync(SettingsKey, theme.ToString());

        private static T GetThemeResource<T>(ElementTheme theme, string resKey)
        {
            var isLightTheme = (theme == ElementTheme.Default)
                ? (IsSystemThemeLight())
                : (theme == ElementTheme.Light);
            var themeKey = isLightTheme ? "Light" : "Dark";
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
