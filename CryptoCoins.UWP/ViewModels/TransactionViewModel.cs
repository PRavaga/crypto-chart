using System;
using System.ComponentModel;
using System.Linq;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Exceptions.Validation;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.ViewModels.Converters;
using CryptoCoins.UWP.ViewModels.Entities;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels
{
    public class TransactionViewModel : DialogViewModelBase
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
        private readonly HoldingsService _holdingsService;
        private readonly UserPreferencesService _userPreferencesService;
        private FilterCollection<CryptoCurrencyInfo> _baseCurrencies;
        private CryptoCurrencyInfo _baseCurrency;
        private RelayCommand _cancelCommand;
        private FilterCollection<CryptoCurrencyInfo> _counterCurrencies;
        private CryptoCurrencyInfo _counterCurrency;
        private AsyncCommand _deleteCommand;
        private RelayCommand<string> _filterSuggestions;
        private DialogMode _mode = DialogMode.Editing;
        private CustomAsyncCommand _saveCommand;
        private readonly HoldingsConverter _holdingsConverter;

        private HoldingsTransaction _transaction;

        public TransactionViewModel(CryptoService cryptoService, UserPreferencesService userPreferencesService,
            HoldingsService holdingsService, HoldingsConverter holdingsConverter)
        {
            _cryptoService = cryptoService;
            _userPreferencesService = userPreferencesService;
            _holdingsService = holdingsService;
            _holdingsConverter = holdingsConverter;
            PropertyChanged += OnPropertyChanged;
        }

        public TransactionType[] TransactionTypes { get; } = (TransactionType[]) Enum.GetValues(typeof(TransactionType));

        public DialogMode Mode
        {
            get => _mode;
            set => Set(ref _mode, value);
        }

        public HoldingsTransaction Transaction
        {
            get => _transaction;
            set => Set(ref _transaction, value);
        }

        public CryptoCurrencyInfo BaseCurrency
        {
            get => _baseCurrency;
            set => Set(ref _baseCurrency, value);
        }

        public CryptoCurrencyInfo CounterCurrency
        {
            get => _counterCurrency;
            set => Set(ref _counterCurrency, value);
        }

        public FilterCollection<CryptoCurrencyInfo> BaseCurrencies
        {
            get => _baseCurrencies;
            set => Set(ref _baseCurrencies, value);
        }

        public FilterCollection<CryptoCurrencyInfo> CounterCurrencies
        {
            get => _counterCurrencies;
            set => Set(ref _counterCurrencies, value);
        }

        public CustomAsyncCommand SaveCommand => _saveCommand ?? (_saveCommand = new CustomAsyncCommand(async () =>
                                                     {
                                                         if (!Transaction.Validate())
                                                         {
                                                             return;
                                                         }
                                                         SaveCommand.OnCanExecuteChanged();
                                                         try
                                                         {
                                                             if (Mode == DialogMode.Editing)
                                                             {
                                                                 await _holdingsService.UpdateTransaction(_holdingsConverter.Convert(Transaction));
                                                             }
                                                             else
                                                             {
                                                                 await _holdingsService.AddTransaction(_holdingsConverter.Convert(Transaction));
                                                             }

                                                             DialogController.Hide(new Result
                                                             {
                                                                 Type = ResultType.Save,
                                                                 Holdings = Transaction
                                                             });
                                                         }
                                                         catch (InsufficientHoldingsException e)
                                                         {
                                                             Transaction.Properties[nameof(HoldingsTransaction.Amount)].Errors.Add("TransactionDialog_ValidationInsufficientFunds".GetLocalized());
                                                         }
                                                     },
                                                     () => { return !SaveCommand.IsExecuting && Transaction != null && Transaction.IsValid; }));

        public AsyncCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new AsyncCommand(async () =>
        {
            // Transaction is null if it's invoked before it's loaded asynchronous
            if (Transaction != null && Mode == DialogMode.Editing)
            {
                try
                {
                    await _holdingsService.DeleteTransaction(_holdingsConverter.Convert(Transaction));
                    DialogController.Hide(new Result {Type = ResultType.Remove, Holdings = Transaction});
                }
                catch (InsufficientHoldingsException e)
                {
                    Transaction.Properties[nameof(HoldingsTransaction.Amount)].Errors.Add("TransactionDialog_ValidationInsufficientFunds".GetLocalized());
                }
            }
        }));

        public RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(() => { DialogController.Hide(new Result {Type = ResultType.Cancel}); }));

        public RelayCommand<string> FilterSuggestions => _filterSuggestions ?? (_filterSuggestions = new RelayCommand<string>(e =>
        {
            Transaction.BaseCode = e;
            BaseCurrencies.Filter();
            SaveCommand.OnCanExecuteChanged();
        }));


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            SaveCommand.OnCanExecuteChanged();
        }

        public override async void OnNavigatedTo(object parameter)
        {
            if (parameter is Argument arg)
            {
                try
                {
                    if (arg.TransactionId != null)
                    {
                        Mode = DialogMode.Editing;
                        Transaction = _holdingsConverter.Convert(await _holdingsService.GetTransaction(arg.TransactionId.Value));
                    }
                    else
                    {
                        Mode = DialogMode.Creating;
                        Transaction = new HoldingsTransaction()
                        {
                            Date = DateTimeOffset.Now,
                            Type = TransactionType.Buy
                        };
                        if (!string.IsNullOrWhiteSpace(arg.CurrencyCode))
                        {
                            Transaction.BaseCode = arg.CurrencyCode;
                        }
                    }

                    var coins = await _cryptoService.GetCoins(false);
                    var fiatCurrencies = UserPreferencesService.FiatCurrencies.Select(s =>
                        new CryptoCurrencyInfo {Code = s, Name = CurrencyHelper.FiatCurrencyName(s), Icon = _cryptoService.CryptoCurrencyIcon(s).ToString()}).ToList();
                    BaseCurrencies = new FilterCollection<CryptoCurrencyInfo>(coins.Concat(fiatCurrencies),
                        new LambdaComparer<CryptoCurrencyInfo>((x, y) => x.RankOrder.CompareTo(y.RankOrder)));
                    CounterCurrencies = new FilterCollection<CryptoCurrencyInfo>(coins.Concat(fiatCurrencies),
                        new LambdaComparer<CryptoCurrencyInfo>((x, y) => x.RankOrder.CompareTo(y.RankOrder)));


                    Transaction.PropertyChanged += OnTransactionPropertyChanged;
                    Transaction.Validator = model => Validate((HoldingsTransaction) model);
                    BaseCurrency = BaseCurrencies.FirstOrDefault(info => info.Code == Transaction.BaseCode);
                    CounterCurrency = CounterCurrencies.FirstOrDefault(info => info.Code == Transaction.CounterCode);

                    BaseCurrencies.FilterFunc = info => string.IsNullOrEmpty(Transaction.BaseCode) ||
                                                        info.Name != null && info.Name.IndexOf(Transaction.BaseCode, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                        info.Code.IndexOf(Transaction.BaseCode, StringComparison.OrdinalIgnoreCase) != -1;
                    CounterCurrencies.FilterFunc = info => string.IsNullOrEmpty(Transaction.CounterCode) ||
                                                           info.Name != null && info.Name.IndexOf(Transaction.CounterCode, StringComparison.OrdinalIgnoreCase) != -1 ||
                                                           info.Code.IndexOf(Transaction.CounterCode, StringComparison.OrdinalIgnoreCase) != -1;
                    //Transaction.Validate();
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

        private void OnTransactionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveCommand.OnCanExecuteChanged();
        }

        public void Validate(HoldingsTransaction transaction)
        {
            if (BaseCurrencies.All(info => info.Code != transaction.BaseCode))
            {
                transaction.Properties[nameof(HoldingsTransaction.BaseCode)].Errors.Add("TransactionDialog_ValidationIncorrect".GetLocalized());
            }

            if (transaction.Amount == null)
            {
                transaction.Properties[nameof(HoldingsTransaction.Amount)].Errors.Add("TransactionDialog_ValidationRequired".GetLocalized());
            }

            if (transaction.Amount <= decimal.Zero)
            {
                transaction.Properties[nameof(HoldingsTransaction.Amount)].Errors.Add("TransactionDialog_ValidationIncorrect".GetLocalized());
            }

            if (transaction.Date > DateTimeOffset.Now)
            {
                transaction.Properties[nameof(HoldingsTransaction.Date)].Errors.Add("TransactionDialog_ValidationDateInvalid".GetLocalized());
            }

            if (transaction.Type == TransactionType.Buy || transaction.Type == TransactionType.Sell)
            {
                if (CounterCurrencies.All(info => info.Code != transaction.CounterCode))
                {
                    transaction.Properties[nameof(HoldingsTransaction.CounterCode)].Errors.Add("TransactionDialog_ValidationRequired".GetLocalized());
                }

                if (transaction.BaseCode == transaction.CounterCode)
                {
                    transaction.Properties[nameof(HoldingsTransaction.CounterCode)].Errors.Add("TransactionDialog_ValidationIncorrect".GetLocalized());
                }

                if (transaction.Price == null)
                {
                    transaction.Properties[nameof(HoldingsTransaction.Price)].Errors.Add("TransactionDialog_ValidationRequired".GetLocalized());
                }
            }
        }

        public override void OnCancelled()
        {
            DialogController.Result = new Result {Type = ResultType.Cancel};
        }

        public class Result
        {
            public HoldingsTransaction Holdings;
            public ResultType Type;
        }

        public class Argument
        {
            public int? TransactionId;
            public string CurrencyCode { get; set; }
        }
    }
}
