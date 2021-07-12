namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class CryptoWalletInfo
    {
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string WalletNumber { get; set; }

        public string FormatFullName(string currencyName, string currencyCode)
        {
            return $"{currencyName} ({currencyCode})";
        }
    }
}
