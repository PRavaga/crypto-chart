using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CryptoCoins.UWP.Views.Controls
{
    public class ListSelectItem : ListBoxItem
    {
        public static readonly DependencyProperty SeparatorVisibilityProperty = DependencyProperty.Register(
            nameof(SeparatorVisibility), typeof(Visibility), typeof(ListSelectItem), new PropertyMetadata(Visibility.Visible));

        public Visibility SeparatorVisibility
        {
            get => (Visibility) GetValue(SeparatorVisibilityProperty);
            set => SetValue(SeparatorVisibilityProperty, value);
        }
    }
}
