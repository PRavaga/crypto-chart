using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace CryptoCoins.UWP.Models.Services
{
    public interface IBitmapProvider
    {
        Task<RandomAccessStreamReference> GetBitmap(object arg);
    }
}
