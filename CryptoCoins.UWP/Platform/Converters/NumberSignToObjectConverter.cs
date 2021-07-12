using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CryptoCoins.UWP.Platform.Converters
{
    public class NumberSignToObjectConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty PositiveProperty = DependencyProperty.Register(
            nameof(Positive), typeof(object), typeof(NumberSignToObjectConverter), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty NegativeProperty = DependencyProperty.Register(
            nameof(Negative), typeof(object), typeof(NumberSignToObjectConverter), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ZeroProperty = DependencyProperty.Register(
            nameof(Zero), typeof(object), typeof(NumberSignToObjectConverter), new PropertyMetadata(default(object)));

        public object Positive
        {
            get => GetValue(PositiveProperty);
            set => SetValue(PositiveProperty, value);
        }

        public object Negative
        {
            get => GetValue(NegativeProperty);
            set => SetValue(NegativeProperty, value);
        }

        public object Zero
        {
            get => GetValue(ZeroProperty);
            set => SetValue(ZeroProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return number > 0 ? Positive : (number < 0 ? Negative : Zero);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
