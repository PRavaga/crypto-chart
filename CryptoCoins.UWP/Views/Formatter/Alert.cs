using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class Alert
    {
        public static string FormatTarget(double amount, string symbol, AlertTargetMode mode)
        {
            string modeSymbol;
            switch (mode)
            {
                case AlertTargetMode.Above:
                    modeSymbol = ">";
                    break;
                case AlertTargetMode.Below:
                    modeSymbol = "<";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            return $"{modeSymbol}{symbol}{amount}";
        }

        public static string FormatFrequency(AlertFrequency frequency)
        {
            switch (frequency)
            {
                case AlertFrequency.OneTime:
                    return "AlertFrequency_OneTime".GetLocalized();
                case AlertFrequency.EveryTime:
                    return "AlertFrequency_EveryTime".GetLocalized();
                default:
                    throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null);
            }
        }
    }
}
