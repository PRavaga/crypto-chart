using System;
using Template10.Validation;

namespace CryptoCoins.UWP.ViewModels.Entities
{
    public class HoldingsTransaction : ValidatableModelBase
    {
        public HoldingsTransaction()
        {
        }

        public HoldingsTransaction(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public string BaseCode
        {
            get => Read<string>();
            set => Write(value);
        }

        public string CounterCode
        {
            get => Read<string>();
            set => Write(value);
        }

        public decimal? Price
        {
            get => Read<decimal?>();
            set => Write(value);
        }

        public decimal? Amount
        {
            get => Read<decimal?>();
            set => Write(value);
        }

        public string Comment
        {
            get => Read<string>();
            set => Write(value);
        }

        public DateTimeOffset Date
        {
            get => Read<DateTimeOffset>();
            set => Write(value);
        }

        public TransactionType Type
        {
            get => Read<TransactionType>();
            set => Write(value);
        }

        public Uri BaseIcon
        {
            get => Read<Uri>();
            set => Write(value);
        }

        public Uri CounterIcon
        {
            get => Read<Uri>();
            set => Write(value);
        }
    }
}
