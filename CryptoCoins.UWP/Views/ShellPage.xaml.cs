using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using CryptoCoins.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace CryptoCoins.UWP.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            InitializeComponent();
            ViewModel.Initialize(shellFrame);
        }

        public ShellViewModel ViewModel => (ShellViewModel) DataContext;
    }
}
