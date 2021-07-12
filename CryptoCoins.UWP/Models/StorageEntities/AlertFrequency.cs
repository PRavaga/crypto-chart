using System.ComponentModel;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    public enum AlertFrequency
    {
        [Description("AlertFrequency_OneTime")]
        OneTime,
        [Description("AlertFrequency_EveryTime")]
        EveryTime
    }
}
