using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    public class CoinsResponse
    {
        [JsonProperty("Response")]
        public string Response { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("BaseImageUrl")]
        public string BaseImageUrl { get; set; }

        [JsonProperty("BaseLinkUrl")]
        public string BaseLinkUrl { get; set; }

        [JsonProperty("Type")]
        public int Type { get; set; }

        [JsonProperty("Data", Required = Required.Always)]
        public Dictionary<string, CoinInfo> Data { get; set; }
    }
}
