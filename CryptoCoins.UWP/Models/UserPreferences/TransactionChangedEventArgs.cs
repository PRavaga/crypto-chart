using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class TransactionChangedEventArgs
    {
        public TransactionChangedEventArgs(HoldingsTransaction transaction)
        {
            Transaction = transaction;
        }

        public HoldingsTransaction Transaction { get; }
    }
}
