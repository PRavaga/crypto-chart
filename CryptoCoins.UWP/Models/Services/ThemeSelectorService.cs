using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Models.Services
{
    public static class ThemeSelectorService
    {
        private const string SettingsKey = "RequestedTheme";

        public static event EventHandler<ElementTheme> OnThemeChanged = delegate { };

        public static bool IsLightThemeEnabled => Theme == ElementTheme.Light;
        public static ElementTheme Theme { get; set; }

        public static async Task InitializeAsync()
        {
            Theme = await LoadThemeFromSettingsAsync();
        }

        public static async Task SwitchThemeAsync()
        {
            if (Theme == ElementTheme.Dark)
            {
                await SetThemeAsync(ElementTheme.Light);
            }
            else
            {
                await SetThemeAsync(ElementTheme.Dark);
            }
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;
            SetRequestedTheme();
            await SaveThemeInSettingsAsync(Theme);
        }

        public static void SetRequestedTheme()
        {
            var frameworkElement = Window.Current.Content as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.RequestedTheme = Theme;
                OnThemeChanged(null, Theme);
            }
        }

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            ElementTheme cacheTheme = ElementTheme.Default;
            var themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);
            if (string.IsNullOrEmpty(themeName))
            {
                cacheTheme = ElementTheme.Default;
            }
            else
            {
                Enum.TryParse<ElementTheme>(themeName, out cacheTheme);
            }
            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SettingsKey, theme.ToString());
        }
    }
}
