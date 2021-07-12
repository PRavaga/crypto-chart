using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.ViewModels.Entities;

namespace CryptoCoins.UWP.Views.Controls
{
    public class BarPanel : Panel
    {
        private List<double> _holdings;
        private double _sum;

        protected override Size MeasureOverride(Size availableSize)
        {
            _holdings = Children.Select(element =>
            {
                var info = (HoldingsSummary) ((ContentPresenter) element).Content;
                var value = info.Value;
                return double.IsNaN(value) ? 0d : value;
            }).ToList();
            _sum = _holdings.Sum();
            for (var i = 0; i < Children.Count; i++)
            {
                var width = _sum == 0d ? 0d : _holdings[i] / _sum * availableSize.Width;
                Children[i].Measure(new Size(width, availableSize.Height));
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var offset = new Rect(0d, 0d, 0d, finalSize.Height);
            for (var i = 0; i < Children.Count; i++)
            {
                offset.X += offset.Width;
                offset.Width = _sum == 0d ? 0d : _holdings[i] / _sum * finalSize.Width;
                Children[i].Arrange(offset);
            }
            return finalSize;
        }
    }
}
