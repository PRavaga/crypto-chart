using System;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public class DeviceContextLostException : Exception
    {
        public DeviceContextLostException()
        {
        }

        public DeviceContextLostException(string message) : base(message)
        {
        }

        public DeviceContextLostException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
