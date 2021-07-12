using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Models.Services.Portfolio
{
    public interface IHoldingsProvider
    {
        Task<HoldingsSummary> GetSummary(string code);
    }
}
