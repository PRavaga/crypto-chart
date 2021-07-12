using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.Views;

namespace CryptoCoins.UWP.ViewModels
{
    public class CoinsViewModel : ViewModelBase
    {
        public enum StatusFilter
        {
            Enabled,
            Disabled,
            All
        }

        private readonly CryptoService _cryptoService;
        private readonly UserPreferencesService _preferencesService;
        private readonly NavigationService _navigationService;
        private FilterCollection<CryptoCurrencyInfo> _coins;
        private string _filter;
        private DescriptionWrapper<StatusFilter> _showFilter;

        public CoinsViewModel(CryptoService cryptoService, UserPreferencesService preferencesService, NavigationService navigationService)
        {
            _cryptoService = cryptoService;
            _preferencesService = preferencesService;
            _navigationService = navigationService;
            _showFilter = ShowFilters.First();
            PropertyChanged += OnPropertyChanged;
        }

        public string Filter
        {
            get => _filter;
            set => Set(ref _filter, value);
        }

        public DescriptionWrapper<StatusFilter> ShowFilter
        {
            get => _showFilter;
            set => Set(ref _showFilter, value);
        }

        public FilterCollection<CryptoCurrencyInfo> Coins
        {
            get => _coins;
            set => Set(ref _coins, value);
        }

        public List<DescriptionWrapper<StatusFilter>> ShowFilters { get; } = new List<DescriptionWrapper<StatusFilter>>()
        {
            new DescriptionWrapper<StatusFilter>()
            {
                Value = StatusFilter.All,
                Description = "CoinsPage_FilterAll".GetLocalized()
            },
            new DescriptionWrapper<StatusFilter>()
            {
                Value = StatusFilter.Enabled,
                Description = "CoinsPage_FilterEnabled".GetLocalized()
            },
            new DescriptionWrapper<StatusFilter>()
            {
                Value = StatusFilter.Disabled,
                Description = "CoinsPage_FilterDisabled".GetLocalized()
            }
        };

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Filter))
            {
                RefreshFilter();
            }
            else if (e.PropertyName == nameof(ShowFilter))
            {
                RefreshFilter();
            }
        }

        public override async void OnNavigatedTo(object parameter)
        {
            SubscribeToEvents();
            await RefreshCoins();
        }

        private void SubscribeToEvents()
        {
            _preferencesService.PreferencesUpdated += OnPrefUpdated;
        }

        private async void OnPrefUpdated(PreferencesUpdatedEventArg e)
        {
            if (e.Action == UpdateAction.Reset)
            {
                await RefreshCoins();
            }
        }

        private void UnsubscribeFromEvents()
        {
            _preferencesService.PreferencesUpdated -= OnPrefUpdated;
        }

        private async Task RefreshCoins()
        {
            try
            {
                var coins = await _cryptoService.GetCoins(false);
                Coins = new FilterCollection<CryptoCurrencyInfo>(coins, new LambdaComparer<CryptoCurrencyInfo>((x, y) => x.RankOrder.CompareTo(y.RankOrder)));
                Coins.FilterFunc = info => (ShowFilter.Value == StatusFilter.All ||
                                            ShowFilter.Value == StatusFilter.Enabled && info.Pref.IsShown ||
                                            ShowFilter.Value == StatusFilter.Disabled && !info.Pref.IsShown) &&
                                           (string.IsNullOrEmpty(Filter) ||
                                            info.Name.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1 ||
                                            info.Code.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) != -1);
                foreach (var coin in Coins.SourceList)
                {
                    coin.Pref.PropertyChanged += OnCoinChanged;
                }
                RefreshFilter();
            }
            catch (ApiException e)
            {
                Logger.Error("Failed to load coins", e);
            }
        }

        private void OnCoinChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CryptoCurrencyPreference.IsShown))
            {
                OnCoinToggled((CryptoCurrencyPreference) sender);
            }
        }

        public override void OnNavigatedFrom(object parameter)
        {
            UnsubscribeFromEvents();
            if (Coins != null)
            {
                foreach (var coin in Coins.SourceList)
                {
                    coin.PropertyChanged -= OnCoinChanged;
                }
            }
        }

        private RelayCommand<CryptoCurrencyInfo> _navigateToInfoPage;

        public RelayCommand<CryptoCurrencyInfo> NavigateToInfoPage => _navigateToInfoPage ?? (_navigateToInfoPage = new RelayCommand<CryptoCurrencyInfo>((e) =>
                                                                          {
                                                                              _navigationService.Navigate<CoinInfoWebPage>(
                                                                                  new CoinInfoWebViewModel.NavigationParameter() {Currency = e});
                                                                          }));

        public async void OnCoinToggled(CryptoCurrencyPreference info)
        {
            await _preferencesService.ModifyCryptoPreference(info).ConfigureAwait(false);
        }

        private void RefreshFilter()
        {
            Coins.Filter();
            CoinsChanged?.Invoke();
        }

        public static T[] GetEnumValues<T>()
        {
            return (T[]) Enum.GetValues(typeof(T));
        }

        public event Action CoinsChanged;
    }
}
