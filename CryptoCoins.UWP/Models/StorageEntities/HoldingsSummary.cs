using SQLite;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    public class HoldingsSummary
    {
        public HoldingsSummary(string currency, decimal amount)
        {
            Currency = currency;
            Amount = amount;
        }

        public HoldingsSummary()
        {
        }

        [PrimaryKey]
        public string Currency { get; set; }

        public decimal Amount { get; set; }
    }
}
