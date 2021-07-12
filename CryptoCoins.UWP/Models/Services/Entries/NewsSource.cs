using CryptoCoins.UWP.Helpers;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class NewsSource : Observable
    {
        private bool _isEnabled;

        public NewsSource(string source, string url)
        {
            Source = source;
            Url = url;
        }

        public string Source { get; set; }

        [JsonIgnore]
        public string Url { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => Set(ref _isEnabled, value);
        }
    }
}
