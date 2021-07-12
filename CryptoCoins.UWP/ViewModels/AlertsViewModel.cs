using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.Views;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels
{
    public class AlertsViewModel : ViewModelBase
    {
        private readonly UserPreferencesService _preferencesService;
        private readonly DialogService _dialogService;
        private readonly CryptoService _cryptoService;
        private ObservableCollection<AlertModel> _alerts;

        private DataState _dataState = DataState.NotReady;
        private bool _isAlertsEnabled;
        private RelayCommand _openCreatingDialogCommand;
        private AsyncCommand _openEditingDialogCommand;
        private string _searchQuery;
        private Dictionary<string, CryptoCurrencyInfo> _coinsLookup;

        public AlertsViewModel(UserPreferencesService preferencesService, DialogService dialogService, CryptoService cryptoService)
        {
            _preferencesService = preferencesService;
            _dialogService = dialogService;
            _cryptoService = cryptoService;
            Alerts = new ObservableCollection<AlertModel>();
            Alerts.CollectionChanged += OnCollectionChanged;
            IsAlertsEnabled = _preferencesService.DisplayPreference.IsAlertsEnabled;
            PropertyChanged += OnPropertyChanged;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (AlertModel item in e.NewItems)
                    item.PropertyChanged += OnAlertModelChanged;

            if (e.OldItems != null)
                foreach (AlertModel item in e.OldItems)
                    item.PropertyChanged -= OnAlertModelChanged;
        }

        private async void OnAlertModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AlertModel.IsEnabled))
            {
                await _preferencesService.UpdateAlert((AlertModel) sender).ConfigureAwait(false);
            }
        }


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsAlertsEnabled))
            {
                var pref =_preferencesService.DisplayPreference;
                pref.IsAlertsEnabled = IsAlertsEnabled;
                _preferencesService.UpdateDisplayPreference(pref);
            }
            if (e.PropertyName == nameof(SearchQuery))
            {
                //Alerts.Filter();
                if (Alerts.Count == 0)
                {
                    DataState = DataState.FilteredEmpty;
                } else
                {
                    DataState = DataState.Available;
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => Set(ref _searchQuery, value);
        }

        public bool IsAlertsEnabled
        {
            get => _isAlertsEnabled;
            set => Set(ref _isAlertsEnabled, value);
        }

        public RelayCommand OpenCreatingDialogCommand =>
            _openCreatingDialogCommand ?? (_openCreatingDialogCommand = new RelayCommand(async () =>
            {
                var result =(AlertDialogViewModel.Result) await _dialogService.ShowAsync<AlertDialog>(new AlertDialogViewModel.Argument());
                if (result.Type == AlertDialogViewModel.ResultType.Save)
                {
                    UpdateCoinInfo(result.Alert);
                    Alerts.Add(result.Alert);
                    DataState = DataState.Available;
                }
            }));

        public AsyncCommand OpenEditingDialogCommand =>
            _openEditingDialogCommand ?? (_openEditingDialogCommand = new AsyncCommand(async (e) =>
            {
                var alert = (AlertModel) e;
                var result = (AlertDialogViewModel.Result)await _dialogService.ShowAsync<AlertDialog>(new AlertDialogViewModel.Argument(){Alert = alert});
                if (result.Type == AlertDialogViewModel.ResultType.Save)
                {
                    UpdateCoinInfo(result.Alert);
                } else if (result.Type == AlertDialogViewModel.ResultType.Remove)
                {
                    Alerts.Remove(result.Alert);
                    if (Alerts.Count == 0)
                    {
                        DataState = DataState.Empty;
                    }
                }
            }));

        public ObservableCollection<AlertModel> Alerts
        {
            get => _alerts;
            set => Set(ref _alerts, value);
        }

        public DataState DataState
        {
            get => _dataState;
            set => Set(ref _dataState, value);
        }

        public ProgressState ProgressState { get; } = new ProgressState();

        public override async void OnNavigatedTo(object parameter)
        {
            await LoadAlarms().ConfigureAwait(false);
        }

        private void UpdateCoinInfo(AlertModel alert)
        {
            if (_coinsLookup != null && _coinsLookup.TryGetValue(alert.FromCode, out var coin))
            {
                alert.FromIcon = coin.Icon;
                alert.FromName = coin.Name;
            }
            if (CurrencyHelper.TryGetCurrencySymbol(alert.ToCode, out var symbol))
            {
                alert.ToSymbol = symbol;
            }
        }

        private async Task LoadAlarms()
        {
            using (ProgressState.BeginOperation())
            {
                DataState = DataState.NotReady;
                Alerts.Clear();
                var alerts = await _preferencesService.GetAlerts();
                try
                {
                    var coins = await _cryptoService.GetCoins(false);
                    _coinsLookup = coins.ToDictionary(info => info.Code);
                    foreach (var alert in alerts)
                    {
                        UpdateCoinInfo(alert);
                    }
                }
                catch (ApiException)
                {
                    
                }
                foreach (var alert in alerts)
                {
                    Alerts.Add(alert);
                }
                if (Alerts.Count > 0)
                {
                    DataState = DataState.Available;
                }
                else
                {
                    DataState = DataState.Empty;
                }
            }
        }
    }
}
