using CryptoCoins.UWP.Helpers;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class ConversionPreference : Observable
    {
        private bool _isFeatured;

        [PrimaryKey]
        [AutoIncrement]
        public int? Id { get; set; }

        [Indexed]
        public string From { get; set; }

        [Indexed]
        public string To { get; set; }

        public int DisplayOrder { get; set; }
        public int FeaturedDisplayOrder { get; set; }

        public bool IsFeatured
        {
            get => _isFeatured;
            set => Set(ref _isFeatured, value);
        }
    }
}
