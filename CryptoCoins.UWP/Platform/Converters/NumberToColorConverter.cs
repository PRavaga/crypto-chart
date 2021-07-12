using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CryptoCoins.UWP.Platform.Converters
{
    public class NumberToColorConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty PositiveColorProperty = DependencyProperty.Register(
            nameof(PositiveColor), typeof(Color), typeof(NumberToColorConverter), new PropertyMetadata(default(Color)));

        public static readonly DependencyProperty NegativeColorProperty = DependencyProperty.Register(
            nameof(NegativeColor), typeof(Color), typeof(NumberToColorConverter), new PropertyMetadata(default(Color)));

        public Color PositiveColor
        {
            get => (Color) GetValue(PositiveColorProperty);
            set => SetValue(PositiveColorProperty, value);
        }

        public Color NegativeColor
        {
            get => (Color) GetValue(NegativeColorProperty);
            set => SetValue(NegativeColorProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double number)
            {
                return number > 0 ? PositiveColor : NegativeColor;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
