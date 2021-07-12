using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.Views;
using CryptoCoins.UWP.Views.Formatter;
using Microsoft.Toolkit.Uwp.Helpers;
using Nito.Mvvm;
using DateTime = System.DateTime;

namespace CryptoCoins.UWP.ViewModels
{
    public enum ViewMode
    {
        Grid,
        List
    }

    public class DashboardViewModel : ViewModelBase
    {
        public const string ConversionsStateAvailable = "Available";
        public const string ConversionsStateEmpty = "Empty";
        public const string ConversionsStateUnavailable = "Unavailable";
        public const string ConversionsStateFilteredEmpty = "FilteredEmpty";
        private static readonly TimeSpan RefreshRate = TimeSpan.FromSeconds(10.5d);
        private static readonly TimeSpan FeaturedRefreshRate = TimeSpan.FromSeconds(60 * 30);
        private readonly CryptoService _cryptoService;
        private readonly DispatcherTimer _featuredTimer = new DispatcherTimer();
        private readonly NavigationService _navigationService;
        private readonly UserPreferencesService _preferencesService;
        private readonly TileUpdateTask _tileUpdateTask;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private FilterCollection<ConversionInfo> _conversions;
        private string _conversionsState;
        private FilterCollection<DetailedConversionInfo> _featuredConversions;
        private DataState _featuredConversionState;
        private string _filter;
        private DateTime _lastFeaturedUpdate = DateTime.MinValue;
        private DateTime _lastUpdate = DateTime.MinValue;
        private ViewMode _mode;
        private AsyncCommand _pinToStartCommand;
        private AsyncCommand _refreshCommand;
        private RelayCommand _retryCommand;
        private RelayCommand _retryFeaturedCommand;
        private RelayCommand<ConversionInfo> _shareCommand;
        private RelayCommand<ViewMode> _toggleViewMode;
        private RelayCommand<ConversionInfo> _updateTileInfoCommand;
        private DescriptionWrapper<FeaturedPreference> _selectedFeaturedPreference;
        public static readonly List<DescriptionWrapper<FeaturedPreference>> TimeFrames = new List<DescriptionWrapper<FeaturedPreference>>
        {
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_Day".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeDay, Steps = 24}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_Week".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeWeek, Steps = 7}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_Month".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeMonth, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_3Months".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.Range3Months, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_6Months".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.Range6Months, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "SettingsPage_ChartTimeFrame_Year".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeYear, Steps = 12}
            }
        };
        public DescriptionWrapper<FeaturedPreference> SelectedFeaturedPreference
        {
            get => _selectedFeaturedPreference;
            set => Set(ref _selectedFeaturedPreference, value);
        }

        public DashboardViewModel(UserPreferencesService preferencesService, CryptoService cryptoService, TileUpdateTask tileUpdateTask, NavigationService navigationService)
        {
            _timer.Interval = RefreshRate;
            _timer.Tick += OnTimerTick;
            _featuredTimer.Interval = FeaturedRefreshRate;
            _featuredTimer.Tick += OnFeaturedTimerTick;
            _preferencesService = preferencesService;
            _cryptoService = cryptoService;
            _tileUpdateTask = tileUpdateTask;
            _navigationService = navigationService;

            Conversions = new FilterCollection<ConversionInfo>(new LambdaComparer<ConversionInfo>((x, y) => x.Pref.DisplayOrder.CompareTo(y.Pref.DisplayOrder)));
            Conversions.CollectionReordering += ReorderConversion;
            Conversions.FilterFunc = info => string.IsNullOrEmpty(Filter) ||
                                             info.From.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1 ||
                                             info.FromFullName.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1;

            FeaturedConversions =
                new FilterCollection<DetailedConversionInfo>(new LambdaComparer<ConversionInfo>((x, y) => x.Pref.FeaturedDisplayOrder.CompareTo(y.Pref.FeaturedDisplayOrder)));
            FeaturedConversions.CollectionReordering += ReorderFeaturedConversion;
            FeaturedConversions.CollectionChanged += (sender, args) => UpdateFeaturedState();
            Conversions.CollectionChanged += (sender, args) => UpdateConversionsState();

            Mode = _preferencesService.DisplayPreference.DashboardViewMode;

            LoadChartFramePrefs();

            //CryptoCurrenciesPreferences and CurrenciesPreferences can be changed outside dashboard. So we subscribe to a change event and reset last update time in case of the event. It will force update conversion on next Activation
            foreach (var pref in _preferencesService.CryptoCurrenciesPreferences)
            {
                pref.PropertyChanged += OnPreferenceChanged;
            }

            foreach (var pref in _preferencesService.CurrencyPreferences)
            {
                pref.PropertyChanged += OnPreferenceChanged;
            }

            _preferencesService.CryptoCurrenciesPreferences.CollectionChanged += OnCryptoPreferenceCollectionChanged;
            _preferencesService.CurrencyPreferences.CollectionChanged += OnCurrencyPreferenceCollectionChanged;

            _preferencesService.PropertyChanged += OnPrefPropertyChanged;
            _preferencesService.PreferencesUpdated += OnPreferencesSourceUpdated;
        }

        public ProgressState ConversionProgress { get; } = new ProgressState();
        public ProgressState FeaturedConversionProgress { get; } = new ProgressState();

        public List<DescriptionWrapper<FeaturedPreference>> FeaturedPreferences { get; } = TimeFrames;

        public FilterCollection<DetailedConversionInfo> FeaturedConversions
        {
            get => _featuredConversions;
            set => Set(ref _featuredConversions, value);
        }

        public IBitmapProvider ShareImageProvider { get; set; }

        public FilterCollection<ConversionInfo> Conversions
        {
            get => _conversions;
            set => Set(ref _conversions, value);
        }

        public string ConversionsState
        {
            get => _conversionsState;
            set => Set(ref _conversionsState, value);
        }

        public DataState FeaturedConversionState
        {
            get => _featuredConversionState;
            set => Set(ref _featuredConversionState, value);
        }

        public ViewMode Mode
        {
            get => _mode;
            set => Set(ref _mode, value);
        }

        public string Filter
        {
            get => _filter;
            set => Set(ref _filter, value);
        }

        public RelayCommand<ViewMode> ToggleViewMode => _toggleViewMode ?? (_toggleViewMode = new RelayCommand<ViewMode>(e => { Mode = e; }));

        public RelayCommand RetryCommand => _retryCommand ?? (_retryCommand = new RelayCommand(async () => { await RefreshConversions().ConfigureAwait(false); }));

        public RelayCommand RetryFeaturedCommand => _retryFeaturedCommand ??
                                                    (_retryFeaturedCommand = new RelayCommand(async () => { await RefreshFeaturedConversions().ConfigureAwait(false); }));

        public AsyncCommand PinToStartCommand => _pinToStartCommand ?? (_pinToStartCommand = new AsyncCommand(async e =>
        {
            var info = (ConversionInfo) e;
            if (TileUpdateTask.IsSecondaryTilePinned(info))
            {
                if (await _tileUpdateTask.UnpinSecondaryTile(info))
                {
                    info.IsPinned = false;
                }
            }
            else
            {
                if (e is DetailedConversionInfo detailedInfo)
                {
                    info.IsPinned = await _tileUpdateTask.PinSecondaryTile(detailedInfo);
                }
                else
                {
                    try
                    {
                        var newInfo = await _cryptoService.GetDetailedConversion(info.From, info.To);
                        info.IsPinned = await _tileUpdateTask.PinSecondaryTile(newInfo);
                    }
                    catch (ApiException ex)
                    {
                        Logger.Error($"Failed to update pinned tile {info.From} {info.To}", ex);
                    }
                }
            }
        }));

        public RelayCommand<ConversionInfo> UpdateTileInfoCommand => _updateTileInfoCommand ??
                                                                     (_updateTileInfoCommand = new RelayCommand<ConversionInfo>(e =>
                                                                     {
                                                                         if (e != null)
                                                                         {
                                                                             e.IsPinned = TileUpdateTask.IsSecondaryTilePinned(e);
                                                                         }
                                                                     }));

        public RelayCommand<ConversionInfo> ShareCommand => _shareCommand ?? (_shareCommand = new RelayCommand<ConversionInfo>(e =>
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();

            dataTransferManager.DataRequested += (sender, args) =>
            {
                var request = args.Request;
                request.Data.Properties.Title = string.Format("DashboardPage_ShareTitle".GetLocalized(), e.From, e.To);
                request.Data.Properties.Description = "DashboardPage_ShareDescription".GetLocalized();
                request.Data.SetText(string.Format("DashboardPage_ShareText".GetLocalized(), e.From, e.To, Currency.FormatRate(e.Rate, e.To)));
                request.Data.SetApplicationLink(new Uri("ms-windows-store://pdp/?ProductId=9nwrp455hswh"));
                request.Data.SetWebLink(new Uri("https://www.microsoft.com/store/apps/9nwrp455hswh"));

                if (e is DetailedConversionInfo)
                {
                    request.Data.SetDataProvider(StandardDataFormats.Bitmap, async r =>
                    {
                        // Request deferral to wait for async calls
                        var deferral = r.GetDeferral();
                        try
                        {
                            var image = await ShareImageProvider.GetBitmap(e);
                            r.SetData(image);
                        }
                        finally
                        {
                            deferral.Complete();
                        }
                    });
                }
            };
            DataTransferManager.ShowShareUI();
        }));

        private AsyncCommand _navigateToInfo;

        public AsyncCommand OpenCoinInfo => _navigateToInfo ?? (_navigateToInfo = new AsyncCommand(async (e) =>
        {
            try
            {
                var info = (string) e;
                var coins = await _cryptoService.GetCoins(false);
                var match = coins.FirstOrDefault(currencyInfo => currencyInfo.Code == info);
                _navigationService.Navigate<CoinInfoWebPage>(new CoinInfoWebViewModel.NavigationParameter()
                {
                    Currency = match
                });
            }
            catch (ApiException)
            {

            }
        }));

        public AsyncCommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new AsyncCommand(async () =>
        {
            await RefreshConversions();
            await RefreshFeaturedConversions();
        }));

        private async void OnPreferencesSourceUpdated(PreferencesUpdatedEventArg e)
        {
            if (e.Action == UpdateAction.Reset)
            {
                await RefreshConversions();
                await RefreshFeaturedConversions();
            }
        }

        private async void OnPrefPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                if (e.PropertyName == nameof(UserPreferencesService.FeaturedPreference))
                {
                    LoadChartFramePrefs();
                    _lastFeaturedUpdate = DateTime.MinValue;
                    await RefreshFeaturedConversions();
                }
            });
        }

        private void OnPreferenceChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CryptoCurrencyPreference.IsShown) || e.PropertyName == nameof(CurrencyPreference.IsShown))
            {
                _lastUpdate = DateTime.MinValue;
            }
        }

        private void OnCryptoPreferenceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //For now only 'Add' is enough. It's the only event can occur.
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CryptoCurrencyPreference pref in e.NewItems)
                {
                    pref.PropertyChanged += OnPreferenceChanged;
                }

                _lastUpdate = DateTime.MinValue;
            }
        }

        private void OnCurrencyPreferenceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //For now only 'Add' is enough. It's the only event can occur.
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CurrencyPreference pref in e.NewItems)
                {
                    pref.PropertyChanged += OnPreferenceChanged;
                }

                _lastUpdate = DateTime.MinValue;
            }
        }

        public event Action ConversionsChanged;

        private void UpdateConversionsState()
        {
            if (Conversions.SourceList.Count == 0)
            {
                ConversionsState = ConversionsStateEmpty;
            }
            else if (Conversions.Count == 0)
            {
                ConversionsState = ConversionsStateFilteredEmpty;
            }
            else
            {
                ConversionsState = ConversionsStateAvailable;
            }
        }

        private void UpdateFeaturedState()
        {
            if (FeaturedConversions.Count > 0)
            {
                FeaturedConversionState = DataState.Available;
            }
            else
            {
                FeaturedConversionState = DataState.Empty;
            }
        }

        private async void OnFeaturedTimerTick(object sender, object e)
        {
            await RefreshFeaturedConversions().ConfigureAwait(false);
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Filter))
            {
                Conversions.Filter();
                ConversionsChanged?.Invoke();
            }
            else if (e.PropertyName == nameof(Mode))
            {
                var pref = _preferencesService.DisplayPreference;
                pref.DashboardViewMode = Mode;
                await _preferencesService.UpdateDisplayPreference(pref);
            }
            else if (e.PropertyName == nameof(SelectedFeaturedPreference))
            {
                await _preferencesService.UpdateFeaturedPreference(SelectedFeaturedPreference.Value).ConfigureAwait(false);
            }
        }

        private async void ReorderConversion(object sender, ReorderEventArgs<ConversionInfo> args)
        {
            var oldIndex = args.OldIndex;
            var newIndex = args.NewIndex;
            var list = (IList<ConversionInfo>) sender;
            var item = args.Item;
            var prefs = _preferencesService.ConversionPreferences;
            var equality = ConversionDirectionEqualityComparer.Instance;
            var oldOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, item)).DisplayOrder;
            if (newIndex > oldIndex)
            {
                var replacedItem = list[newIndex - 1];
                var newOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, replacedItem)).DisplayOrder;
                foreach (var pref in prefs)
                {
                    if (equality.Equals(pref, item))
                    {
                        pref.DisplayOrder = newOrder;
                    }
                    else if (pref.DisplayOrder > oldOrder && pref.DisplayOrder <= newOrder)
                    {
                        pref.DisplayOrder--;
                    }
                }
            }
            else
            {
                var replacedItem = list[newIndex];
                var newOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, replacedItem)).DisplayOrder;
                foreach (var pref in prefs)
                {
                    if (equality.Equals(pref, item))
                    {
                        pref.DisplayOrder = newOrder;
                    }
                    else if (pref.DisplayOrder < oldOrder && pref.DisplayOrder >= newOrder)
                    {
                        pref.DisplayOrder++;
                    }
                }
            }

            await _preferencesService.UpdateConversionPreference(prefs);
        }

        private async void ReorderFeaturedConversion(object sender, ReorderEventArgs<DetailedConversionInfo> args)
        {
            var oldIndex = args.OldIndex;
            var newIndex = args.NewIndex;
            var list = (IList<DetailedConversionInfo>) sender;
            var item = args.Item;
            var prefs = _preferencesService.ConversionPreferences;
            var equality = ConversionDirectionEqualityComparer.Instance;
            var oldOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, item)).FeaturedDisplayOrder;
            if (newIndex > oldIndex)
            {
                var replacedItem = list[newIndex - 1];
                var newOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, replacedItem)).FeaturedDisplayOrder;
                foreach (var pref in prefs)
                {
                    if (equality.Equals(pref, item))
                    {
                        pref.FeaturedDisplayOrder = newOrder;
                    }
                    else if (pref.FeaturedDisplayOrder > oldOrder && pref.FeaturedDisplayOrder <= newOrder)
                    {
                        pref.FeaturedDisplayOrder--;
                    }
                }
            }
            else
            {
                var replacedItem = list[newIndex];
                var newOrder = prefs.FirstOrDefault(pref => equality.Equals(pref, replacedItem)).FeaturedDisplayOrder;
                foreach (var pref in prefs)
                {
                    if (equality.Equals(pref, item))
                    {
                        pref.FeaturedDisplayOrder = newOrder;
                    }
                    else if (pref.FeaturedDisplayOrder < oldOrder && pref.FeaturedDisplayOrder >= newOrder)
                    {
                        pref.FeaturedDisplayOrder++;
                    }
                }
            }

            await _preferencesService.UpdateConversionPreference(prefs);
        }

        private void StartTimers()
        {
            _timer.Start();
            _featuredTimer.Start();
        }

        private void LoadChartFramePrefs()
        {
            var featuredPref = _preferencesService.FeaturedPreference;
            if (featuredPref != null)
            {
                SelectedFeaturedPreference =
                    FeaturedPreferences.FirstOrDefault(pref => pref.Value.RangeMinutes == featuredPref.RangeMinutes &&
                                                               pref.Value.Steps == featuredPref.Steps);
            }

            if (SelectedFeaturedPreference == null)
            {
                SelectedFeaturedPreference = FeaturedPreferences.First();
            }
        }

        public override async void OnNavigatedTo(object parameter)
        {
            AttachEventHandlers();
            await RefreshIfNeeded();
            StartTimers();
        }

        public override void OnNavigatedFrom(object parameter)
        {
            StopTimers();
            DeattachEventHandlers();
        }

        public override async void OnActivated()
        {
            AttachEventHandlers();
            await RefreshIfNeeded();
            StartTimers();
        }

        public override void OnDeactivated()
        {
            StopTimers();
            DeattachEventHandlers();
        }

        private void StopTimers()
        {
            _timer.Stop();
            _featuredTimer.Stop();
        }

        private async void OnTimerTick(object sender, object e)
        {
            await RefreshConversions().ConfigureAwait(false);
        }

        private void AttachEventHandlers()
        {
            foreach (var pref in _preferencesService.ConversionPreferences)
            {
                pref.PropertyChanged += OnConversionPrefPropertyChanged;
            }

            PropertyChanged += OnPropertyChanged;
        }

        private void DeattachEventHandlers()
        {
            foreach (var pref in _preferencesService.ConversionPreferences)
            {
                pref.PropertyChanged -= OnConversionPrefPropertyChanged;
            }

            PropertyChanged -= OnPropertyChanged;
        }

        private async Task RefreshConversions()
        {
            _lastUpdate = DateTime.Now;
            using (ConversionProgress.BeginOperation())
            {
                try
                {
                    var conversions = await _cryptoService.GetPreferedConversion();
                    ModifyConversions(conversions);
                    FeaturedConversions?.SourceList.UpdateElements(conversions, info => info.From + info.To,
                        info => info.From + info.To,
                        (featured, info) =>
                        {
                            featured.Rate = info.Rate;
                            if (_preferencesService.FeaturedPreference.RangeMinutes == SettingsViewModel.RangeDay)
                            {
                                featured.Change24 = info.Change24;
                                featured.ChangeValue = info.ChangeValue;
                                featured.Volume24 = info.Volume24;
                            }
                        });
                    UpdateConversionsState();
                    ConversionsChanged?.Invoke();
                }
                catch (ApiException)
                {
                    if (FeaturedConversions == null || FeaturedConversions.SourceList.Count == 0)
                    {
                        ConversionsState = ConversionsStateUnavailable;
                    }
                }
            }
        }

        private void ModifyConversions(List<ConversionInfo> conversions)
        {
            var comparer = CurrencyPairEqualityUpdater.Instance;
            var updateMap = conversions.AddToDictionary(comparer.GetHashCode);
            for (var i = Conversions.SourceList.Count - 1; i >= 0; i--)
            {
                var targetItem = Conversions.SourceList[i];
                var hash = comparer.GetHashCode(targetItem);
                if (updateMap.TryGetValue(hash, out var sourceItem))
                {
                    comparer.Update(targetItem, sourceItem);
                    updateMap.Remove(hash);
                }
                else
                {
                    Conversions.RemoveSourceAt(i);
                }
            }

            foreach (var missingElement in updateMap.Values)
            {
                Conversions.Add(missingElement);
            }
        }

        private async void OnConversionPrefPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConversionPreference.IsFeatured))
            {
                await OnFeaturedChanged((ConversionPreference) sender);
            }
        }

        private async Task OnFeaturedChanged(ConversionPreference pref)
        {
            if (pref.IsFeatured)
            {
                try
                {
                    var info = await _cryptoService.GetDetailedConversion(pref.From, pref.To);
                    info.Pref = pref;
                    FeaturedConversions.AddOrUpdate(info, CurrencyPairEqualityUpdater.Instance);
                }
                catch (ApiException e)
                {
                    Logger.Error("Failed to add featured conversion", e);
                }
            }
            else
            {
                var match = FeaturedConversions.FirstOrDefault(info => info.Pref == pref);
                if (match != null)
                {
                    FeaturedConversions.Remove(match);
                }
            }

            await _preferencesService.UpdateConversionPreference(pref).ConfigureAwait(false);
        }

        private async Task RefreshIfNeeded()
        {
            var now = DateTime.Now;
            if (now - _lastUpdate > RefreshRate)
            {
                await RefreshConversions();
            }

            if (now - _lastFeaturedUpdate > FeaturedRefreshRate)
            {
                await RefreshFeaturedConversions();
            }
        }

        private async Task RefreshFeaturedConversions()
        {
            _lastFeaturedUpdate = DateTime.Now;
            using (FeaturedConversionProgress.BeginOperation())
            {
                var conversionPreferences = _preferencesService.ConversionPreferences;
                var requestedConv = conversionPreferences.Where(pref => pref.IsFeatured).OrderBy(pref => pref.FeaturedDisplayOrder).ToArray();
                var conversions = requestedConv.Select(pref => _cryptoService.GetDetailedConversion(pref.From, pref.To)).ToArray();

                for (var i = FeaturedConversions.SourceList.Count - 1; i >= 0; i--)
                {
                    var conv = FeaturedConversions.SourceList[i];
                    if (requestedConv.All(preference => preference.From != conv.From && preference.To != conv.To))
                    {
                        FeaturedConversions.RemoveSourceAt(i);
                    }
                }

                foreach (var conversionTask in conversions)
                {
                    try
                    {
                        var conversion = await conversionTask;
                        conversion.Pref = conversionPreferences.Single(pref => pref.From == conversion.From && pref.To == conversion.To);
                        FeaturedConversions.AddOrUpdate(conversion, CurrencyPairEqualityUpdater.Instance);
                    }
                    catch (ApiException)
                    {
                        if (FeaturedConversions == null || FeaturedConversions.Count == 0)
                        {
                            FeaturedConversionState = DataState.Unavailable;
                        }

                        _featuredTimer.Interval = RefreshRate;
                    }
                }

                if (conversions.All(task => task.Status == TaskStatus.RanToCompletion))
                {
                    UpdateFeaturedState();
                    _featuredTimer.Interval = FeaturedRefreshRate;

                    await UpdateLiveTiles();
                }
            }
        }

        private async Task UpdateLiveTiles()
        {
            if (Interlocked.CompareExchange(ref _tileUpdateTask.IsRunning, 1, 0) == 0)
            {
                try
                {
                    await _tileUpdateTask.UpdateTiles(FeaturedConversions.ToList(), new List<DetailedConversionInfo>(), CancellationToken.None);
                }
                catch (Exception e)
                {
                    Logger.Error("Skipping exception", e);
                }
                finally
                {
                    _tileUpdateTask.IsRunning = 0;
                }
            }
        }
    }
}
