using System;
using System.Threading.Tasks;
using MetroLog;

namespace CryptoCoins.UWP.Helpers
{
    public static class ExceptionExtensions
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(nameof(ExceptionExtensions));

        public static async Task<T> Safe<T>(this Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error("Skipping exception", e);
            }
            return default(T);
        }
    }
}
