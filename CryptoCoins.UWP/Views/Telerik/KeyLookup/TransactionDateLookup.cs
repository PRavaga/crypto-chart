using CryptoCoins.UWP.ViewModels.Entities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Telerik.KeyLookup
{
    public class TransactionDateLookup : IKeyLookup
    {
        public object GetKey(object instance)
        {
            var transaction = (HoldingsTransaction) instance;
            return transaction.Date;
        }
    }
}
