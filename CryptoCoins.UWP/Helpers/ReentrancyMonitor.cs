using System;
using Nito.Disposables;

namespace CryptoCoins.UWP.Helpers
{
    public class ReentrancyMonitor
    {
        private readonly int _maxCount;
        private int _operationsCount;

        public ReentrancyMonitor(int maxCount)
        {
            _maxCount = maxCount;
        }

        public bool IsAvailable => _operationsCount < _maxCount;

        public IDisposable BeginOperation()
        {
            ++_operationsCount;
            return new AnonymousDisposable(() => { --_operationsCount; });
        }
    }
}
