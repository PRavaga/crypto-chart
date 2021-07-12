using System.Collections.Generic;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Models.Services.Entries.Compare
{
    public sealed class CurrencyPairEqualityUpdater : IEqualityUpdater<ConversionInfo>, IEqualityUpdater<object>, IEqualityUpdater<DetailedConversionInfo>
    {
        private CurrencyPairEqualityUpdater()
        {
        }

        public static CurrencyPairEqualityUpdater Instance { get; } = new CurrencyPairEqualityUpdater();

        public bool Equals(ConversionInfo x, ConversionInfo y)
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
            if (x.GetType() != y.GetType())
            {
                return false;
            }
            return string.Equals(x.From, y.From) && string.Equals(x.To, y.To);
        }

        public int GetHashCode(ConversionInfo obj)
        {
            unchecked
            {
                return ((obj.From != null ? obj.From.GetHashCode() : 0) * 397) ^ (obj.To != null ? obj.To.GetHashCode() : 0);
            }
        }

        public void Update(ConversionInfo target, ConversionInfo source)
        {
            target.Change24 = source.Change24;
            target.ChangeValue = source.ChangeValue;
            target.From = source.From;
            target.Rate = source.Rate;
            target.To = source.To;
            target.Volume24 = source.Volume24;
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return Equals((ConversionInfo) x, (ConversionInfo) y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return GetHashCode((ConversionInfo) obj);
        }

        void IEqualityUpdater<object>.Update(object target, object source)
        {
            Update((ConversionInfo) target, (ConversionInfo) source);
        }

        public bool Equals(DetailedConversionInfo x, DetailedConversionInfo y)
        {
            return Equals((ConversionInfo) x, y);
        }

        public int GetHashCode(DetailedConversionInfo obj)
        {
            return GetHashCode((ConversionInfo) obj);
        }

        public void Update(DetailedConversionInfo target, DetailedConversionInfo source)
        {
            Update((ConversionInfo)target, source);
            target.RateHourlyHistory = source.RateHourlyHistory;
        }
    }
}
