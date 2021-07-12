using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using CryptoCoins.UWP.ViewModels;

namespace CryptoCoins.UWP.Views
{
    public sealed partial class SettingsPage : MvvmPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        public SettingsViewModel ViewModel => (SettingsViewModel) DataContext;
    }
}
