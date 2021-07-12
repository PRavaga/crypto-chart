using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Exceptions.Sync;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Sync;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.ViewModels.Common;
using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Services.Core;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public const int RangeDay = 60 * 24;
        public const int RangeWeek = RangeDay * 7;
        public const int RangeMonth = RangeDay * 30;
        public const int Range3Months = RangeMonth * 3;
        public const int Range6Months = RangeMonth * 6;
        public const int RangeYear = RangeDay * 365;
        private const string SystemDisplayLanguage = "SystemDisplayLanguage";
        private readonly SyncService _syncService;
        private readonly SyncBackgroundTask _syncBackgroundTask;

        

        private readonly UserPreferencesService _preferencesService;

        private string _appDescription;

        private RelayCommand _contactDeveloperCommand;
        private ObservableCollection<CurrencyPreference> _currencies;

        private RelayCommand _giveFeedbackCommand;


        private DescriptionWrapper<string> _selectedLanguage;
        private ElementTheme _theme;

        public SettingsViewModel(UserPreferencesService preferencesService, SyncService syncService, SyncBackgroundTask syncBackgroundTask)
        {
            _preferencesService = preferencesService;
            _syncService = syncService;
            _syncBackgroundTask = syncBackgroundTask;
            var package = Package.Current;
            AppDescription = $"{package.DisplayName} - {AppInfoHelper.GetAppVersion()}";
        }

        

        public List<DescriptionWrapper<string>> Languages { get; } = Enumerable
            .Repeat(new DescriptionWrapper<string> {Description = "SettingsPage_SystemDisplayLanguage".GetLocalized(), Value = SystemDisplayLanguage}, 1).Concat(
                ApplicationLanguages
                    .ManifestLanguages
                    .Select(code => new DescriptionWrapper<string> {Value = code, Description = new Language(code).DisplayName})).ToList();

        public DescriptionWrapper<string> SelectedLanguage
        {
            get => _selectedLanguage;
            set => Set(ref _selectedLanguage, value);
        }

        public ObservableCollection<CurrencyPreference> Currencies
        {
            get => _currencies;
            set => Set(ref _currencies, value);
        }

        public ElementTheme Theme
        {
            get => _theme;
            set => Set(ref _theme, value);
        }

        public string AppDescription
        {
            get => _appDescription;
            set => Set(ref _appDescription, value);
        }

        private string _syncStatus;

        public string SyncStatus
        {
            get => _syncStatus;
            set => Set(ref _syncStatus, value);
        }

        private bool _isAutoSyncEnabled;

        public bool IsAutoSyncEnabled
        {
            get => _isAutoSyncEnabled;
            set => Set(ref _isAutoSyncEnabled, value);
        }

        private AsyncCommand _syncNowCommand;

        public AsyncCommand SyncNowCommand => _syncNowCommand ?? (_syncNowCommand = new AsyncCommand(async () =>
        {
            try
            {
                SyncStatus = string.Format("SettingsPage_SyncStatus_InProgress".GetLocalized());
                await _syncService.Sync();
                SyncStatus = string.Format("SettingsPage_SyncStatus_Succed".GetLocalized(), Views.Formatter.DateTime.TimeAgo(_syncService.LastSyncDate.Value.LocalDateTime, false));
            }
            catch (SyncException e)
            {
                Logger.Error("Can't sync", e);
                SyncStatus = string.Format("SettingsPage_SyncStatus_Failed".GetLocalized(), DateTime.Now);
            }
        }));

        public RelayCommand ContactDeveloperCommand => _contactDeveloperCommand ?? (_contactDeveloperCommand = new RelayCommand(async () =>
        {
            var emailMessage = new EmailMessage();
            var emailRecipient = new EmailRecipient("hello@ravaga.com");
            emailMessage.To.Add(emailRecipient);

            try
            {
                var dbFile = await ApplicationData.Current.LocalFolder.GetFileAsync(SqLiteConnectionProvider.DatabasePath);
                var stream = RandomAccessStreamReference.CreateFromFile(dbFile);
                var attachment = new EmailAttachment(dbFile.Name, stream);
                emailMessage.Attachments.Add(attachment);
            }
            catch (Exception e)
            {
                Logger.Error("Can't attach UserPreferences db", e);
            }

            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("MetroLogs");
                var files = await folder.GetFilesAsync();
                foreach (var file in files.OrderByDescending(file => file.DateCreated).Take(2))
                {
                    try
                    {
                        var stream = RandomAccessStreamReference.CreateFromFile(file);
                        var attachment = new EmailAttachment(file.Name, stream);
                        emailMessage.Attachments.Add(attachment);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error loading log file {file.Name}", e);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Can't load logs folder", e);
            }

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }));


        public bool IsFeedbackSupported { get; } = StoreServicesFeedbackLauncher.IsSupported();

        public RelayCommand GiveFeedbackCommand => _giveFeedbackCommand ?? (_giveFeedbackCommand = new RelayCommand(async () =>
        {
            if (IsFeedbackSupported)
            {
                var launcher = StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            }
        }));

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Theme))
            {
                await ThemeSelectorService.SetThemeAsync(Theme).ConfigureAwait(false);
            }
            else if (e.PropertyName == nameof(SelectedLanguage))
            {
                ApplicationLanguages.PrimaryLanguageOverride = SelectedLanguage.Value != SystemDisplayLanguage ? SelectedLanguage.Value : string.Empty;
                var dialog = new ContentDialog
                {
                    Title = "SettingsPage_LanguageChangedWarningTitle".GetLocalized(),
                    Content = "SettingsPage_LanguageChangedMessage".GetLocalized(),
                    RequestedTheme = ThemeSelectorService.Theme
                };
                if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", nameof(ContentDialog.CloseButtonText)))
                {
                    dialog.CloseButtonText = "SettingsPage_LanguageChangedOK".GetLocalized();
                }
                else
                {
                    dialog.SecondaryButtonText = "SettingsPage_LanguageChangedOK".GetLocalized();
                }
                await dialog.ShowAsync();
            } else if (e.PropertyName == nameof(IsAutoSyncEnabled))
            {
                if (IsAutoSyncEnabled)
                {
                    try
                    {
                        await _syncService.SignIn();
                        _syncBackgroundTask.Register();
                    }
                    catch (SyncException ex)
                    {
                        Logger.Error("Can't sign in to OneDrive", ex);
                        IsAutoSyncEnabled = false;
                    }
                }
                else
                {
                    _syncBackgroundTask.Unregister();
                }
            }
        }

        public override async void OnNavigatedTo(object parameter)
        {
            SubscribeToEvents();
            await Initialize();
        }

        public override void OnNavigatedFrom(object parameter)
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _preferencesService.PreferencesUpdated += OnPrefUpdated;
        }

        private async void OnPrefUpdated(PreferencesUpdatedEventArg e)
        {
            if (e.Action == UpdateAction.Reset)
            {
                await Initialize();
            }
        }

        private void UnsubscribeFromEvents()
        {
            _preferencesService.PreferencesUpdated -= OnPrefUpdated;
        }

        public async Task Initialize()
        {
            Theme = ThemeSelectorService.Theme;
            Currencies = _preferencesService.CurrencyPreferences;
            foreach (var currency in Currencies)
            {
                currency.PropertyChanged += OnCurrencyChanged;
            }

            SelectedLanguage = Languages.FirstOrDefault(info => ApplicationLanguages.PrimaryLanguageOverride == info.Value) ?? Languages.First();
            
            var syncDate = _syncService.LastSyncDate;
            if (syncDate != null && !await _syncService.IsLocalModified())
            {
                SyncStatus = string.Format("SettingsPage_SyncStatus_Succed".GetLocalized(), Views.Formatter.DateTime.TimeAgo(syncDate.Value.LocalDateTime, false));
            }
            else
            {
                SyncStatus = "SettingsPage_SyncStatus_NotSynced".GetLocalized();
            }

            IsAutoSyncEnabled = _syncBackgroundTask.IsRegistered;

            PropertyChanged += OnPropertyChanged;
        }

        public void Deinitialize()
        {
            foreach (var currency in Currencies)
            {
                currency.PropertyChanged += OnCurrencyChanged;
            }
        }

        private async void OnCurrencyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrencyPreference.IsShown))
            {
                await _preferencesService.UpdateCurrencyPreference((CurrencyPreference) sender).ConfigureAwait(false);
            }
        }
    }
}
