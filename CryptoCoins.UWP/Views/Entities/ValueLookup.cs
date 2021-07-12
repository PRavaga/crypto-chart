using CryptoCoins.UWP.ViewModels.Entities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Entities
{
    public class ValueLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            var info = ((HoldingsSummary) instance);
            return info.Value;
        }
    }
}
