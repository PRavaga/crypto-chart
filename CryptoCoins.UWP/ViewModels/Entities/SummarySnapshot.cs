using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.ViewModels.Entities
{
    public class SummarySnapshot:Observable
    {
        private double _value;

        public double Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        private DateTime _date;

        public DateTime Date
        {
            get => _date;
            set => Set(ref _date, value);
        }
    }
}
