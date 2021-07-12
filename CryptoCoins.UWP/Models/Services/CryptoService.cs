using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using CryptoCoins.UWP.Models.Services.Api;
using CryptoCoins.UWP.Models.Services.Api.Entities;
using CryptoCoins.UWP.Models.Services.Api.Entities.Request;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.ViewModels;
using CryptoCoins.UWP.Views.Formatter;
using Microsoft.Toolkit.Uwp.UI;
using ConversionInfo = CryptoCoins.UWP.Models.Services.Entries.ConversionInfo;

namespace CryptoCoins.UWP.Models.Services
{
    public class CryptoService
    {
        private readonly CryptoApi _cryptoApi;
        private readonly UserPreferencesService _preferencesService;
        private readonly StorageService _storageService;
        private CoinsResponse _coins;
        private readonly SemaphoreSlim _coinsSempaphore = new SemaphoreSlim(1);

        public CryptoService(CryptoApi cryptoApi, UserPreferencesService preferencesService, StorageService storageService)
        {
            _cryptoApi = cryptoApi;
            _preferencesService = preferencesService;
            _storageService = storageService;
        }

        public async Task EnsureCoinsLoaded()
        {
            if (_coins == null)
            {
                await _coinsSempaphore.WaitAsync();
                try
                {
                    if (_coins == null)
                    {
                        _coins = await _storageService.LoadCached<CoinsResponse>().ConfigureAwait(false);
                        if (_coins == null)
                        {
                            _coins = await ApiExtensions.Retry<CoinsResponse, ApiException>(_cryptoApi.GetCoins, 3).ConfigureAwait(false);
                            await _storageService.SaveCached(_coins, TimeSpan.FromDays(7)).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    _coinsSempaphore.Release();
                }
            }
        }

        public Uri CryptoCurrencyIcon(string currencyCode)
        {
            if (_coins != null && _coins.Data.TryGetValue(currencyCode, out var info))
            {
                return new Uri(_coins.BaseImageUrl + info.LogoUrl);
            }

            if (CurrencyHelper.IsFiatCurrency(currencyCode))
            {
                return new Uri("ms-appx:///Assets/Images/FiatTemplate.png");
            }

            return new Uri("ms-appx:///Assets/Images/CoinTemplate.png");
        }

        public string FullCryptoCurrencyName(string currencyCode)
        {
            if (_coins != null &&_coins.Data.TryGetValue(currencyCode, out var info))
            {
                return info.CoinName;
            }
            return string.Empty;
        }

        public async Task<List<CryptoCurrencyInfo>> GetCoins(bool forceUpdate)
        {
            await EnsureCoinsLoaded().ConfigureAwait(false);
            var prefs = _preferencesService.CryptoCurrenciesPreferences;
            var result = _coins.Data.Values.GroupJoin(prefs, info => info.Symbol, pref => pref.Code,
                    (info, pref) => new CryptoCurrencyInfo
                    {
                        Name = info.CoinName,
                        Code = info.Symbol,
                        Icon = _coins.BaseImageUrl + info.LogoUrl,
                        RankOrder = info.SortOrder,
                        Pref = pref.SingleOrDefault() ?? new CryptoCurrencyPreference {Code = info.Symbol, IsShown = false}
                    })
                .ToList();
            return result;
        }

        public async Task<List<ConversionInfo>> GetPreferedConversion()
        {
            await EnsureCoinsLoaded().ConfigureAwait(false);
            var cryptoPreferences = _preferencesService.CryptoCurrenciesPreferences;
            var currencyPreferences = _preferencesService.CurrencyPreferences;
            var request = new ConversionRequest
            {
                CurrenciesFrom = cryptoPreferences.Where(pref => pref.IsShown).Select(pref => pref.Code).ToList(),
                CurrenciesTo = currencyPreferences.Where(pref => pref.IsShown).Select(pref => pref.Code).ToList()
            };
            if (request.CurrenciesFrom.Count == 0 || request.CurrenciesTo.Count == 0)
            {
                return new List<ConversionInfo>();
            }
            var conversions =  (await GetConversion(request,CancellationToken.None).ConfigureAwait(false)).Where(info => info.From != info.To).ToList();

            var conversionPreferences = _preferencesService.ConversionPreferences.ToDictionary(pref => pref.From+pref.To);
            foreach (var conv in conversions)
            {
                if (conversionPreferences.TryGetValue(conv.From+conv.To, out var pref))
                {
                    conv.Pref = pref;
                }
            }
            return conversions;
        }

        public Task<DetailedConversionInfo> GetDetailedConversion(string from, string to)
        {
            return GetDetailedConversion(from, to, _preferencesService.FeaturedPreference.RangeMinutes, CancellationToken.None);
        }

        public Task<DetailedConversionInfo> GetDetailedConversion(string from, string to, CancellationToken token)
        {
            return GetDetailedConversion(from, to, _preferencesService.FeaturedPreference.RangeMinutes, CancellationToken.None);
        }

        public async Task<DetailedConversionInfo> GetDetailedConversion(string from, string to, int timeFrame, CancellationToken token)
        {
            await EnsureCoinsLoaded().ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            if (timeFrame == SettingsViewModel.RangeYear)
            {
                return await GetHistoryDaily(new HistoryRequest
                {
                    CurrencyFrom = from,
                    CurrencyTo = to,
                    Aggregate = 12,
                    Max = 29
                }, token).ConfigureAwait(false);
            }
            if (timeFrame == SettingsViewModel.Range6Months)
            {
                return await GetHistoryDaily(new HistoryRequest
                {
                    CurrencyFrom = from,
                    CurrencyTo = to,
                    Aggregate = 6,
                    Max = 29
                }, token).ConfigureAwait(false);
            }
            if (timeFrame == SettingsViewModel.Range3Months)
            {
                return await GetHistoryDaily(new HistoryRequest
                {
                    CurrencyFrom = from,
                    CurrencyTo = to,
                    Aggregate = 3,
                    Max = 29
                }, token).ConfigureAwait(false);
            }
            if (timeFrame == SettingsViewModel.RangeMonth)
            {
                return await GetHistoryDaily(new HistoryRequest
                {
                    CurrencyFrom = from,
                    CurrencyTo = to,
                    Aggregate = 1,
                    Max = 29
                }, token).ConfigureAwait(false);
            }
            if (timeFrame == SettingsViewModel.RangeWeek)
            {
                return await GetHistoryDaily(new HistoryRequest
                {
                    CurrencyFrom = from,
                    CurrencyTo = to,
                    Aggregate = 1,
                    Max = 6
                }, token).ConfigureAwait(false);
            }

            return await GetHistoryHourly(new HistoryRequest
            {
                CurrencyFrom = from,
                CurrencyTo = to,
                Aggregate = 1,
                Max = 23
            }, token).ConfigureAwait(false);
        }

        public IEnumerable<Task<DetailedConversionInfo>> GetFeaturedConversions()
        {
            var conversionPreferences = _preferencesService.ConversionPreferences;
            return conversionPreferences.Where(pref => pref.IsFeatured).OrderBy(pref => pref.FeaturedDisplayOrder).Select(pref => GetDetailedConversion(pref.From, pref.To));
        }

        public IEnumerable<Task<DetailedConversionInfo>> GetFeaturedConversions(int takeCount, CancellationToken token)
        {
            var conversionPreferences = _preferencesService.ConversionPreferences;
            return conversionPreferences.Where(pref => pref.IsFeatured).OrderBy(pref => pref.FeaturedDisplayOrder).Take(takeCount).Select(pref => GetDetailedConversion(pref.From, pref.To, token));
        }

        public async Task<List<ConversionInfo>> GetConversions(List<string> from, List<string> to, CancellationToken token)
        {
            await EnsureCoinsLoaded().ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return await GetConversion(new ConversionRequest() { CurrenciesFrom = from, CurrenciesTo = to },token);
        }

        public Task<List<ConversionInfo>> GetConversions(List<string> from, List<string> to)
        {
            return GetConversions(from, to, CancellationToken.None);
        }
        private async Task<List<ConversionInfo>> GetConversion(ConversionRequest request, CancellationToken token)
        {
            var response = await ApiExtensions.Retry<ConversionResponse, ApiException>(() => _cryptoApi.GetConversion(request, token), 3).ConfigureAwait(false);
            return response.Raw.SelectMany(fromPair => fromPair.Value.Select(
                (toPair) =>
                {
                    var info = toPair.Value;
                    // Sometimes info.From/To gives not unique values so lets use top key values
                    var fromCode = fromPair.Key;
                    var toCode = toPair.Key;
                    var r = new ConversionInfo
                    {
                        From = fromCode,
                        FromFullName = FullCryptoCurrencyName(fromCode),
                        FromIcon = CryptoCurrencyIcon(fromCode),
                        Change24 = info.ChangePct24Hour / 100,
                        ChangeValue = info.Change24Hour,
                        Rate = info.Price,
                        To = toCode,
                        Volume24 = info.Volume24Hour
                    };
                    if (fromCode == toCode)
                    {
                        r.Change24 = 0d;
                        r.ChangeValue = 0d;
                        r.Rate = 1d;
                    }
                    return r;
                })).ToList();
        }

        private async Task<DetailedConversionInfo> GetHistoryHourly(HistoryRequest request, CancellationToken token)
        {
            DetailedConversionInfo result;
            var info = await ApiExtensions.Retry<HistoryResponse, ApiException>(() => _cryptoApi.GetHistoryHourly(request, token), 3).ConfigureAwait(false);
            var changeValue = info.Data.Last().Close - info.Data.First().Open;
            var change = (changeValue) / info.Data.First().Open;
            result = new DetailedConversionInfo
            {
                From = request.CurrencyFrom,
                Change24 = double.IsInfinity(change) ? 0d : change,
                ChangeValue = changeValue,
                Rate = info.Data.Last().Close,
                Min = info.Data.Min(item => item.Low),
                Max = info.Data.Max(item => item.High),
                To = request.CurrencyTo,
                Volume24 = info.Data.Sum(item => item.VolumeFrom),
                RateHourlyHistory = info.Data.Select(item => item.Open).Concat(new[] {info.Data.Last().Close}).ToList()
            };

            result.FromIcon = CryptoCurrencyIcon(result.From);
            result.FromFullName = FullCryptoCurrencyName(result.From);

            return result;
        }

        private async Task<DetailedConversionInfo> GetHistoryDaily(HistoryRequest request, CancellationToken token)
        {
            DetailedConversionInfo result;
            var info = await ApiExtensions.Retry<HistoryResponse, ApiException>(() => _cryptoApi.GetHistoryDaily(request, token), 3).ConfigureAwait(false);
            var changeValue = info.Data.Last().Close - info.Data.First().Open;
            var change = (changeValue) / info.Data.First().Open;
            result = new DetailedConversionInfo
            {
                From = request.CurrencyFrom,
                Change24 = double.IsInfinity(change) ? 0d : change,
                ChangeValue = changeValue,
                Rate = info.Data.Last().Close,
                Min = info.Data.Min(item => item.Low),
                Max = info.Data.Max(item => item.High),
                To = request.CurrencyTo,
                Volume24 = info.Data.Sum(item => item.VolumeFrom),
                RateHourlyHistory = info.Data.Select(item => item.Open).Concat(new[] {info.Data.Last().Close}).ToList()
            };


            result.FromIcon = CryptoCurrencyIcon(result.From);
            result.FromFullName = FullCryptoCurrencyName(result.From);

            return result;
        }
    }
}
