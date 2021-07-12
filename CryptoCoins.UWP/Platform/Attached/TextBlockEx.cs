using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CryptoCoins.UWP.Platform.Attached
{
    public class TextBlockEx
    {
        private static long? _token;

        public static readonly DependencyProperty CapitalizeProperty = DependencyProperty.RegisterAttached(
            "Capitalize", typeof(bool), typeof(TextBlockEx), new PropertyMetadata(default(bool), TextBoxRegexPropertyOnChange));

        private static void TextBoxRegexPropertyOnChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = (TextBlock) sender;
            if (_token.HasValue)
            {
                textBlock.UnregisterPropertyChangedCallback(TextBlock.TextProperty, _token.Value);
            }

            _token = textBlock.RegisterPropertyChangedCallback(TextBlock.TextProperty, Callback);
        }

        private static void Callback(DependencyObject sender, DependencyProperty dp)
        {
            var textBlock = (TextBlock) sender;

            if (GetCapitalize(sender))
            {
                textBlock.Text = textBlock.Text.ToUpper();
            }
        }

        public static void SetCapitalize(DependencyObject element, bool value)
        {
            element.SetValue(CapitalizeProperty, value);
        }

        public static bool GetCapitalize(DependencyObject element)
        {
            return (bool) element.GetValue(CapitalizeProperty);
        }
    }
}
