using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Extensions;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.ViewModels.Converters;
using CryptoCoins.UWP.ViewModels.Entities;
using CryptoCoins.UWP.Views;
using CryptoCoins.UWP.Views.Entities;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using Nito.Mvvm;
using HoldingsTransaction = CryptoCoins.UWP.Models.StorageEntities.HoldingsTransaction;

namespace CryptoCoins.UWP.ViewModels
{
    public enum PortfolioViewMode
    {
        Chart,
        List,
        History
    }

    public class PortfolioViewModel : ViewModelBase
    {
        private const string DefaultTargetCurrency = "USD";
        private static readonly TimeSpan CurrentRefreshRate = TimeSpan.FromSeconds(10.5d);
        private static readonly TimeSpan RateLimitInterval = TimeSpan.FromSeconds(5d);

        public List<DateTime> DatesHistory;
        private readonly CryptoService _cryptoService;
        private readonly DialogService _dialogService;
        private readonly HoldingsConverter _holdingsConverter;
        private readonly HoldingsService _holdingsService;
        private readonly PortfolioManager _portfolioManager;
        private readonly UserPreferencesService _preferencesService;
        private readonly Random _random = new Random();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly TileUpdateTask _tileUpdateTask;
        private readonly DispatcherTimer _timer = new DispatcherTimer {Interval = CurrentRefreshRate};

        private readonly Dictionary<string, Color> ColorsMap = new Dictionary<string, Color>
        {
            {"BTC", "#F7931A".ToColor()},
            {"ETH", "#8D8D8D".ToColor()},
            {"LTC", "#BEBEBE".ToColor()},
            {"DASH", "#0074B6".ToColor()},
            {"XMR", "#FF6600".ToColor()},
            {"NXT", "#008FBB".ToColor()},
            {"ETC", "#669073".ToColor()},
            {"DOGE", "#BB9F36".ToColor()},
            {"ZEC", "#F2BF4D".ToColor()},
            {"BTS", "#F2BF4D".ToColor()},
            {"DGB", "#0066CC".ToColor()},
            {"XRP", "#0090C9".ToColor()},
            {"BTCD", "#010101".ToColor()},
            {"PPC", "#3CB054".ToColor()},
            {"CRAIG", "#3CB054".ToColor()},
            {"XBS", "#1B9F66".ToColor()},
            {"XPY", "#4E3F3F".ToColor()},
            {"PRC", "#FF1A00".ToColor()},
            {"YBC", "#FF1A00".ToColor()},
            {"DANK", "#481444".ToColor()},
            {"GIVE", "#E1AF45".ToColor()}
        };

        private readonly string[] StaticColors =
        {
            "#3366CC", "#DC3912", "#FF9900", "#109618", "#990099", "#3B3EAC", "#0099C6", "#DD4477", "#66AA00", "#B82E2E", "#316395", "#994499", "#22AA99", "#AAAA11", "#6633CC",
            "#E67300", "#8B0707", "#329262", "#5574A6", "#3B3EAC"
        };

        private double _change;
        private double _changePercent;

        private int _chartColorIndex;
        private DateTime _currentUpdateTime;
        private DataState _dataState = DataState.NotReady;
        private DateTime _historyUpdateTime;
        private double _holdings;

        private List<SummarySnapshot> _holdingsHistory;
        private List<SummarySnapshot> _investmentsHistory;

        private ObservableCollection<HoldingsSummary> _holdingsSummaries;
        private List<HoldingsSummary> _investmentsSummaries;
        private ObservableCollection<HoldingsSummary> _summaries;

        private double _investments;
        private bool _isCurrentLoaded;

        private bool _isCurrentValid;

        private bool _isHistoryLoaded;
        private bool _isHistoryValid;
        private bool _isViewActive;
        private double _max;
        private double _min;
        private PortfolioViewMode _mode;
        private AsyncCommand _openEditDialogCommand;

        private AsyncCommand _pinPortfolioTile;

        private double _releasedProfit;

        private AsyncCommand _retryCommand;

        private double _roi;

        private double _roiPercent;
        private RelayCommand _openCreateDialogCommand;
        private RelayCommand<HoldingsSummary> _openSpecifiedDialogCommand;
        private string _selectedCurrency;
        private CancellationTokenSource _serverUpdateTokenSource = new CancellationTokenSource();

        private ObservableCollection<HoldingsSummary> _sortedHoldings;
        private List<HoldingsTransaction> _storageTransactions;
        private DescriptionWrapper<FeaturedPreference> _timeFrame;
        private RelayCommand<PortfolioViewMode> _toggleViewModeCommand;

        private ObservableCollection<Entities.HoldingsTransaction> _transactions;

        private ValueRange[] _loseAreas;

        public ValueRange[] LoseAreas
        {
            get => _loseAreas;
            set => Set(ref _loseAreas, value);
        }

        public PortfolioViewModel(DialogService dialogService, CryptoService cryptoService, UserPreferencesService preferencesService, TileUpdateTask tileUpdateTask,
            HoldingsService holdingsService, HoldingsConverter holdingsConverter)
        {
            _dialogService = dialogService;
            _cryptoService = cryptoService;
            _preferencesService = preferencesService;
            _tileUpdateTask = tileUpdateTask;
            _holdingsService = holdingsService;
            _holdingsConverter = holdingsConverter;
            _portfolioManager = new PortfolioManager();
            LoadPreferences();
            _timer.Tick += OnTimerTick;
            PropertyChanged += OnPropertyChanged;
            _preferencesService.PreferencesUpdated += OnPreferencesUpdated;
            _holdingsService.HoldingsChanged += OnHoldingsChanged;
            CurrentProgress.PropertyChanged += OnCurrentProgressChanged;
            HistoryProgress.PropertyChanged += OnHistoryProgressChanged;
        }

        private async void OnPreferencesUpdated(PreferencesUpdatedEventArg e)
        {
            if (e.Action == UpdateAction.Reset)
            {
                InvalidateCurrent();
                InvalidateHistory();
                Transactions = null;
                _summaries = null;
                
                LoadDynamicPreferences();
                await EnsureLocalDataLoaded();
                await EnsurePortfolioUpToDate();
            }
        }

        private CancellationToken ServerUpdateToken => _serverUpdateTokenSource.Token;


        public ObservableCollection<string> Currencies { get; } = new ObservableCollection<string>();

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set => Set(ref _selectedCurrency, value);
        }

        public double Holdings
        {
            get => _holdings;
            set => Set(ref _holdings, value);
        }

        public double Change
        {
            get => _change;
            set => Set(ref _change, value);
        }

        public double ChangePercent
        {
            get => _changePercent;
            set => Set(ref _changePercent, value);
        }

        public bool IsCurrentLoaded
        {
            get => _isCurrentLoaded;
            set => Set(ref _isCurrentLoaded, value);
        }

        public bool IsHistoryLoaded
        {
            get => _isHistoryLoaded;
            set => Set(ref _isHistoryLoaded, value);
        }

        public double Min
        {
            get => _min;
            set => Set(ref _min, value);
        }

        public double Max
        {
            get => _max;
            set => Set(ref _max, value);
        }

        public double Investments
        {
            get => _investments;
            set => Set(ref _investments, value);
        }

        public double ReleasedProfit
        {
            get => _releasedProfit;
            set => Set(ref _releasedProfit, value);
        }

        public double Roi
        {
            get => _roi;
            set => Set(ref _roi, value);
        }

        public double RoiPercent
        {
            get => _roiPercent;
            set => Set(ref _roiPercent, value);
        }

        public DescriptionWrapper<FeaturedPreference> TimeFrame
        {
            get => _timeFrame;
            set => Set(ref _timeFrame, value);
        }

        public bool IsCurrentValid
        {
            get => _isCurrentValid;
            set => Set(ref _isCurrentValid, value);
        }

        public bool IsHistoryValid
        {
            get => _isHistoryValid;
            set => Set(ref _isHistoryValid, value);
        }

        private DateTime _chartStart;

        public DateTime ChartStart
        {
            get => _chartStart;
            set => Set(ref _chartStart, value);
        }

        public bool IsCurrentLoading => !IsCurrentValid && CurrentProgress.IsOperating;

        public bool IsHistoryLoading => !IsHistoryValid && HistoryProgress.IsOperating;


        public List<DescriptionWrapper<FeaturedPreference>> TimeFrames { get; } = new List<DescriptionWrapper<FeaturedPreference>>
        {
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_Day".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeDay, Steps = 24}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_Week".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeWeek, Steps = 7}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_Month".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeMonth, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_3Months".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.Range3Months, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_6Months".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.Range6Months, Steps = 30}
            },
            new DescriptionWrapper<FeaturedPreference>
            {
                Description = "PortfolioPage_ChartTimeFrame_Year".GetLocalized(),
                Value = new FeaturedPreference {RangeMinutes = SettingsViewModel.RangeYear, Steps = 12}
            }
        };

        public PortfolioViewMode Mode
        {
            get => _mode;
            set => Set(ref _mode, value);
        }

        public ObservableCollection<HoldingsSummary> HoldingsSummaries
        {
            get => _holdingsSummaries;
            set => Set(ref _holdingsSummaries, value);
        }

        public ObservableCollection<Entities.HoldingsTransaction> Transactions
        {
            get => _transactions;
            set => Set(ref _transactions, value);
        }

        public ObservableCollection<HoldingsSummary> SortedHoldings
        {
            get => _sortedHoldings;
            set => Set(ref _sortedHoldings, value);
        }

        public RelayCommand<PortfolioViewMode> ToggleViewModeCommand =>
            _toggleViewModeCommand ?? (_toggleViewModeCommand = new RelayCommand<PortfolioViewMode>(e => { Mode = e; }));

        public AsyncCommand OpenEditDialogCommand => _openEditDialogCommand ?? (_openEditDialogCommand = new AsyncCommand(async e =>
        {
            var arg = (Entities.HoldingsTransaction) e;
            var result = (TransactionViewModel.Result) await _dialogService.ShowAsync<TransactionDialog>(new TransactionViewModel.Argument {TransactionId = arg.Id});
        }));

        public AsyncCommand PinPortfolioTile => _pinPortfolioTile ?? (_pinPortfolioTile = new AsyncCommand(async () =>
        {
            if (!TileUpdateTask.IsSecondaryPortfolioTilePinned(SelectedCurrency))
            {
                await _tileUpdateTask.PinSecondaryPortfolioTile(SelectedCurrency).ConfigureAwait(false);
            }
            else
            {
                await _tileUpdateTask.UnpinSecondaryPortfolioTile(SelectedCurrency).ConfigureAwait(false);
            }
        }));

        public RelayCommand OpenCreateDialogCommand => _openCreateDialogCommand ?? (_openCreateDialogCommand = new RelayCommand(async () =>
        {
            var result = (TransactionViewModel.Result) await _dialogService.ShowAsync<TransactionDialog>(new TransactionViewModel.Argument());
        }));

        public RelayCommand<HoldingsSummary> OpenSpecifiedDialogCommand => _openSpecifiedDialogCommand ?? (_openSpecifiedDialogCommand = new RelayCommand<HoldingsSummary>(async (o) =>
        {
            var result = (TransactionViewModel.Result) await _dialogService.ShowAsync<TransactionDialog>(new TransactionViewModel.Argument(){CurrencyCode = o.CurrencyCode});
        }));

        public DataState DataState
        {
            get => _dataState;
            set => Set(ref _dataState, value);
        }

        public ProgressState CurrentProgress { get; } = new ProgressState();

        public ProgressState HistoryProgress { get; } = new ProgressState();

        public AsyncCommand RetryCommand => _retryCommand ?? (_retryCommand = new AsyncCommand(async () => { await EnsurePortfolioUpToDate().ConfigureAwait(false); }));

        public int Steps
        {
            get
            {
                switch (TimeFrame.Value.RangeMinutes)
                {
                    case SettingsViewModel.RangeYear:
                        return 30 + 1;
                    case SettingsViewModel.Range6Months:
                        return 30 + 1;
                    case SettingsViewModel.Range3Months:
                        return 30 + 1;
                    case SettingsViewModel.RangeMonth:
                        return 30 + 1;
                    case SettingsViewModel.RangeWeek:
                        return 7 + 1;
                    case SettingsViewModel.RangeDay:
                        return 24 + 1;
                    default:
                        throw new ArgumentOutOfRangeException("Not implemented");
                }
            }
        }

        public int StepInDays => TimeFrame.Value.RangeMinutes / (60 * 24);

        public DateTime GetChartStart(DescriptionWrapper<FeaturedPreference> pref)
        {
            var minutes = pref.Value.RangeMinutes;
            var start = DateTime.Now.Subtract(TimeSpan.FromMinutes(minutes)).Date;
            start = start.Subtract(TimeSpan.FromDays(start.Day-1));
            return start;
        }

        public List<SummarySnapshot> HoldingsHistory
        {
            get => _holdingsHistory;
            set => Set(ref _holdingsHistory, value);
        }

        public List<SummarySnapshot> InvestmentsHistory
        {
            get => _investmentsHistory;
            set => Set(ref _investmentsHistory, value);
        }

        private void OnHistoryProgressChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProgressState.IsOperating))
            {
                OnPropertyChanged(nameof(IsHistoryLoading));
            }
        }

        private void OnCurrentProgressChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProgressState.IsOperating))
            {
                OnPropertyChanged(nameof(IsCurrentLoading));
            }
        }

        private void OnHoldingsChanged(HoldingsUpdatedEventArg e)
        {
            switch (e.Action)
            {
                case UpdateAction.Add:
                    _storageTransactions.Add(e.Transaction);
                    Transactions.Add(_holdingsConverter.Convert(e.Transaction));
                    break;
                case UpdateAction.Remove:
                    var index = Transactions.FindIndex(holdingsTransaction => holdingsTransaction.Id == e.Transaction.Id);
                    if (index != -1)
                    {
                        Transactions.RemoveAt(index);
                    }

                    index = _storageTransactions.FindIndex(holdingsTransaction => holdingsTransaction.Id == e.Transaction.Id);
                    if (index != -1)
                    {
                        _storageTransactions.RemoveAt(index);
                    }

                    break;
                case UpdateAction.Replace:
                    index = _storageTransactions.FindIndex(holdingsTransaction => holdingsTransaction.Id == e.Transaction.Id);
                    if (index != -1)
                    {
                        _storageTransactions[index] = e.Transaction;
                    }

                    index = Transactions.FindIndex(holdingsTransaction => holdingsTransaction.Id == e.Transaction.Id);
                    if (index != -1)
                    {
                        Transactions[index] = _holdingsConverter.Convert(e.Transaction);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            

            var summaries = e.Summaries.Select(ConvertHoldings);

            var comparer = new HoldingsSummarieEqualityComparer();
            _summaries.AddOrUpdate(summaries, comparer, (x, y) => x.Amount = y.Amount);
            _investmentsSummaries = _summaries.Where(summary => _preferencesService.IsInvestmentCurrency(summary.CurrencyCode)).ToList();
            HoldingsSummaries = new ObservableCollection<HoldingsSummary>(_summaries.Where(summary => !_preferencesService.IsInvestmentCurrency(summary.CurrencyCode) && summary.Amount != 0d));
            SortedHoldings = new ObservableCollection<HoldingsSummary>(HoldingsSummaries.OrderByDescending(summary => summary.Value));
            OnTransactionOrSummaryChanged();
        }

        private void UpdateUnidirectionalSummary(HoldingsSummary summary)
        {
            summary.CurrencyName = _cryptoService.FullCryptoCurrencyName(summary.CurrencyCode);
            summary.Icon = _cryptoService.CryptoCurrencyIcon(summary.CurrencyCode);
            summary.Rate = 1d;
            summary.Min = 1d;
            summary.Max = 1d;
            summary.RateHistory = Enumerable.Repeat(1d, Steps).ToArray();
            summary.IsLoaded = true;
        }

        private void SortHoldings()
        {
            SortedHoldings.Sort(summary => -summary.Value);
        }

        private async Task EnsurePortfolioUpToDate()
        {
            await EnsureCurrentRatesUpToDate();
            UpdateStats();
            await EnsureHistoryRatesUpToDate();
            UpdateStats();
        }

        private async Task EnsureLocalDataLoaded()
        {
            await EnsureTransactionsLoaded();
            await EnsureSummariesLoaded();
            InitializeAmountHistory();
        }

        private async void OnTransactionOrSummaryChanged()
        {
            UpdateDataState();
            InitializeAmountHistory();
            InvalidateCurrent();
            InvalidateHistory();
            await EnsurePortfolioUpToDate().ConfigureAwait(false);
        }

        private async Task EnsureTransactionsLoaded()
        {
            if (Transactions == null)
            {
                _storageTransactions = await _holdingsService.GetTransactions();
                await _holdingsConverter.TryLoadCoins();
                Transactions = new ObservableCollection<Entities.HoldingsTransaction>(_storageTransactions.Select(transaction => _holdingsConverter.Convert(transaction)));
                UpdateDataState();
            }
        }

        private void UpdateDataState()
        {
            if (Transactions.Count == 0)
            {
                DataState = DataState.Empty;
            }
            else if (IsCurrentLoaded)
            {
                DataState = DataState.Available;
            }
        }

        private async Task EnsureSummariesLoaded()
        {
            if (_summaries == null)
            {
                var summaries = await _holdingsService.GetHoldings();
                if (_summaries != null)
                {
                    _summaries.CollectionChanged -= OnSummariesChanged;
                }
                _summaries = new ObservableCollection<HoldingsSummary>(summaries.Select(ConvertHoldings));
                _investmentsSummaries = _summaries.Where(summary => _preferencesService.IsInvestmentCurrency(summary.CurrencyCode)).ToList();
                HoldingsSummaries = new ObservableCollection<HoldingsSummary>(_summaries.Where(summary => !_preferencesService.IsInvestmentCurrency(summary.CurrencyCode) && summary.Amount!= 0d));
                SortedHoldings = new ObservableCollection<HoldingsSummary>(HoldingsSummaries.OrderByDescending(summary => summary.Value));
            }
        }

        private void OnSummariesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
            }
        }

        private HoldingsSummary ConvertHoldings(Models.StorageEntities.HoldingsSummary summary)
        {
            var result = _holdingsConverter.Convert(summary);
            result.CounterCurrencyCode = SelectedCurrency;
            result.ChartColor = GetCurrencyColor(result.CurrencyCode, _chartColorIndex++);
            return result;
        }

        private async Task EnsureCurrentRatesUpToDate()
        {
            if (!IsCurentRatesUpToDate())
            {
                ResetServerCancellationToken();
                await LoadCurrentRates(ServerUpdateToken);
            }
        }

        private void ResetServerCancellationToken()
        {
            _serverUpdateTokenSource.Cancel();
            _serverUpdateTokenSource = new CancellationTokenSource();
        }

        private async Task LoadCurrentRates(CancellationToken token)
        {
            try
            {
                if (Transactions.Count == 0)
                {
                    return;
                }
                
                using (CurrentProgress.BeginOperation())
                {
                    var toCurrencies = new List<string> {SelectedCurrency};
                    var fromCurrencies = _summaries.Select(info => info.CurrencyCode).Except(toCurrencies).ToList();

                    var conversions = await _cryptoService.GetConversions(fromCurrencies, toCurrencies, token);
                    _summaries.UpdateElements(conversions, info => info.CurrencyCode, info => info.From, (summary, info) => summary.UpdateFromCurrentRates(info));

                    IsCurrentValid = true;
                    IsCurrentLoaded = true;
                    _currentUpdateTime = DateTime.Now;
                    DataState = DataState.Available;
                }
            }
            catch (ApiException)
            {
                if (!IsCurrentLoaded)
                {
                    DataState = DataState.Unavailable;
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InitializeAmountHistory()
        {
            var steps = Steps;
            var stepRangeMinutes = TimeFrame.Value.RangeMinutes / (steps - 1);
            var stepRange = TimeSpan.FromMinutes(stepRangeMinutes);
            var openDate = DateTime.Now;

            var datesHistory = new DateTime[steps];

            foreach (var summary in _summaries)
            {
                summary.AmountHistory = new double[steps];
            }

            var orderedTransactions = _storageTransactions.OrderByDescending(transaction => transaction.Date).ToList();

            var summaryAmounts = _summaries.ToDictionary(info => info.CurrencyCode, info => info.Amount);


            var transacitonsIndex = 0;
            for (var i = steps - 1; i >= 0; i--)
            {
                datesHistory[i] = openDate;

                HoldingsTransaction transaction;
                for (; transacitonsIndex < orderedTransactions.Count && (transaction = orderedTransactions[transacitonsIndex]).Date >= openDate; transacitonsIndex++)
                {
                    var changes = _portfolioManager.GetUndoChanges(transaction).Values;
                    foreach (var updatedSummary in changes)
                    {
                        if (summaryAmounts.ContainsKey(updatedSummary.Currency))
                        {
                            summaryAmounts[updatedSummary.Currency] += (double) updatedSummary.AmountChange;
                        }
                        else
                        {
                            var summary = ConvertHoldings(new Models.StorageEntities.HoldingsSummary(updatedSummary.Currency, decimal.Zero));
                            summary.AmountHistory = new double[steps];
                            _summaries.Add(summary);
                            summaryAmounts.Add(updatedSummary.Currency, (double) updatedSummary.AmountChange);
                        }
                    }
                }

                foreach (var summary in _summaries)
                {
                    summary.AmountHistory[i] = summaryAmounts[summary.CurrencyCode];
                }

                openDate = openDate.Subtract(stepRange);
            }

            DatesHistory = datesHistory.ToList();
        }

        private async Task EnsureHistoryRatesUpToDate()
        {
            if (IsCurrentLoaded && !IsHistoryRatesUpToDate())
            {
                ResetServerCancellationToken();
                await LoadHistoricRates(ServerUpdateToken).ConfigureAwait(false);
            }
        }

        private async Task LoadHistoricRates(CancellationToken token)
        {
            using (HistoryProgress.BeginOperation())
            {
                bool isDataLoaded = true;
                foreach (var summary in _summaries)
                {
                    if (summary.CurrencyCode != SelectedCurrency)
                    {
                        try
                        {
                            var conversion = await _cryptoService.GetDetailedConversion(summary.CurrencyCode, SelectedCurrency,
                                TimeFrame.Value.RangeMinutes, token);
                            UpdateHoldingsSummaryFromHistoryRates(summary, conversion);
                        }
                        catch (NoDataForCurrencyException e)
                        {
                        }
                        catch (ApiException e)
                        {
                            isDataLoaded = false;
                        }
                        catch (OperationCanceledException)
                        {
                            isDataLoaded = false;
                        }
                    }
                    else
                    {
                        UpdateUnidirectionalSummary(summary);
                    }
                }

                IsHistoryValid = true;
                IsHistoryLoaded = isDataLoaded;
                _historyUpdateTime = DateTime.Now;
            }
        }

        private void UpdateHoldingsSummaryFromHistoryRates(HoldingsSummary summary, DetailedConversionInfo conversionInfo)
        {
            summary.CounterCurrencyCode = conversionInfo.To;
            summary.Min = conversionInfo.Min;
            summary.Max = conversionInfo.Max;
            summary.Change = conversionInfo.ChangeValue;
            summary.ChangePercent = conversionInfo.Change24;
            summary.RateHistory = conversionInfo.RateHourlyHistory.ToArray();
            summary.IsLoaded = true;
        }


        private void LoadPreferences()
        {
            Mode = _preferencesService.DisplayPreference.PortfolioViewMode;
            var frame = _preferencesService.DisplayPreference.PortfolioChartPreference;
            if (frame != null)
            {
                TimeFrame = TimeFrames.FirstOrDefault(wrapper => wrapper.Value.RangeMinutes == frame.RangeMinutes && wrapper.Value.Steps == frame.Steps);
            }

            if (TimeFrame == null)
            {
                TimeFrame = TimeFrames.First();
            }
        }

        private void InvalidateHistory()
        {
            IsHistoryValid = false;
            foreach (var summary in _summaries)
            {
                summary.IsLoaded = false;
            }
        }

        private void InvalidateCurrent()
        {
            IsCurrentValid = false;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedCurrency) && SelectedCurrency != null)
            {
                InvalidateCurrent();
                InvalidateHistory();

                // Save pref
                var pref = _preferencesService.DisplayPreference;
                pref.PortfolioTargetCurrency = SelectedCurrency;
                var prefSaving = _preferencesService.UpdateDisplayPreference(pref);

                await EnsurePortfolioUpToDate().ConfigureAwait(false);
                await prefSaving.ConfigureAwait(false);
            }
            else if (e.PropertyName == nameof(TimeFrame))
            {
                InvalidateHistory();

                // Save pref
                var pref = _preferencesService.DisplayPreference;
                pref.PortfolioChartPreference = TimeFrame.Value;
                var prefSaving = _preferencesService.UpdateDisplayPreference(pref);

                InitializeAmountHistory();
                await EnsurePortfolioUpToDate().ConfigureAwait(false);
                await prefSaving.ConfigureAwait(false);
            }
            else if (e.PropertyName == nameof(Mode))
            {
                var pref = _preferencesService.DisplayPreference;
                pref.PortfolioViewMode = Mode;
                await _preferencesService.UpdateDisplayPreference(pref).ConfigureAwait(false);
            }
            else if (e.PropertyName == nameof(IsCurrentValid))
            {
                OnPropertyChanged(nameof(IsCurrentLoading));
            }
            else if (e.PropertyName == nameof(IsHistoryValid))
            {
                OnPropertyChanged(nameof(IsHistoryLoading));
            }
        }

        /// <summary>
        ///     Load settings that can be modified between OnNavigatedFrom and OnNavigatedTo because the VM support multiple
        ///     activation.
        /// </summary>
        private void LoadDynamicPreferences()
        {
            var currs = UserPreferencesService.OrderedCurrencies.ToList();
            Currencies.CompleteElements(currs, EqualityComparer<string>.Default);

            var selectedCur = _preferencesService.DisplayPreference.PortfolioTargetCurrency ?? DefaultTargetCurrency;
            if (!Currencies.Contains(selectedCur))
            {
                selectedCur = Currencies.First();
            }

            _selectedCurrency = selectedCur;
        }

        private async void OnTimerTick(object sender, object e)
        {
            await EnsurePortfolioUpToDate().ConfigureAwait(false);
        }

        public override async void OnNavigatedTo(object parameter)
        {
            _isViewActive = true;
            LoadDynamicPreferences();
            await EnsureLocalDataLoaded();
            await EnsurePortfolioUpToDate();
            StartUpdateTimer();
        }

        public override void OnNavigatedFrom(object parameter)
        {
            _isViewActive = false;
            StopUpdateTimer();
        }

        public override void OnActivated()
        {
            _isViewActive = true;
            StartUpdateTimer();
        }

        public override void OnDeactivated()
        {
            _isViewActive = false;
            StopUpdateTimer();
        }

        public void StartUpdateTimer()
        {
            if (_isViewActive)
            {
                _timer.Start();
            }
        }

        public void StopUpdateTimer()
        {
            _timer.Stop();
        }

        private bool IsHistoryRatesUpToDate()
        {
            return IsHistoryValid && DateTime.Now < _historyUpdateTime + TimeSpan.FromMinutes(TimeFrame.Value.RangeMinutes / 30d);
        }

        private bool IsCurentRatesUpToDate()
        {
            return IsCurrentValid && DateTime.Now < _currentUpdateTime + CurrentRefreshRate;
        }

        public void UpdateStats()
        {
            if (IsCurrentLoaded)
            {
                Holdings = HoldingsSummaries.Sum(info => info.Amount * info.Rate);
                Investments = Math.Max(_investmentsSummaries.Sum(summary => -summary.Amount * summary.Rate), 0d);
                ReleasedProfit = Math.Max(_investmentsSummaries.Sum(summary => summary.Amount * summary.Rate), 0d);
                Roi = Holdings - Investments;
                RoiPercent = Roi / Investments;
                SortHoldings();
            }

            double min = double.MaxValue, max = double.MinValue;

            var holdingsHistory = new SummarySnapshot[Steps];
            var investmentsHistory = new SummarySnapshot[Steps];
            for (var i = Steps - 1; i >= 0; i--)
            {
                var totalHoldings = 0d;
                var totalInvestments = 0d;
                foreach (var summary in _summaries)
                {
                    if (summary.IsLoaded)
                    {
                        if (_preferencesService.IsInvestmentCurrency(summary.CurrencyCode))
                        {
                            var summaryValue = summary.AmountHistory[i] * summary.Rate;
                            totalInvestments -= summaryValue;
                        }
                        else
                        {
                            var summaryValue = summary.AmountHistory[i] * summary.RateHistory[i];
                            totalHoldings += summaryValue;
                        }
                        
                    }
                }

                var investments = Math.Max(totalInvestments, 0d);
                holdingsHistory[i] = new SummarySnapshot {Value = totalHoldings, Date = DatesHistory[i]};
                investmentsHistory[i] = new SummarySnapshot {Value = investments, Date = DatesHistory[i]};

                min = Math.Min(min, totalHoldings);
                max = Math.Max(max, totalHoldings);
            }

            HoldingsHistory = holdingsHistory.ToList();
            InvestmentsHistory = investmentsHistory.ToList();
            BuildLoseAreas();

            Min = min;
            Max = max;

            Change = HoldingsHistory.Last().Value - HoldingsHistory.First().Value;
            ChangePercent = HoldingsHistory.First().Value == 0d ? 0d : Change / HoldingsHistory.First().Value;
            IsHistoryLoaded = HoldingsSummaries.All(info => info.IsLoaded);
        }

        private void BuildLoseAreas()
        {
            var ranges = new List<ValueRange>();
            bool isLoseAreaOpened = false;
            for (var i = Steps - 1; i >= 0; i--)
            {
                var curDate = DatesHistory[i];
                var curHoldings = HoldingsHistory[i].Value;
                var curInvestments = InvestmentsHistory[i].Value;

                if (HoldingsHistory[i].Value < InvestmentsHistory[i].Value)
                {
                    if (isLoseAreaOpened)
                    {
                        //continue the area
                        ranges.Add(new ValueRange(){Low = curHoldings, High = curInvestments, Date = curDate});
                    }
                    else
                    {
                        //open the area
                        if (i < DatesHistory.Count - 1)
                        {
                            //opening not from the start
                            var prevDate = DatesHistory[i + 1];
                            var prevHoldings = HoldingsHistory[i + 1].Value;
                            var prevInvestments = InvestmentsHistory[i + 1].Value;

                            var intersect = Intersection(new Point(0d, curHoldings), new Point(1d, prevHoldings), new Point(0d, curInvestments), new Point(1d, prevInvestments));

                            var delta = (long)((prevDate - curDate).Ticks * intersect.X);
                            ranges.Add(new ValueRange(){Low = intersect.Y, High = intersect.Y-10, Date = curDate + TimeSpan.FromTicks(delta)});
                        }
                        ranges.Add(new ValueRange(){Low = curHoldings, High = curInvestments, Date = curDate});

                        isLoseAreaOpened = true;
                    }
                }
                else
                {
                    if (isLoseAreaOpened)
                    {
                        //close the area
                        var prevDate = DatesHistory[i + 1];
                        var prevHoldings = HoldingsHistory[i + 1].Value;
                        var prevInvestments = InvestmentsHistory[i + 1].Value;

                        var intersect = Intersection(new Point(0d, curHoldings), new Point(1d, prevHoldings), new Point(0d, curInvestments), new Point(1d, prevInvestments));

                        var delta = (long)((prevDate - curDate).Ticks * intersect.X);
                        ranges.Add(new ValueRange(){Low = intersect.Y, High = intersect.Y-10, Date = curDate + TimeSpan.FromTicks(delta)});
                        
                        ranges.Add(new ValueRange(){Low = null, High = null, Date = curDate});

                        isLoseAreaOpened = false;
                    }
                    else
                    {
                        ranges.Add(new ValueRange(){Low = null, High = null, Date = curDate});
                    }
                }
            }
            

            LoseAreas = Enumerable.Reverse(ranges).ToArray();
        }

        private Point Intersection(Point p1, Point p2, Point p3, Point p4)
        {
            var x1 = p1.X;
            var y1 = p1.Y;

            var x2 = p2.X;
            var y2 = p2.Y;

            var x3 = p3.X;
            var y3 = p3.Y;

            var x4 = p4.X;
            var y4 = p4.Y;

            var xi = ((x1 * y2 - y1 * x2)*(x3 - x4) - (x1 - x2)*(x3 * y4 - y3 * x4)) / ((x1 - x2)*(y3 - y4) - (y1 - y2)*(x3 - x4));
            var yi = ((x1 * y2 - y1 * x2)*(y3 - y4) - (y1 - y2)*(x3 * y4 - y3 * x4)) / ((x1 - x2)*(y3 - y4) - (y1 - y2)*(x3 - x4));
            return new Point(xi,yi);
        }


        private Color GetCurrencyColor(string currency, int index)
        {
            if (ColorsMap.TryGetValue(currency, out var color))
            {
                return color;
            }

            if (index < StaticColors.Length)
            {
                return StaticColors[index].ToColor();
            }

            return Color.FromArgb(255, (byte) _random.Next(256), (byte) _random.Next(256), (byte) _random.Next(256));
        }

        /*
         int n = 12;

Color baseColor = System.Drawing.ColorTranslator.FromHtml("#8A56E2");
double baseHue = (new HSLColor(baseColor)).Hue;

List<Color> colors = new List<Color>();
colors.Add(baseColor);

double step = (240.0 / (double)n);

for (int i = 1; i < n; ++i)
{
    HSLColor nextColor = new HSLColor(baseColor);
    nextColor.Hue = (baseHue + step * ((double)i)) % 240.0;
    colors.Add((Color)nextColor);
}

string colors = string.Join(",", colors.Select(e => e.Name.Substring(2)).ToArray());
         */
    }
}
