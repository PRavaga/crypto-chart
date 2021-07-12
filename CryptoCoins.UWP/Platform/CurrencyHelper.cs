using System.Collections.Generic;
using System.Linq;
using Windows.System.Profile;
using CryptoCoins.UWP.Models.UserPreferences;

namespace CryptoCoins.UWP.Platform
{
    public static class CurrencyHelper
    {
        public static Dictionary<string, string> CurrenciesName = new Dictionary<string, string>
        {
            {"USD", "US dollar"},
            {"EUR", "Euro"},
            {"AUD", "Australian dollar"},
            {"CAD", "Canadian dollar"},
            {"GBP", "Pound sterling"},
            {"CHF", "Swiss franc"},
            {"PLN", "Polish złoty"},
            {"CNY", "Renminbi"},
            {"KRW", "South Korean won"},
            {"JPY", "Japanese yen"},
            {"ZAR", "South African rand"},
            {"RUB", "Russian ruble"},
            {"INR", "Indian rupee"},
            {"BRL", "Brazilian real"},
            {"TRY", "Turkish lira"},
            {"THB", "Thai baht"},
            {"PHP", "Philippine peso"}
        };
        public static Dictionary<string, string> Currencies = new Dictionary<string, string>
        {
            {"AED", "د.إ.‏"},
            {"AFN", "؋ "},
            {"ALL", "Lek"},
            {"AMD", "դր."},
            {"ARS", "$"},
            {"AUD", "A$"},
            {"AZN", "man."},
            {"BAM", "KM"},
            {"BDT", "৳"},
            {"BGN", "лв."},
            {"BHD", "د.ب.‏ "},
            {"BND", "$"},
            {"BOB", "$b"},
            {"BRL", "R$"},
            {"BYR", "р."},
            {"BZD", "BZ$"},
            {"CAD", "C$"},
            {"CHF", "fr."},
            {"CLP", "$"},
            {"CNY", "¥"},
            {"COP", "$"},
            {"CRC", "₡"},
            {"CSD", "Din."},
            {"CZK", "Kč"},
            {"DKK", "kr."},
            {"DOP", "RD$"},
            {"DZD", "DZD"},
            {"EEK", "kr"},
            {"EGP", "ج.م.‏ "},
            {"ETB", "ETB"},
            {"EUR", "€"},
            {"GBP", "£"},
            {"GEL", "Lari"},
            {"GTQ", "Q"},
            {"HKD", "HK$"},
            {"HNL", "L."},
            {"HRK", "kn"},
            {"HUF", "Ft"},
            {"IDR", "Rp"},
            {"ILS", "₪"},
            {"INR", "रु"},
            {"IQD", "د.ع.‏ "},
            {"IRR", "ريال "},
            {"ISK", "kr."},
            {"JMD", "J$"},
            {"JOD", "د.ا.‏ "},
            {"JPY", "¥"},
            {"KES", "S"},
            {"KGS", "сом"},
            {"KHR", "៛"},
            {"KRW", "₩"},
            {"KWD", "د.ك.‏ "},
            {"KZT", "Т"},
            {"LAK", "₭"},
            {"LBP", "ل.ل.‏ "},
            {"LKR", "රු."},
            {"LTL", "Lt"},
            {"LVL", "Ls"},
            {"LYD", "د.ل.‏ "},
            {"MAD", "د.م.‏ "},
            {"MKD", "ден."},
            {"MNT", "₮"},
            {"MOP", "MOP"},
            {"MVR", "ރ."},
            {"MXN", "$"},
            {"MYR", "RM"},
            {"NIO", "N"},
            {"NOK", "kr"},
            {"NPR", "रु"},
            {"NZD", "$"},
            {"OMR", "ر.ع.‏ "},
            {"PAB", "B/."},
            {"PEN", "S/."},
            {"PHP", "₱"},
            {"PKR", "Rs"},
            {"PLN", "zł"},
            {"PYG", "Gs"},
            {"QAR", "ر.ق.‏ "},
            {"RON", "lei"},
            {"RSD", "Din."},
            {"RUB", "р."},
            {"RWF", "RWF"},
            {"SAR", "ر.س.‏ "},
            {"SEK", "kr"},
            {"SGD", "$"},
            {"SYP", "ل.س.‏ "},
            {"THB", "฿"},
            {"TJS", "т.р."},
            {"TMT", "m."},
            {"TND", "د.ت.‏ "},
            {"TRY", "₺"},
            {"TTD", "TT$"},
            {"TWD", "NT$"},
            {"UAH", "₴"},
            {"USD", "$"},
            {"UYU", "$U"},
            {"UZS", "so'm"},
            {"VEF", "Bs. F."},
            {"VND", "₫"},
            {"XOF", "XOF"},
            {"YER", "ر.ي.‏ "},
            {"ZAR", "R"},
            {"ZWL", "Z$"},

            {"BTC", isCreatorUpdate() ? "\u20BF" : "Ƀ"},
            {"ETH", "Ξ"},
            {"LTC", "Ł"}
        };

        static CurrencyHelper()
        {
            foreach (var keyValuePair in Currencies)
            {
                if (!CurrenciesCodes.ContainsKey(keyValuePair.Value))
                {
                    CurrenciesCodes.Add(keyValuePair.Value, keyValuePair.Key);
                }
            }
        }

        public static Dictionary<string, string> CurrenciesCodes= new Dictionary<string, string>();

        public static bool TryCodeFromSymbol(string symbol, out string code)
        {
            return CurrenciesCodes.TryGetValue(symbol, out code);
        }

        public static bool IsFiatCurrency(string code)
        {
            return UserPreferencesService.FiatCurrencies.Contains(code);
        }

        public static string FiatCurrencyName(string code)
        {
            if (CurrenciesName.TryGetValue(code, out var name))
            {
                return name;
            }

            return null;
        }

        public static bool TryGetCurrencySymbol(string code, out string symbol)
        {
            return Currencies.TryGetValue(code, out symbol);
        }

        private static bool isCreatorUpdate()
        {
            var version = GetOsVersion();
            return version.Item1 >= 10 && version.Item3 >= 15063;
        }

        private static (ulong, ulong, ulong, ulong) GetOsVersion()
        {
            var m = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var v = ulong.Parse(m);
            var v1 = (v & 0xFFFF000000000000L) >> 48;
            var v2 = (v & 0x0000FFFF00000000L) >> 32;
            var v3 = (v & 0x00000000FFFF0000L) >> 16;
            var v4 = v & 0x000000000000FFFFL;
            return (v1, v2, v3, v4);
        }
    }
}
