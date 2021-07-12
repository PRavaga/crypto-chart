using System;
using CryptoCoins.UWP.Models.Services.Entries;

namespace CryptoCoins.UWP.Platform.Behaviors
{
    public class NewsSelectBehavior : SelectBehavior<NewsSource>
    {
        protected override bool IsSelected(NewsSource item)
        {
            return item.IsEnabled;
        }

        protected override void SetSelected(NewsSource item, bool value)
        {
            item.IsEnabled = value;
        }
    }
}
