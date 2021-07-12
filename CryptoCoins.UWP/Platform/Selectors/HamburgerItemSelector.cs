using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.ViewModels;

namespace CryptoCoins.UWP.Platform.Selectors
{
    public class HamburgerItemSelector : DataTemplateSelector
    {
        public DataTemplate SymbolItem { get; set; }
        public DataTemplate IconItem { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ShellNavigationItem shellItem)
            {
                return shellItem.IconUri != null ? IconItem : SymbolItem;
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
