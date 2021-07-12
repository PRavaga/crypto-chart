using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;

namespace CryptoCoins.UWP.Models.Services.Api
{
    public class DonationsApi : ApiServiceBase
    {
        private const string Host = "http://ravaga.com";
        private const string Endpoint = "donations.json";

        public async Task<List<CryptoWalletInfo>> GetWallets()
        {
            return await ApiExtensions.Retry<List<CryptoWalletInfo>, ApiException>(() => SendAsync<List<CryptoWalletInfo>>(HttpMethod.Get, new[] {Host, Endpoint}), 3)
                .ConfigureAwait(false);
        }
    }
}
