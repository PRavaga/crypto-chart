using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public enum DataState
    {
        NotReady,
        Available,
        Empty,
        FilteredEmpty,
        Cached,
        Unavailable
    }
}
