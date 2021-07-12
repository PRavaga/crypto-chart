using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    public class HistoryItem
    {
        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("volumefrom")]
        public double VolumeFrom { get; set; }

        [JsonProperty("volumeto")]
        public double VolumeTo { get; set; }
    }
}
