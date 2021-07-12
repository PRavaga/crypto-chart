using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.ViewModels.Entities
{
    public class SpendingsSummary
    {
        public string Currency { get; set; }
        public double Amount { get; set; }
        public double Rate { get; set; }
    }
}
