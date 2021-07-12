using System;
using Windows.UI.Xaml.Data;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Platform.Converters
{
    public class LocalizedEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue)
            {
                return enumValue.GetAttributeLocalized();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
