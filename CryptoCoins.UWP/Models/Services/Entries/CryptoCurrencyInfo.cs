using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.UserPreferences;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class CryptoCurrencyInfo : Observable
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Code { get; set; }
        public int RankOrder { get; set; }

        public CryptoCurrencyPreference Pref { get; set; }
    }
}
