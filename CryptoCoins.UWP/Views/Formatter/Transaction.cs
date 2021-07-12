using System;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.ViewModels.Entities;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class Transaction
    {
        public static string SummaryBase(string baseCurrency, decimal amount, TransactionType type)
        {
            switch (type)
            {
                case TransactionType.Sell:
                    return string.Format("PortfolioPage_SellDescription_Base".GetLocalized(), Currency.FormatRateAndCode((double) amount, baseCurrency));
                case TransactionType.Buy:
                    return string.Format("PortfolioPage_BuyDescription_Base".GetLocalized(), Currency.FormatRateAndCode((double) amount, baseCurrency));
                case TransactionType.AirDrop:
                    return string.Format("PortfolioPage_AirdropDescription_Base".GetLocalized(), Currency.FormatRateAndCode((double) amount, baseCurrency));
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static string SummaryCounter(string counterCurrency, decimal amount, TransactionType type, decimal price)
        {
            switch (type)
            {
                case TransactionType.Sell:
                    return string.Format("PortfolioPage_SellDescription_Counter".GetLocalized(),Currency.FormatRateAndCode((double) (amount * price), counterCurrency));
                case TransactionType.Buy:
                    return string.Format("PortfolioPage_BuyDescription_Counter".GetLocalized(), Currency.FormatRateAndCode((double) (amount * price), counterCurrency));
                case TransactionType.AirDrop:
                    return string.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
