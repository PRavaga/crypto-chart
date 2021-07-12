using System;
using CryptoCoins.UWP.Helpers;
using SQLite;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    [Obsolete]
    public class CurrencyHoldings : Observable
    {
        private double _amount;
        private string _currency;

        [PrimaryKey]
        public string Currency
        {
            get => _currency;
            set => Set(ref _currency, value);
        }

        public double Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }
    }
}
