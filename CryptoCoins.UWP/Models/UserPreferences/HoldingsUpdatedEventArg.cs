using System.Collections.Generic;
using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class HoldingsUpdatedEventArg
    {
        public HoldingsUpdatedEventArg(UpdateAction action, HoldingsTransaction transaction, IList<HoldingsSummary> summaries)
        {
            Action = action;
            Transaction = transaction;
            Summaries = summaries;
        }

        public UpdateAction Action { get; }
        public HoldingsTransaction Transaction { get; }
        public IList<HoldingsSummary> Summaries { get; }
    }
}
