using CryptoCoins.UWP.ViewModels.Entities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Entities
{
    public class QuantityLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return ((HoldingsSummary) instance).Amount;
        }
    }
}
