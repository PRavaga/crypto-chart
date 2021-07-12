using System;

using CryptoCoins.UWP.Helpers;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using CryptoCoins.UWP.Models.Services;

namespace CryptoCoins.UWP.ViewModels
{
    public class ShellNavigationItem : Observable
    {
        private bool _isSelected;

        private Visibility _selectedVis = Visibility.Collapsed;
        public Visibility SelectedVis
        {
            get { return _selectedVis; }
            set { Set(ref _selectedVis, value); }
        }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { Set(ref _isEnabled, value); }
        }

        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar { get { return (char)Symbol; } }
        public Uri IconUri { get; set; }
        public Type PageType { get; set; }
        public bool IsFlyout { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                SelectedVis = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ShellNavigationItem(string name, Symbol symbol, Type pageType)
        {
            this.Label = name;
            this.Symbol = symbol;
            this.PageType = pageType;
        }

        private ShellNavigationItem(string name, Uri iconUri, Type pageType)
        {
            this.Label = name;
            this.IconUri = iconUri;
            this.PageType = pageType;
        }

        public static ShellNavigationItem FromType<T>(string name, Symbol symbol) where T : Page
        {
            return new ShellNavigationItem(name, symbol, typeof(T));
        }

        public static ShellNavigationItem FromType<T>(string name, Uri iconUri) where T : Page
        {
            return new ShellNavigationItem(name, iconUri, typeof(T));
        }
    }
}
