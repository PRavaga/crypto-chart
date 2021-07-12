using System.ComponentModel;

namespace CryptoCoins.UWP.ViewModels.Entities
{
    public enum TransactionType
    {
        [Description("TransactionType_Sell")]
        Sell,
        [Description("TransactionType_Buy")]
        Buy,
        [Description("TransactionType_AirDrop")]
        AirDrop
    }
}
