using CryptoCoins.UWP.Helpers.QueryString;

namespace CryptoCoins.UWP.Models.Services.Api.Entities.Request
{
    public class HistoryRequest
    {
        [QueryParameter("fsym")]
        public string CurrencyFrom { get; set; }
        [QueryParameter("tsym")]
        public string CurrencyTo { get; set; }
        [QueryParameter("aggregate")]
        public int Aggregate { get; set; }
        [QueryParameter("limit")]
        public int Max { get; set; }
    }
}
