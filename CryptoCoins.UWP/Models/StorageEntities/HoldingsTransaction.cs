using System;
using System.Collections.Generic;
using SQLite;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    public class HoldingsTransaction
    {
        public HoldingsTransaction()
        {
        }


        public HoldingsTransaction(string baseCode, decimal amount, DateTime date, string comment, string counterCode, decimal price, TransactionType type, int id)
        {
            Id = id;
            BaseCode = baseCode;
            CounterCode = counterCode;
            Type = type;
            Price = price;
            Amount = amount;
            Comment = comment;
            Date = date;
        }

        public HoldingsTransaction(string baseCode, decimal amount, DateTime date, string comment, string counterCode, decimal price, TransactionType type) : this(baseCode,
            amount, date, comment, counterCode, price, type, 0)
        {
        }

        public HoldingsTransaction(string baseCode, decimal amount, DateTime date, string comment) : this(baseCode, amount, date, comment, null, 0, TransactionType.Buy)
        {
        }

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string BaseCode { get; set;}

        public string CounterCode { get; set;}

        public TransactionType Type { get; set;}

        public decimal Price { get; set;}

        public decimal Amount { get; set;}

        public string Comment { get; set;}

        public DateTime Date { get; set;}

        public bool IsAirDrop => Type == TransactionType.Buy && Price == decimal.Zero;

        public string FromCode
        {
            get
            {
                switch (Type)
                {
                    case TransactionType.Sell:
                        return BaseCode;
                    case TransactionType.Buy:
                        return CounterCode;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string ToCode
        {
            get
            {
                switch (Type)
                {
                    case TransactionType.Sell:
                        return CounterCode;
                    case TransactionType.Buy:
                        return BaseCode;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public decimal ToAmount
        {
            get
            {
                switch (Type)
                {
                    case TransactionType.Sell:
                        return Amount * Price;
                    case TransactionType.Buy:
                        return Amount;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public decimal FromAmount
        {
            get
            {
                switch (Type)
                {
                    case TransactionType.Sell:
                        return Amount;
                    case TransactionType.Buy:
                        return Amount * Price;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
