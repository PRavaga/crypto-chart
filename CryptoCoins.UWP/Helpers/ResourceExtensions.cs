using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

using Windows.ApplicationModel.Resources;

namespace CryptoCoins.UWP.Helpers
{
    internal static class ResourceExtensions
    {
        private static ResourceLoader _resLoader = new ResourceLoader();

        public static string GetLocalized(this string resourceKey)
        {
            return _resLoader.GetString(resourceKey);
        }

        public static string GetAttributeLocalized(this Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString());
            var description = enumMember[0].GetCustomAttribute<DescriptionAttribute>();
            return description.Description.GetLocalized();
        }
    }
}
