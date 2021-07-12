using System;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Platform;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class Currency
    {
        public const int DefaultRateLength = 12;
        public const int DefaultRateAndCodeLength = 15;
        public static string FormatName(string name, string code)
        {
            return $"{name} ({code})";
        }

        public static string FormatFromTo(string from, string to)
        {
            return $"{@from}-{to}";
        }

        public static string FormatVolume24Long(double volume24, string code)
        {
            var symbol = CurrencySymbol(code);
            var formattedVolume = Value.FormatNumber(volume24, DefaultRateLength - symbol.Length, 2);
            return String.Format("DashboardPage_VolumeTemplate".GetLocalized(), FormatValueAndSymbol(formattedVolume, symbol));
        }


        public static string FormatVolume24(double volume24, string code)
        {
            var symbol = CurrencySymbol(code);
            var formattedVolume = Value.FormatNumber(volume24, DefaultRateLength - symbol.Length, 2);
            return FormatValueAndSymbol(formattedVolume, symbol);
        }

        public static string FormatConversionName(string fromName, string fromCode, string toCode)
        {
            return $"{fromName} ({fromCode}) - {toCode}";
        }

        public static string FormatVolume24Abbr(double volume24, string fromSymbol)
        {
            return String.Format("DashboardPage_VolumeTemplate".GetLocalized(), $"{fromSymbol} {FormatHelper.ToEngineeringNotation(volume24)}");
        }

        public static string FormatRateAndCode(double amount, string code, int length)
        {
            if (double.IsNaN(amount))
            {
                return "RateNotAvailable".GetLocalized();
            }
            
            string formatterValue;
            if (code == "BTC")
            {
                // Custom formatting for BTC to always show 1 satoshi.
                formatterValue = amount.ToString("F8");
            }
            else
            {
                var maxFriction = MaxFriction(code, amount);
                formatterValue = Value.FormatNumber(amount, length - code.Length, maxFriction);
            }
            return FormatValueAndCode(formatterValue, code);
        }
        public static string FormatRate(double amount, string code, int length)
        {
            if (double.IsNaN(amount))
            {
                return "RateNotAvailable".GetLocalized();
            }

            var symbol = CurrencySymbol(code);
            string formatterValue;
            if (code == "BTC")
            {
                // Custom formatting for BTC to always show 1 satoshi.
                formatterValue = amount.ToString("F8");
            }
            else
            {
                var maxFriction = MaxFriction(code, amount);
                formatterValue = Value.FormatNumber(amount, length - symbol.Length, maxFriction);
            }
            return FormatValueAndSymbol(formatterValue, symbol);
        }

        private static int MaxFriction(string currencyCode, double value)
        {
            if (CurrencyHelper.IsFiatCurrency(currencyCode))
            {
                if (value <= 1d)
                {
                    return 4;
                }

                return 2;
            }
            return Int32.MaxValue;
        }

        private static string FormatValueAndSymbol(string value, string symbol)
        {
            return $"{symbol} {value}";
        }
        private static string FormatValueAndCode(string value, string code)
        {
            return $"{value} {code}";
        }


        public static string FormatRate(double amount, string code)
        {
            return FormatRate(amount, code, DefaultRateLength);
        }
        public static string FormatRateAndCode(double amount, string code)
        {
            return FormatRateAndCode(amount, code, DefaultRateAndCodeLength);
        }

        public static string FormatRate(double rate, double amount, string code, int length)
        {
            return FormatRate(amount * rate, code, length);
        }

        public static string FormatChangeValue(double changeAmount, string code, bool isLoaded)
        {
            if (!isLoaded)
            {
                return "";
            }
            return FormatChangeValue(changeAmount, code);
        }

        public static string FormatChangeValue(double changeAmount, string code)
        {
            return FormatRate(changeAmount, code, 8);
        }

        public static string FormatChangeValue(double changeAmount, double quantity, string code, bool isLoaded)
        {
            return FormatChangeValue(changeAmount * quantity, code, isLoaded);
        }

        public static string FormatChangePercent(double changePercent, bool isLoaded)
        {
            if (!isLoaded)
            {
                return "PortfolioPage_ChangePercentLoading".GetLocalized();
            }
            return FormatChangePercent(changePercent);
        }
        public static string FormatChangePercent(double changePercent)
        {
            return changePercent == 0d ? $"({changePercent:P2})" : $"{SignSymbol(changePercent)} ({changePercent:P2})";
        }

        public static string FormatRoiPercent(double roiPercent)
        {
            return roiPercent == 0d ? $"({roiPercent:P2})" : $"{SignSymbol(roiPercent)} ({roiPercent:P2})";
        }

        public static string SignSymbol(double value)
        {
            return value > 0 ? "\u25B2" : "\u25BC";
        }

        public static string FormatMax(double max, string toSymbol)
        {
            var symbol = CurrencySymbol(toSymbol);
            var value =  $"{symbol}{Value.FormatNumber(max, 11 - symbol.Length)}";
            return string.Format("PortfolioPage_Max".GetLocalized(), value);
        }

        public static string FormatMin(double min, string toSymbol)
        {
            var symbol = CurrencySymbol(toSymbol);
            var value = $"{symbol}{Value.FormatNumber(min, 11 - symbol.Length)}";
            return string.Format("PortfolioPage_Min".GetLocalized(), value);
        }

        public static string CurrencySymbol(string currencyCode)
        {
            /*if (_coins.TryGetValue(currencyCode, out var info))
            {
                return info.Code;
            }*/
            if (CurrencyHelper.TryGetCurrencySymbol(currencyCode, out var symbol))
            {
                return symbol;
            }
            return currencyCode.Length > 0 ? currencyCode.Substring(0, 1) : String.Empty;
        }

        public static string FormatInfo(string name)
        {
            return string.Format("DashboardPage_CoinInfo".GetLocalized(), name);
        }
    }
}
