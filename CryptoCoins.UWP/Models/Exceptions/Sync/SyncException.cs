using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Models.Exceptions.Sync
{
    public class SyncException:Exception
    {
        public SyncException()
        {
        }

        public SyncException(string message) : base(message)
        {
        }

        public SyncException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
