using System;
using System.Collections.Generic;
using Windows.UI;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services.Entries;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels.Entities
{
    public class HoldingsSummary : Observable
    {
        private double _amount;
        private double _change;
        private double _changePercent;
        private Color _chartColor;
        private string _currencyCode;
        private string _currencyName;
        private double _investments;
        private bool _isLoaded;
        private double _max;
        private double _min;
        private AsyncCommand _openEditDialogCommand;
        private double _rate;
        private double[] _rateHistory;
        private double _value;

        public Color ChartColor
        {
            get => _chartColor;
            set => Set(ref _chartColor, value);
        }


        public string CurrencyCode
        {
            get => _currencyCode;
            set => Set(ref _currencyCode, value);
        }

        public string CurrencyName
        {
            get => _currencyName;
            set => Set(ref _currencyName, value);
        }

        private string _counterCurrencyCode;

        public string CounterCurrencyCode
        {
            get => _counterCurrencyCode;
            set => Set(ref _counterCurrencyCode, value);
        }

        public double Investments
        {
            get => _investments;
            set => Set(ref _investments, value);
        }

        public double Amount
        {
            get => _amount;
            set
            {
                Set(ref _amount, value);
                OnPropertyChanged(nameof(Value));
            }
        }

        public double Value => Amount * Rate;

        public double Rate
        {
            get => _rate;
            set
            {
                Set(ref _rate, value);
                OnPropertyChanged(nameof(Value));
            }
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

        public bool IsLoaded
        {
            get => _isLoaded;
            set => Set(ref _isLoaded, value);
        }

        public AsyncCommand OpenEditDialogCommand
        {
            get => _openEditDialogCommand;
            set => Set(ref _openEditDialogCommand, value);
        }

        public double[] RateHistory
        {
            get => _rateHistory;
            set => Set(ref _rateHistory, value);
        }

        private double[] _amountHistory;

        public double[] AmountHistory
        {
            get => _amountHistory;
            set => Set(ref _amountHistory, value);
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

        private Uri _icon;

        public Uri Icon
        {
            get => _icon;
            set => Set(ref _icon, value);
        }

        public void UpdateFromCurrentRates(ConversionInfo conversionInfo)
        {
            Rate = conversionInfo.Rate;
            CurrencyName = conversionInfo.FromFullName;
            Icon = conversionInfo.FromIcon;
        }

        public void UpdateChangeFromCurrentRates(ConversionInfo conversionInfo)
        {
            Change = conversionInfo.ChangeValue;
            ChangePercent = conversionInfo.Change24;
        }
    }
}
