using System;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.ViewModels.Entities;

namespace CryptoCoins.UWP.ViewModels.Converters
{
    public class HoldingsConverter
    {
        private readonly CryptoService _cryptoService;

        public HoldingsConverter(CryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public async Task TryLoadCoins()
        {
            try
            {
                await _cryptoService.EnsureCoinsLoaded().ConfigureAwait(false);
            }
            catch (ApiException)
            {
            }
        }

        public HoldingsTransaction Convert(Models.StorageEntities.HoldingsTransaction transaction)
        {
            var result = new HoldingsTransaction(transaction.Id)
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                BaseCode = transaction.BaseCode,
                CounterCode = transaction.CounterCode,
                Comment = transaction.Comment,
                Date = transaction.Date,
                Price = transaction.Price,
                BaseIcon = _cryptoService.CryptoCurrencyIcon(transaction.BaseCode),
                CounterIcon = !string.IsNullOrEmpty(transaction.CounterCode) ? _cryptoService.CryptoCurrencyIcon(transaction.CounterCode) : null
            };
            if (transaction.IsAirDrop)
            {
                result.Type = TransactionType.AirDrop;
            }
            else
            {
                switch (transaction.Type)
                {
                    case Models.StorageEntities.TransactionType.Sell:
                        result.Type = TransactionType.Sell;
                        break;
                    case Models.StorageEntities.TransactionType.Buy:
                        result.Type = TransactionType.Buy;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        public Models.StorageEntities.HoldingsTransaction Convert(HoldingsTransaction transaction)
        {
            Models.StorageEntities.TransactionType type;
            switch (transaction.Type)
            {
                case TransactionType.Sell:
                    type = Models.StorageEntities.TransactionType.Sell;
                    break;
                case TransactionType.Buy:
                case TransactionType.AirDrop:
                    type = Models.StorageEntities.TransactionType.Buy;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = new Models.StorageEntities.HoldingsTransaction(transaction.BaseCode, transaction.Amount.GetValueOrDefault(), transaction.Date.Date, transaction.Comment,
                transaction.CounterCode,
                transaction.Price.GetValueOrDefault(), type, transaction.Id);

            return result;
        }

        public HoldingsSummary Convert(Models.StorageEntities.HoldingsSummary summary)
        {
            return new HoldingsSummary {Amount = (double) summary.Amount, CurrencyCode = summary.Currency};
        }
    }
}
