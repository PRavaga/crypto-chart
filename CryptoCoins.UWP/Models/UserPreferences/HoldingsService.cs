using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Exceptions.Validation;
using CryptoCoins.UWP.Models.Extensions;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Platform;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class HoldingsService
    {
        private readonly HoldingsStorage _holdingsStorage;
        private readonly PortfolioManager _portfolioManager;

        public HoldingsService(HoldingsStorage holdingsStorage)
        {
            _holdingsStorage = holdingsStorage;
            _portfolioManager = new PortfolioManager();
        }

        public event Action<HoldingsUpdatedEventArg> HoldingsChanged;

        public async Task<List<HoldingsTransaction>> GetTransactions()
        {
            return await _holdingsStorage.GetTransactions().ConfigureAwait(false);
        }

        /*public async Task<HoldingsSummary> GetSnapshotSummary(string code, DateTime date)
        {
            var resultSummary = await _holdingsStorage.GetSummary(code).ConfigureAwait(false);
            var transactions = await _holdingsStorage.GetRelatedTransactions(code).ConfigureAwait(false);
            foreach (var transaction in transactions.Where(transaction => transaction.Date>=date).OrderByDescending(transaction => transaction.Date))
            {
                var changes = transaction.Portfoliotest.GetUndoChanges();
                var change = changes.FirstOrDefault(changing => changing.Currency == code);
                if (change != null)
                {
                    resultSummary.Amount += change.AmountChange;
                }
            }

            return resultSummary;
        }*/

        public async Task<HoldingsTransaction> GetTransaction(int id)
        {
            return await _holdingsStorage.GetTransaction(id).ConfigureAwait(false);
        }

        public async Task<List<HoldingsSummary>> GetHoldings()
        {
            return await _holdingsStorage.GetSummaries().ConfigureAwait(false);
        }
        private async Task<HoldingsSummary> GetSummary(string currency)
        {
            return await _holdingsStorage.GetSummary(currency).ConfigureAwait(false);
        }

        private async Task<List<HoldingsSummary>> ApplyChanges(HoldingsTransaction transaction, Dictionary<string, HoldingsChanging> changing)
        {
            var result = new List<HoldingsSummary>();
            foreach (var currency in changing.Keys)
            {
                var summary = await _holdingsStorage.GetSummary(currency) ?? new HoldingsSummary(currency, decimal.Zero);
                var summaries = new List<HoldingsSummary>() {summary};
                _portfolioManager.ApplyChanges(summaries, changing);

                result.Add(summary);
            }

            return result;
        }

        public async Task AddTransaction(HoldingsTransaction transaction)
        {
            var changes = _portfolioManager.GetApplyChanges(transaction);

            var summaries = changes.Keys.Select(s => new HoldingsSummary(s, decimal.Zero)).ToList();
            var transactionSet = new HashSet<HoldingsTransaction>(new LambdaEqualable<int, HoldingsTransaction>(t => t.Id));
            foreach (var summary in summaries)
            {
                var relatedTransactions = await _holdingsStorage.GetRelatedTransactions(summary.Currency);
                transactionSet.UnionWith(relatedTransactions);
            }

            transactionSet.Add(transaction);
            foreach (var t in transactionSet.ToLookup(t => t.Date).OrderBy(grouping => grouping.Key))
            {
                var applyChanges = _portfolioManager.GetApplyChanges(t);
                _portfolioManager.ApplyChanges(summaries, applyChanges);
            }

            await _holdingsStorage.AddTransaction(transaction, summaries);
            HoldingsChanged?.Invoke(new HoldingsUpdatedEventArg(UpdateAction.Add, transaction, summaries));
        }

        public async Task DeleteTransaction(HoldingsTransaction transaction)
        {
            var changes = _portfolioManager.GetUndoChanges(transaction);

            var summaries = changes.Keys.Select(s => new HoldingsSummary(s, decimal.Zero)).ToList();
            var transactionSet = new HashSet<HoldingsTransaction>(new LambdaEqualable<int, HoldingsTransaction>(t => t.Id));
            foreach (var summary in summaries)
            {
                var relatedTransactions = await _holdingsStorage.GetRelatedTransactions(summary.Currency);
                transactionSet.UnionWith(relatedTransactions);
            }

            var match = transactionSet.FirstOrDefault(t => t.Id == transaction.Id);
            transactionSet.Remove(match);
            foreach (var t in transactionSet.ToLookup(t => t.Date).OrderBy(grouping => grouping.Key))
            {
                var applyChanges = _portfolioManager.GetApplyChanges(t);
                _portfolioManager.ApplyChanges(summaries, applyChanges);
            }

            await _holdingsStorage.DeleteTransaction(transaction, summaries);
            HoldingsChanged?.Invoke(new HoldingsUpdatedEventArg(UpdateAction.Remove, transaction, summaries));
        }

        public async Task UpdateTransaction(HoldingsTransaction transaction)
        {
            var oldTransaction = await _holdingsStorage.GetTransaction(transaction.Id);
            var undoChanges = _portfolioManager.GetUndoChanges(oldTransaction);
            var changes = _portfolioManager.GetApplyChanges(transaction, undoChanges);

            var summaries = changes.Keys.Select(s => new HoldingsSummary(s, decimal.Zero)).ToList();
            var transactionSet = new HashSet<HoldingsTransaction>(new LambdaEqualable<int, HoldingsTransaction>(t => t.Id));

            transactionSet.Add(transaction);
            foreach (var summary in summaries)
            {
                var relatedTransactions = await _holdingsStorage.GetRelatedTransactions(summary.Currency);
                transactionSet.UnionWith(relatedTransactions);
            }

            foreach (var t in transactionSet.ToLookup(t => t.Date).OrderBy(grouping => grouping.Key))
            {
                var applyChanges = _portfolioManager.GetApplyChanges(t);
                _portfolioManager.ApplyChanges(summaries, applyChanges);
            }

            await _holdingsStorage.UpdateHoldings(transaction, summaries);
            HoldingsChanged?.Invoke(new HoldingsUpdatedEventArg(UpdateAction.Replace, transaction, summaries));
        }
    }
}
