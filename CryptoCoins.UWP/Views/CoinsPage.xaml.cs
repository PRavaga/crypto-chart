using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.ViewModels;

namespace CryptoCoins.UWP.Views
{
    public sealed partial class CoinsPage : MvvmPage
    {
        private readonly Brush _listOddBgBrush;
        private readonly Brush _transparentBrush;

        public CoinsPage()
        {
            InitializeComponent();
            _listOddBgBrush = (Brush) Application.Current.Resources["ConversionListOddBackgroundBrush"];
            _transparentBrush = new SolidColorBrush(Colors.Transparent);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }


        public CoinsViewModel ViewModel => (CoinsViewModel) DataContext;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CoinsChanged -= OnCoinsChanged;
            Bindings.StopTracking();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.CoinsChanged += OnCoinsChanged;
        }

        private void OnCoinsChanged()
        {
            RefreshItemsBackground();
        }

        private void ListViewBase_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 1 ? _listOddBgBrush : _transparentBrush;
        }

        private void RefreshItemsBackground()
        {
            var items = CoinsList.Items;

            for (var i = 0; i < items.Count; i++)
            {
                var item = (SelectorItem) CoinsList.ContainerFromItem(items[i]);
                if (item != null)
                {
                    item.Background = i % 2 == 1 ? _listOddBgBrush : _transparentBrush;
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is CryptoCurrencyInfo item)
            {
                ViewModel.NavigateToInfoPage.Execute(item);
            }
        }
    }
}
