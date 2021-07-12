using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Platform.Behaviors;
using CryptoCoins.UWP.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewsFeedPage : MvvmPage
    {
        public NewsFeedPage()
        {
            InitializeComponent();
        }

        public NewsFeedViewModel ViewModel => (NewsFeedViewModel) DataContext;

        public void SelectBehavior_OnSelectionChanged(object sender, SelectionEventArgs e)
        {
            
        }
    }
}
