using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries.Сoinmarketcap;

namespace CryptoCoins.UWP.Models.Services.Api
{
    public class CoinmarketcapApi : ApiServiceBase
    {
        public const string BaseUrl = "https://api.coinmarketcap.com/v1";
        public const string TickerUrl = "ticker/?limit=0";

        public async Task<List<CryptoCurrencyInfo>> Currencies()
        {
            return await ApiExtensions.Retry<List<CryptoCurrencyInfo>, ApiException>(() => SendAsync<List<CryptoCurrencyInfo>>(HttpMethod.Get, new[] {BaseUrl, TickerUrl}), 3)
                .ConfigureAwait(false);
        }
    }
}
