using System;
using CryptoCoins.UWP.Helpers;
using Nito.Disposables;

namespace CryptoCoins.UWP.Models.Services
{
    public class ProgressState : Observable
    {
        private bool _isOperating;
        private int _operationsCount;

        public bool IsOperating
        {
            get => _isOperating;
            set => Set(ref _isOperating, value);
        }

        public IDisposable BeginOperation()
        {
            if (++_operationsCount > 0)
            {
                IsOperating = true;
            }
            return new AnonymousDisposable(() =>
            {
                if (--_operationsCount == 0)
                {
                    IsOperating = false;
                }
            });
        }
    }
}
