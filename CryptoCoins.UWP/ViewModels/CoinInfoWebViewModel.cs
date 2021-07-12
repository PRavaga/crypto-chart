using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.ViewModels.Common;
using MetroLog;

namespace CryptoCoins.UWP.ViewModels
{
    public class CoinInfoWebViewModel : ViewModelBase
    {
        private const string CoinInfoUri = "https://coinmarketcap.com/currencies/";
        private const string CoinmarketcapCoinsCache = "CoinmarketCapCoins";

        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<CoinInfoWebViewModel>();
        private readonly NavigationService _navigationService;
        private readonly CoinmarketcapApi _coinmarketcapApi;
        private readonly StorageService _storageService;
        private DataState _dataState;
        private IDisposable _loadingOperation;
        private ProgressState _progress = new ProgressState();

        public CoinInfoWebViewModel(NavigationService navigationService, CoinmarketcapApi coinmarketcapApi, StorageService storageService)
        {
            _navigationService = navigationService;
            _coinmarketcapApi = coinmarketcapApi;
            _storageService = storageService;
        }

        public ProgressState Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        public DataState DataState
        {
            get => _dataState;
            set => Set(ref _dataState, value);
        }

        public WebView WebView { get; private set; }
        public Uri Uri { get; set; }

        public void Initialize(WebView webView)
        {
            WebView = webView;
            WebView.DOMContentLoaded += OnContentLoaded;
            WebView.NavigationFailed += WebViewOnNavigationFailed;
            WebView.NavigationStarting += WebViewOnNavigationStarting;
        }

        private void OnContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            DataState = DataState.Available;
            _loadingOperation?.Dispose();
        }

        public override async void OnNavigatedTo(object parameter)
        {
            if (parameter is NavigationParameter navParameter && navParameter.Currency!= null)
            {
                await LoadInfo(navParameter.Currency);
            }
            else
            {
                _navigationService.GoBack();
            }
        }

        private string GetCoinId(List<Models.Services.Entries.Сoinmarketcap.CryptoCurrencyInfo> coins, CryptoCurrencyInfo info)
        {
            var match = coins.FirstOrDefault(i => string.Equals(i.Name, info.Name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                match = coins.FirstOrDefault(i => string.Equals(i.Symbol, info.Code, StringComparison.OrdinalIgnoreCase));
            }

            if (match == null)
            {
                return info.Name;
            }

            return match.Id;
        }

        private async Task LoadInfo(CryptoCurrencyInfo info)
        {
            try
            {
                var coinmarketcapInfos = await _storageService.LoadCached<List<Models.Services.Entries.Сoinmarketcap.CryptoCurrencyInfo>>(CoinmarketcapCoinsCache);
                if (coinmarketcapInfos == null)
                {
                    coinmarketcapInfos = await _coinmarketcapApi.Currencies();
                    await _storageService.SaveCached(coinmarketcapInfos, TimeSpan.FromDays(7), CoinmarketcapCoinsCache);
                }

                var id = GetCoinId(coinmarketcapInfos, info);
                if (id != null && Uri.TryCreate(CoinInfoUri + id, UriKind.Absolute, out var uri))
                {
                    Log.Info($"Loading {uri}");
                    WebView.Navigate(uri);
                }
                else
                {
                    Log.Warn($"Can't open coin page {info.Code}");
                    _navigationService.GoBack();
                }
            }
            catch (ApiException)
            {
                DataState = DataState.Unavailable;
            }
        }

        private void WebViewOnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            DataState = DataState.NotReady;
            _loadingOperation = Progress.BeginOperation();
        }

        private void WebViewOnNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            _loadingOperation?.Dispose();
            if (e.WebErrorStatus == WebErrorStatus.NotFound)
            {
                DataState = DataState.Available;
            }
            else
            {
                DataState = DataState.Unavailable;
            }
        }

        public class NavigationParameter
        {
            public CryptoCurrencyInfo Currency { get; set; }
            public string Title { get; set; }
        }

        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new RelayCommand(() =>
        {
            WebView.Refresh();
        }));
    }
}
