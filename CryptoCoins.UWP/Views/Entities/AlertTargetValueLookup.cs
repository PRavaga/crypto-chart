using CryptoCoins.UWP.Models.StorageEntities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Entities
{
    public class AlertTargetValueLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            return ((AlertModel) instance).TargetValue;
        }
    }
}
