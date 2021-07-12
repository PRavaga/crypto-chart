using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Platform.Dialogs;
using CryptoCoins.UWP.ViewModels;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    public sealed partial class AlertDialog : MvvmContentDialog
    {
        public AlertDialog()
        {
            InitializeComponent();
            TitleSetter.Value = "AlertDialog_CreateTitle".GetLocalized();
            EditingTitleSetter.Value = "AlertDialog_EditTitle".GetLocalized();
            ViewModel.PropertyChanged += OnPropertyChanged;
        }

        public AlertDialogViewModel ViewModel => (AlertDialogViewModel) DataContext;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (ViewModel.Frequency)
            {
                case AlertFrequency.OneTime:
                    FrequencyOneTime.IsChecked = true;
                    break;
                case AlertFrequency.EveryTime:
                    FrequencyEveryTime.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnCoinInputChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.FilterSuggestions.Execute(sender.Text);
        }

        private void CoinInput_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.SelectedCoin = (CryptoCurrencyInfo) args.SelectedItem;
        }
    }
}
