using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.Services.Portfolio;
using CryptoCoins.UWP.Models.StorageEntities;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class HoldingsStorage : IHoldingsProvider
    {
        private readonly SqLiteConnectionProvider _connectionProvider;

        public HoldingsStorage(SqLiteConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<HoldingsSummary> GetSummary(string code)
        {
            var connection = await _connectionProvider.GetConnection();
            return await connection.FindAsync<HoldingsSummary>(code).ConfigureAwait(false);
        }

        public async Task<HoldingsTransaction> GetTransaction(int id)
        {
            var connection = await _connectionProvider.GetConnection();
            return await connection.FindAsync<HoldingsTransaction>(id).ConfigureAwait(false);
        }

        public async Task<List<HoldingsTransaction>> GetTransactions()
        {
            var connection = await _connectionProvider.GetConnection();
            return await connection.Table<HoldingsTransaction>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<HoldingsTransaction>> GetRelatedTransactions(string code)
        {
            var connection = await _connectionProvider.GetConnection();
            return await connection.Table<HoldingsTransaction>().Where(transaction => transaction.BaseCode == code || transaction.CounterCode == code).ToListAsync().ConfigureAwait(false);
        }

        public async Task<List<HoldingsSummary>> GetSummaries()
        {
            var connection = await _connectionProvider.GetConnection();
            return await connection.Table<HoldingsSummary>().ToListAsync().ConfigureAwait(false);
        }

        public async Task AddTransaction(HoldingsTransaction transaction, IEnumerable<HoldingsSummary> summaries)
        {
            var connection = await _connectionProvider.GetConnection();
            await connection.RunInTransactionAsync(c =>
            {
                c.Insert(transaction);
                foreach (var summary in summaries)
                {
                    //connection.InsertOrReplace(summary);
                    if (summary.Amount == decimal.Zero)
                    {
                        c.Delete<HoldingsSummary>(summary.Currency);
                    }
                    else
                    {
                        c.InsertOrReplace(summary);
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteTransaction(HoldingsTransaction transaction, IEnumerable<HoldingsSummary> summaries)
        {
            var connection = await _connectionProvider.GetConnection();
            await connection.RunInTransactionAsync(c =>
            {
                c.Delete(transaction);
                foreach (var summary in summaries)
                {
                    //connection.InsertOrReplace(summary);
                    if (summary.Amount == decimal.Zero)
                    {
                        c.Delete<HoldingsSummary>(summary.Currency);
                    }
                    else
                    {
                        c.InsertOrReplace(summary);
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task UpdateHoldings(HoldingsTransaction transaction, IEnumerable<HoldingsSummary> summaries)
        {
            var connection = await _connectionProvider.GetConnection();
            await connection.RunInTransactionAsync(c =>
            {
                c.Update(transaction);
                foreach (var summary in summaries)
                {
                    //connection.InsertOrReplace(summary);
                    if (summary.Amount == decimal.Zero)
                    {
                        c.Delete<HoldingsSummary>(summary.Currency);
                    }
                    else
                    {
                        c.InsertOrReplace(summary);
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}
