using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api.Entities
{
    internal class ErrorResponse
    {
        [JsonProperty("Response")]
        public string Response { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Type")]
        public int Type { get; set; }
    }
}
