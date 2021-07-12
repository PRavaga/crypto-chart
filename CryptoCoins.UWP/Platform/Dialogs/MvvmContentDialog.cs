using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.ViewModels.Common;

namespace CryptoCoins.UWP.Platform.Dialogs
{
    public class MvvmContentDialog : ContentDialog, IDialogController
    {
        private object _argument;

        public MvvmContentDialog()
        {
            Opened += OnOpened;
            Closed += OnClosed;
        }

        private DialogViewModelBase VmBase => (DialogViewModelBase) DataContext;

        public object Result { get; set; }

        void IDialogController.Hide(object result)
        {
            Result = result;
            Hide();
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (Result == null)
            {
                VmBase.OnCancelled();
            }
            VmBase.NavigatedFrom(null);
        }

        public void SetArgument(object argument)
        {
            _argument = argument;
        }

        private void OnOpened(FrameworkElement sender, object args)
        {
            VmBase.DialogController = this;
            VmBase.NavigatedTo(_argument);
        }
    }
}
