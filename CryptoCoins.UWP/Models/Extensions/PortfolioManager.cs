using System.Collections.Generic;
using CryptoCoins.UWP.Models.Exceptions.Validation;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Platform;

namespace CryptoCoins.UWP.Models.Extensions
{
    public class PortfolioManager
    {
        private void ApplyChange(HoldingsSummary summary, decimal amountChange)
        {
            var resultAmount = summary.Amount + amountChange;

            if (resultAmount < decimal.Zero && !CurrencyHelper.IsFiatCurrency(summary.Currency))
            {
                throw new InsufficientHoldingsException();
            }

            summary.Amount = resultAmount;
        }

        public void ApplyChanges(IList<HoldingsSummary> summaries, Dictionary<string, HoldingsChanging> changes)
        {
            foreach (var summary in summaries)
            {
                if (changes.TryGetValue(summary.Currency, out var change))
                {
                    ApplyChange(summary, change.AmountChange);
                }
            }
        }

        private void AddChange(Dictionary<string, HoldingsChanging> changes, string code, decimal amountChange)
        {
            if (amountChange != decimal.Zero)
            {
                if (!changes.TryGetValue(code, out var change))
                {
                    change = new HoldingsChanging {Currency = code, AmountChange = decimal.Zero};
                    changes.Add(code, change);
                }

                change.AmountChange += amountChange;
            }
        }

        public Dictionary<string, HoldingsChanging> GetApplyChanges(IEnumerable<HoldingsTransaction> transactions, Dictionary<string, HoldingsChanging> result = null)
        {
            result = result ?? new Dictionary<string, HoldingsChanging>();

            foreach (var transaction in transactions)
            {
                GetApplyChanges(transaction, result);
            }

            return result;
        }

        public Dictionary<string, HoldingsChanging> GetApplyChanges(HoldingsTransaction transaction, Dictionary<string, HoldingsChanging> result = null)
        {
            result = result ?? new Dictionary<string, HoldingsChanging>();

            AddChange(result, transaction.FromCode, -transaction.FromAmount);
            AddChange(result, transaction.ToCode, transaction.ToAmount);

            return result;
        }

        public Dictionary<string, HoldingsChanging> GetUndoChanges(HoldingsTransaction transaction, Dictionary<string, HoldingsChanging> result = null)
        {
            result = result ?? new Dictionary<string, HoldingsChanging>();

            AddChange(result, transaction.FromCode, transaction.FromAmount);
            AddChange(result, transaction.ToCode, -transaction.ToAmount);

            return result;
        }

        public Dictionary<string, HoldingsChanging> GetUndoChanges(IEnumerable<HoldingsTransaction> transactions, Dictionary<string, HoldingsChanging> result = null)
        {
            result = result ?? new Dictionary<string, HoldingsChanging>();

            foreach (var transaction in transactions)
            {
                GetUndoChanges(transaction, result);
            }

            return result;
        }
    }
}
