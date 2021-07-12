using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Platform
{
    public class SelectWrapper<T> : Observable
    {
        private bool _isSelected;
        public T Value { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
    }
}
