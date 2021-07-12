using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.ViewModels;

namespace CryptoCoins.UWP.Styles
{
    public sealed partial class Styles
    {
        public Styles()
        {
            InitializeComponent();
        }

        private void OnPrimaryListChaning(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var listViewItem = args.ItemContainer;

            if (listViewItem != null)
            {
                var model = (ShellNavigationItem) args.Item;

                listViewItem.IsEnabled = model.IsEnabled;
            }
        }
    }
}
