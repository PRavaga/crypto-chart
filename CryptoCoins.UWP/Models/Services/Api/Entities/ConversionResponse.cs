using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    public class ConversionResponse
    {
        [JsonProperty("RAW", Required = Required.Always)]
        public Dictionary<string, Dictionary<string, ConversionInfo>> Raw { get; set; }
    }
}
