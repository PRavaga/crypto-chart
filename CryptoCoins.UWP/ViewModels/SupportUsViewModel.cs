using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.ViewModels.Common;

namespace CryptoCoins.UWP.ViewModels
{
    public class SupportUsViewModel : ViewModelBase
    {
        private RelayCommand _copyWalletCommand;
        private CryptoWalletInfo _selectedWallet;
        private DonationsApi _donationsApi;
        private List<CryptoWalletInfo> _wallets;
        private static readonly List<CryptoWalletInfo> DefaultWallets = new List<CryptoWalletInfo>
        {
            new CryptoWalletInfo
            {
                CurrencyCode = "BTC",
                CurrencyName = "Bitcoin",
                WalletNumber = "13HftV6rgWnBTKFPxPJTGQiUzTyeiazPJ2"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "BCH",
                CurrencyName = "Bitcoin Cash",
                WalletNumber = "1C8aBB7jC5JAM8Y2P54QHNiSsPeFEsJqnY"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "ETH",
                CurrencyName = "Ethereum",
                WalletNumber = "0xa423F4369a4d09E2a96fef3d63DF3059Dd6a0E8f"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "LTC",
                CurrencyName = "Litecoin",
                WalletNumber = "LRcNXNHRFAFxSqaKhu19yFm8pbsKybKpVV"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "ETC",
                CurrencyName = "Ethereum Classic",
                WalletNumber = "0x66521ECeB892957A5ce5499B11129EF5E2B59E36"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "IOT",
                CurrencyName = "IOTA",
                WalletNumber = "H9GGPMKHLVO9FLNDEUA9CRPERLSEIYKVKUMHCPGKFSOHEFENAQQMIAOVWYLKUDQQFCFZX9OAZVDDOOVIDJBCLKFMYC"
            },
            new CryptoWalletInfo
            {
                CurrencyCode = "DASH",
                CurrencyName = "DigitalCash",
                WalletNumber = "XkAWaAXTTJeL89fPCP1VRoytjV3MkAePqp"
            },

        };

        private ProgressState _progress = new ProgressState();

        public ProgressState Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        public SupportUsViewModel(DonationsApi donationsApi)
        {
            _donationsApi = donationsApi;
        }

        public List<CryptoWalletInfo> Wallets
        {
            get => _wallets;
            set => Set(ref _wallets, value);
        }

        public CryptoWalletInfo SelectedWallet
        {
            get => _selectedWallet;
            set => Set(ref _selectedWallet, value);
        }

        public RelayCommand CopyWalletCommand => _copyWalletCommand ?? (_copyWalletCommand = new RelayCommand(() =>
        {
            var copyPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            copyPackage.SetText(SelectedWallet.WalletNumber);

            Clipboard.SetContent(copyPackage);
        }));

        public override async void OnNavigatedTo(object parameter)
        {
            await LoadWallets();
        }

        public async Task LoadWallets()
        {
            try
            {
                using (Progress.BeginOperation())
                {
                    var wallets = await _donationsApi.GetWallets();
                    Wallets = wallets;
                }
            }
            catch (ApiException e)
            {
                Logger.Warn("Failed to load donation wallets", e);
                Wallets = DefaultWallets;
            }

            SelectedWallet = Wallets.FirstOrDefault();
        }
    }
}
