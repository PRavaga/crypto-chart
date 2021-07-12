using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels
{
    public class AlertDialogViewModel : DialogViewModelBase
    {
        public enum DialogMode
        {
            Creating,
            Editing
        }

        public enum ResultType
        {
            Save,
            Remove,
            Cancel
        }

        private readonly CryptoService _cryptoService;
        private readonly UserPreferencesService _userPreferencesService;
        private decimal? _amount;
        private RelayCommand _cancelCommand;
        private string _coinQuery;
        private FilterCollection<CryptoCurrencyInfo> _coins;
        private AsyncCommand _deleteCommand;
        private AlertModel _editedAlert;
        private RelayCommand<string> _filterSuggestions;
        private AlertFrequency _frequency;
        private DialogMode _mode = DialogMode.Editing;
        private CustomAsyncCommand _saveCommand;
        private CryptoCurrencyInfo _selectedCoin;
        private string _targetCode;
        private AlertTargetMode _targetMode;

        private RelayCommand<AlertFrequency> _toggleFrequency;

        public AlertDialogViewModel(CryptoService cryptoService, UserPreferencesService userPreferencesService)
        {
            _cryptoService = cryptoService;
            _userPreferencesService = userPreferencesService;
            TargetCode = Currencies.FirstOrDefault();
            PropertyChanged += OnPropertyChanged;
        }

        public DialogMode Mode
        {
            get => _mode;
            set => Set(ref _mode, value);
        }

        public decimal? Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }

        public CryptoCurrencyInfo SelectedCoin
        {
            get => _selectedCoin;
            set => Set(ref _selectedCoin, value);
        }

        public FilterCollection<CryptoCurrencyInfo> Coins
        {
            get => _coins;
            set => Set(ref _coins, value);
        }

        public string TargetCode
        {
            get => _targetCode;
            set => Set(ref _targetCode, value);
        }

        public AlertTargetMode TargetMode
        {
            get => _targetMode;
            set => Set(ref _targetMode, value);
        }

        public AlertFrequency Frequency
        {
            get => _frequency;
            set => Set(ref _frequency, value);
        }

        public List<string> Currencies { get; } = UserPreferencesService.OrderedCurrencies.ToList();
        public AlertTargetMode[] Modes { get; } = (AlertTargetMode[]) Enum.GetValues(typeof(AlertTargetMode));

        public CustomAsyncCommand SaveCommand => _saveCommand ?? (_saveCommand = new CustomAsyncCommand(async () =>
        {
            SaveCommand.OnCanExecuteChanged();
            if (_editedAlert != null)
            {
                _editedAlert.TargetValue = (double) Amount.GetValueOrDefault();
                _editedAlert.TargetMode = TargetMode;
                _editedAlert.ToCode = TargetCode;
                _editedAlert.FromCode = SelectedCoin.Code;
                _editedAlert.Frequency = Frequency;
                _editedAlert.IsArmed = true;
                await _userPreferencesService.UpdateAlert(_editedAlert);
            }
            else
            {
                _editedAlert = new AlertModel
                {
                    TargetValue = (double) Amount.GetValueOrDefault(),
                    TargetMode = TargetMode,
                    ToCode = TargetCode,
                    FromCode = SelectedCoin.Code,
                    Frequency = Frequency,
                    IsEnabled = true,
                    IsArmed = true
                };
                await _userPreferencesService.AddAlert(_editedAlert);
            }
            DialogController.Hide(new Result {Type = ResultType.Save, Alert = _editedAlert});
        }, () => !SaveCommand.IsExecuting && SelectedCoin != null && string.Equals(SelectedCoin.Code, CoinQuery, StringComparison.Ordinal) && Amount.HasValue && Amount != decimal.Zero));

        public AsyncCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new AsyncCommand(async () =>
        {
            if (_editedAlert != null)
            {
                await _userPreferencesService.DeleteAlert(_editedAlert);
                DialogController.Hide(new Result {Type = ResultType.Remove, Alert = _editedAlert});
            }
        }));

        public RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(() => { DialogController.Hide(new Result {Type = ResultType.Cancel}); }));

        public RelayCommand<string> FilterSuggestions => _filterSuggestions ?? (_filterSuggestions = new RelayCommand<string>(e =>
        {
            CoinQuery = e;
            Coins.Filter();
            SaveCommand.OnCanExecuteChanged();
        }));

        public RelayCommand<AlertFrequency> ToggleFrequency => _toggleFrequency ?? (_toggleFrequency = new RelayCommand<AlertFrequency>(e => { Frequency = e; }));

        public string CoinQuery
        {
            get => _coinQuery;
            set => Set(ref _coinQuery, value);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Amount))
            {
                SaveCommand.OnCanExecuteChanged();
            }
        }

        public override async void OnNavigatedTo(object parameter)
        {
            if (parameter is Argument arg)
            {
                try
                {
                    if (arg.Alert != null)
                    {
                        _editedAlert = arg.Alert;
                        Mode = DialogMode.Editing;
                        Amount = (decimal) _editedAlert.TargetValue;
                        TargetMode = _editedAlert.TargetMode;
                        Frequency = _editedAlert.Frequency;
                        TargetCode = _editedAlert.ToCode;
                        var coins = await _cryptoService.GetCoins(false);
                        Coins = new FilterCollection<CryptoCurrencyInfo>(coins, new LambdaComparer<CryptoCurrencyInfo>((x, y) => x.RankOrder.CompareTo(y.RankOrder)));
                        Coins.FilterFunc = info => string.IsNullOrEmpty(CoinQuery) ||
                                                   info.Name.IndexOf(CoinQuery, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                   info.Code.IndexOf(CoinQuery, StringComparison.OrdinalIgnoreCase) != -1;
                        SelectedCoin = Coins.FirstOrDefault(info => info.Code == arg.Alert.FromCode);
                        CoinQuery = SelectedCoin?.Code;
                    }
                    else
                    {
                        Mode = DialogMode.Creating;
                        var coins = await _cryptoService.GetCoins(false);
                        Coins = new FilterCollection<CryptoCurrencyInfo>(coins, new LambdaComparer<CryptoCurrencyInfo>((x, y) => x.RankOrder.CompareTo(y.RankOrder)));
                        Coins.FilterFunc = info => string.IsNullOrEmpty(CoinQuery) ||
                                                   info.Name.IndexOf(CoinQuery, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                   info.Code.IndexOf(CoinQuery, StringComparison.OrdinalIgnoreCase) != -1;
                        SelectedCoin = Coins.FirstOrDefault();
                    }
                }
                catch (ApiException)
                {
                    DialogController.Hide(new Result {Type = ResultType.Cancel});
                }
            }
            else
            {
                DialogController.Hide(new Result {Type = ResultType.Cancel});
            }
        }

        public override void OnCancelled()
        {
            DialogController.Result = new Result {Type = ResultType.Cancel};
        }

        public class Result
        {
            public AlertModel Alert;
            public ResultType Type;
        }

        public class Argument
        {
            public AlertModel Alert;
        }
    }
}
