using System;
using Windows.UI.Xaml.Data;

namespace CryptoCoins.UWP.Platform.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, (string) value);
        }
    }
}
