using CryptoCoins.UWP.Views.Formatter;
using Telerik.Charting;

namespace CryptoCoins.UWP.Views.Entities
{
    public class FixedLabelFormatter : IContentFormatter
    {
        public object Format(object owner, object content)
        {
            return Value.FormatNumber((double) (decimal) content, 6);
        }
    }
}
