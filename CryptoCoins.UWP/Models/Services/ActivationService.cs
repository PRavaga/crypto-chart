using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform.Activation;
using CryptoCoins.UWP.Views;
using Microsoft.AppCenter.Analytics;
using Microsoft.Practices.ServiceLocation;

namespace CryptoCoins.UWP.Models.Services
{
    //For more information on application activation see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/activation.md
    internal class ActivationService
    {
        private readonly App _app;
        private bool _isInitialized;
        private readonly Type _defaultNavItem;
        private UIElement _shell;
        private NavigationService _navigationService;

        public ActivationService(App app, Type defaultNavItem)
        {
            _app = app;
            _defaultNavItem = defaultNavItem;
        }

        public async Task ActivateAsync(object activationArgs)
        {
            if (IsInteractive(activationArgs) && !_isInitialized)
            {
                // Initialize things like registering background task before the app is loaded
                await InitializeAsync();

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (Window.Current.Content == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    Window.Current.Content = _shell;
                    _navigationService.Frame.NavigationFailed += (sender, e) =>
                    {
                        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
                    };
                    _navigationService.Navigated += OnFrameNavigated;
                    if (SystemNavigationManager.GetForCurrentView() != null)
                    {
                        SystemNavigationManager.GetForCurrentView().BackRequested += OnAppViewBackButtonRequested;
                    }
                }
            }

            var activationHandler = GetActivationHandlers()
                .FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (IsInteractive(activationArgs))
            {
                var defaultHandler = new DefaultLaunchActivationHandler(_defaultNavItem);
                if (defaultHandler.CanHandle(activationArgs))
                {
                    await defaultHandler.HandleAsync(activationArgs);
                }

                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
                // Ensure the current window is active
                Window.Current.Activate();

                // Tasks after activation
                if (!_isInitialized)
                {
                    StartupAsync();
                    _isInitialized = true;
                }
            }
        }

        private async Task InitializeAsync()
        {
            _shell = new ShellPage();
            _navigationService = ServiceLocator.Current.GetInstance<NavigationService>();
            ServiceLocator.Current.GetInstance<AppVersionMigrationService>().MigrateIfNeeded();
            await Singleton<BackgroundTaskService>.Instance.RegisterBackgroundTasks();
            await ThemeSelectorService.InitializeAsync();
            await ServiceLocator.Current.GetInstance<UserPreferencesService>().InitAsync();
        }

        private void StartupAsync()
        {
            ThemeSelectorService.SetRequestedTheme();
            TrackPreferences();
        }

        private void TrackPreferences()
        {
            Task.Run(async () =>
            {
                var prefService = ServiceLocator.Current.GetInstance<UserPreferencesService>();
                var ratesFromCount = prefService.CurrencyPreferences.Count(pref => pref.IsShown);
                var ratesToCount = prefService.ConversionPreferences.Count(pref => pref.IsFeatured);
                Analytics.TrackEvent("Rates", new Dictionary<string, string>
                {
                    {"Total count", (ratesToCount * ratesFromCount).ToString()},
                    {"From count", ratesFromCount.ToString()},
                    {"To count", ratesToCount.ToString()},
                });
                Analytics.TrackEvent("Featured rates", new Dictionary<string, string>
                {
                    {"Count", prefService.ConversionPreferences.Count(pref => pref.IsFeatured).ToString()},
                });
                Analytics.TrackEvent("Display", new Dictionary<string, string>
                {
                    {"Theme", ThemeSelectorService.Theme.ToString()},
                    {"Chart time frame", (prefService.FeaturedPreference.RangeMinutes / 60).ToString()},
                    {"Dashboard rates view mode", prefService.DisplayPreference.DashboardViewMode.ToString()},
                });
                Analytics.TrackEvent("Tiles", new Dictionary<string, string>
                {
                    {"Pinned tiles count", (await SecondaryTile.FindAllAsync().AsTask().ConfigureAwait(false)).Count.ToString()}
                });

                Analytics.TrackEvent("Portfolio",new Dictionary<string,string>
                {
                    {"Holdings count", (await prefService.HoldingsCount()).ToString()}
                });

                Analytics.TrackEvent("Alerts",new Dictionary<string,string>
                {
                    {"Alerts count", (await prefService.AlertsCount()).ToString()}
                });
            });
        }

        private IEnumerable<ActivationHandler> GetActivationHandlers()
        {
            yield return Singleton<LiveTileService>.Instance;
            yield return Singleton<BackgroundTaskService>.Instance;
            yield return Singleton<SuspendAndResumeService>.Instance;
        }

        private bool IsInteractive(object args)
        {
            return args is IActivatedEventArgs;
        }

        private void OnFrameNavigated(object sender, object e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = (_navigationService.CanGoBack) ? 
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnAppViewBackButtonRequested(object sender, BackRequestedEventArgs e)
        {
            if (_navigationService.CanGoBack)
            {
                _navigationService.GoBack();
                e.Handled = true;
            }
        }
    }
}
