using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CryptoCoins.UWP.Platform.Selectors
{
    public class ItemsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AllItems { get; set; }
        public DataTemplate LastItems { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            if (itemsControl.IndexFromContainer(container) == ((IList) itemsControl.ItemsSource).Count - 1)
            {
                return LastItems;
            }
            return AllItems;
        }
    }
}
