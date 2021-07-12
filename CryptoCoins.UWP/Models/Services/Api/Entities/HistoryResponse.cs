using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    public class HistoryResponse
    {
        [JsonProperty("Response", Required = Required.Always)]
        public string Response { get; set; }

        [JsonProperty("Type", Required = Required.Always)]
        public int Type { get; set; }

        [JsonProperty("Aggregated")]
        public bool Aggregated { get; set; }

        [JsonProperty("TimeTo", Required = Required.Always)]
        public int TimeTo { get; set; }

        [JsonProperty("TimeFrom", Required = Required.Always)]
        public int TimeFrom { get; set; }

        [JsonProperty("FirstValueInArray")]
        public bool FirstValueInArray { get; set; }

        [JsonProperty("Data", Required = Required.Always)]
        public List<HistoryItem> Data { get; set; }
    }
}
