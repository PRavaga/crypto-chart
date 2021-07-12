using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.ViewModels.Entities;

namespace CryptoCoins.UWP.Models.Services.Entries.Compare
{
    public class HoldingsSummarieEqualityComparer:IEqualityComparer<HoldingsSummary>
    {
        public bool Equals(HoldingsSummary x, HoldingsSummary y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(HoldingsSummary obj)
        {
            return obj.CurrencyCode.GetHashCode();
        }
    }
}
