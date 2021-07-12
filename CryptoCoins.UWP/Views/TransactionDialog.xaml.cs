using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Platform.Dialogs;
using CryptoCoins.UWP.ViewModels;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    public sealed partial class TransactionDialog : MvvmContentDialog
    {
        public TransactionDialog()
        {
            InitializeComponent();
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                Resources["ContentDialogPadding"] = new Thickness(12, 12, 12, 12);
            }

            TitleSetter.Value = "TransactionDialog_CreateTitle".GetLocalized();
            EditingTitleSetter.Value = "TransactionDialog_EditTitle".GetLocalized();
        }

        public TransactionViewModel ViewModel => (TransactionViewModel) DataContext;

        private void OnCoinInputChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.FilterSuggestions.Execute(sender.Text);
        }

        private void CoinInput_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.BaseCurrency = (CryptoCurrencyInfo) args.SelectedItem;
        }

        private void CounterCurrencyInput_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.CounterCurrency = (CryptoCurrencyInfo) args.SelectedItem;
        }
    }
}
