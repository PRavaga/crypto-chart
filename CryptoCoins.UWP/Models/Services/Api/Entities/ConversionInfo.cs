using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    public class ConversionInfo
    {
        [JsonProperty("TYPE")]
        public string Type { get; set; }

        [JsonProperty("MARKET")]
        public string Market { get; set; }

        [JsonProperty("FROMSYMBOL")]
        public string FromSymbol { get; set; }

        [JsonProperty("TOSYMBOL")]
        public string ToSymbol { get; set; }

        [JsonProperty("FLAGS")]
        public string Flags { get; set; }

        [JsonProperty("PRICE")]
        public double Price { get; set; }

        [JsonProperty("LASTUPDATE")]
        public int LastUpdate { get; set; }

        [JsonProperty("LASTVOLUME")]
        public double LastVolume { get; set; }

        [JsonProperty("LASTVOLUMETO")]
        public double LastVolumeTo { get; set; }

        [JsonProperty("LASTTRADEID")]
        public string LastTradeId { get; set; }

        [JsonProperty("VOLUME24HOUR")]
        public double Volume24Hour { get; set; }

        [JsonProperty("VOLUME24HOURTO")]
        public double Volume24HourTo { get; set; }

        [JsonProperty("OPEN24HOUR")]
        public double Open24Hour { get; set; }

        [JsonProperty("HIGH24HOUR")]
        public double High24Hour { get; set; }

        [JsonProperty("LOW24HOUR")]
        public double Low24Hour { get; set; }

        [JsonProperty("LASTMARKET")]
        public string Lastmarket { get; set; }

        [JsonProperty("CHANGE24HOUR")]
        public double Change24Hour { get; set; }

        [JsonProperty("CHANGEPCT24HOUR")]
        public double ChangePct24Hour { get; set; }

        [JsonProperty("SUPPLY")]
        public double Supply { get; set; }

        [JsonProperty("MKTCAP")]
        public double Mktcap { get; set; }
    }
}
