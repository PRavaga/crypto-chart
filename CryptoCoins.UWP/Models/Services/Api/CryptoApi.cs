using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using CryptoCoins.UWP.Helpers.QueryString;
using CryptoCoins.UWP.Models.Services.Api.Entities;
using CryptoCoins.UWP.Models.Services.Api.Entities.Request;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;

namespace CryptoCoins.UWP.Models.Services.Api
{
    public class CryptoApi : ApiServiceBase
    {
        public const string ApiBaseUrl = "https://min-api.cryptocompare.com/data";
        public const string MultiPriceFull = "pricemultifull";
        public const string HistoryHourly = "histohour";
        public const string HistoryDayly = "histoday";

        public Task<CoinsResponse> GetCoins()
        {
            return GetCoins(CancellationToken.None);
        }

        public Task<CoinsResponse> GetCoins(CancellationToken token)
        {
            return SendAsync<CoinsResponse>(HttpMethod.Get, new[] {"https://min-api.cryptocompare.com/data/all/coinlist"}, token);
        }

        public Task<ConversionResponse> GetConversion(ConversionRequest request)
        {
            return GetConversion(request, CancellationToken.None);
        }

        public Task<ConversionResponse> GetConversion(ConversionRequest request, CancellationToken token)
        {
            return SendAsync<ConversionResponse>(HttpMethod.Get, new[] {ApiBaseUrl, MultiPriceFull}, token, QueryHelper.QueryString(request));
        }

        public Task<HistoryResponse> GetHistoryHourly(HistoryRequest request)
        {
            return GetHistoryHourly(request, CancellationToken.None);
        }

        public Task<HistoryResponse> GetHistoryHourly(HistoryRequest request, CancellationToken token)
        {
            return ApiExtensions.HandleServerError(
                () => SendAsync<HistoryResponse>(HttpMethod.Get, new[] {ApiBaseUrl, HistoryHourly}, token, QueryHelper.QueryString(request) + "&e=CCCAGG"),
                (ErrorResponse e) =>
                {
                    if (e.Type == 1)
                    {
                        throw new NoDataForCurrencyException(200, e.Message);
                    }

                    return null;
                });
        }

        public Task<HistoryResponse> GetHistoryDaily(HistoryRequest request)
        {
            return GetHistoryDaily(request, CancellationToken.None);
        }

        public Task<HistoryResponse> GetHistoryDaily(HistoryRequest request, CancellationToken token)
        {
            return ApiExtensions.HandleServerError(
                () => SendAsync<HistoryResponse>(HttpMethod.Get, new[] {ApiBaseUrl, HistoryDayly}, token, QueryHelper.QueryString(request) + "&e=CCCAGG"),
                (ErrorResponse e) =>
                {
                    if (e.Type == 1)
                    {
                        throw new NoDataForCurrencyException(200, e.Message);
                    }

                    return null;
                });
        }
    }
}
