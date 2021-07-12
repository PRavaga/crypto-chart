using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class SummaryChangedEventArgs
    {
        public SummaryChangedEventArgs(HoldingsSummary[] summaries)
        {
            Summaries = summaries;
        }

        public HoldingsSummary[] Summaries { get; }
    }
}
