using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    public class HoldingsChanging
    {
        public string Currency { get; set; }
        public decimal AmountChange { get; set; }
    }
}
