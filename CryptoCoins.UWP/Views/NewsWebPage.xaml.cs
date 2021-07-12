using CryptoCoins.UWP.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewsWebPage : MvvmPage
    {
        public NewsWebPage()
        {
            InitializeComponent();
            ViewModel.Initialize(WebView);
        }

        public NewsWebViewModel ViewModel => (NewsWebViewModel) DataContext;
    }
}
