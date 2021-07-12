using System.Collections;
using System.Collections.Generic;
using CryptoCoins.UWP.Models.UserPreferences;

namespace CryptoCoins.UWP.Models.Services.Entries.Compare
{
    internal class ConversionDirectionEqualityComparer : IEqualityComparer, IEqualityComparer<object>, IEqualityComparer<ConversionPreference>, IEqualityComparer<ConversionInfo>
    {
        private ConversionDirectionEqualityComparer()
        {
        }

        public static ConversionDirectionEqualityComparer Instance { get; } = new ConversionDirectionEqualityComparer();

        public bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (ReferenceEquals(x, null))
            {
                return false;
            }
            if (ReferenceEquals(y, null))
            {
                return false;
            }
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(object obj)
        {
            if (obj is ConversionPreference pref)
            {
                return GetHashCode(pref);
            }
            if (obj is ConversionInfo info)
            {
                return GetHashCode(info);
            }
            return obj.GetHashCode();
        }

        public bool Equals(ConversionInfo x, ConversionInfo y)
        {
            return Equals(x, (object) y);
        }

        public int GetHashCode(ConversionInfo obj)
        {
            unchecked
            {
                return ((obj.From != null ? obj.From.GetHashCode() : 0) * 397) ^ (obj.To != null ? obj.To.GetHashCode() : 0);
            }
        }

        public bool Equals(ConversionPreference x, ConversionPreference y)
        {
            return Equals(x, (object) y);
        }

        public int GetHashCode(ConversionPreference obj)
        {
            unchecked
            {
                return ((obj.From != null ? obj.From.GetHashCode() : 0) * 397) ^ (obj.To != null ? obj.To.GetHashCode() : 0);
            }
        }
    }
}
