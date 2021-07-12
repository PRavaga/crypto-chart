using CryptoCoins.UWP.Helpers;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class CurrencyPreference : Observable
    {
        private bool _isShown;
        [PrimaryKey]
        public string Code { get; set; }

        public bool IsShown
        {
            get => _isShown;
            set => Set(ref _isShown, value);
        }
    }
}
