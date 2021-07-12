using System;
using System.ComponentModel;
using Nito.Disposables;

namespace CryptoCoins.UWP.Models.Services
{
    public class ProgressService : INotifyPropertyChanged
    {
        private int _backgroundOperationCount;
        private int _uiOperationCount;

        public bool UiOperation => _uiOperationCount > 0;
        public bool BackgroundOperation => _backgroundOperationCount > 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public IDisposable BeginUiOperation()
        {
            _uiOperationCount++;
            OnPropertyChanged(nameof(UiOperation));
            return new AnonymousDisposable(() =>
            {
                if (--_uiOperationCount == 0)
                {
                    OnPropertyChanged(nameof(UiOperation));
                }
            });
        }

        public IDisposable BeginBackgroundOperation()
        {
            _backgroundOperationCount++;
            OnPropertyChanged(nameof(BackgroundOperation));
            return new AnonymousDisposable(() =>
            {
                if (--_backgroundOperationCount == 0)
                {
                    OnPropertyChanged(nameof(BackgroundOperation));
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
