using CryptoCoins.UWP.Platform.Dialogs;

namespace CryptoCoins.UWP.ViewModels.Common
{
    public abstract class DialogViewModelBase : ViewModelBase
    {
        public IDialogController DialogController { get; set; }
        public abstract void OnCancelled();
    }
}
