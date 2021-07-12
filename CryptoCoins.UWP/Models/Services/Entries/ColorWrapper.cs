using Windows.UI;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class ColorWrapper<T>
    {
        public T Value { get; set; }
        public Color Color { get; set; }
    }
}
