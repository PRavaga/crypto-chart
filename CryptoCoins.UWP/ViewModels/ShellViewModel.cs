using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Views;

namespace CryptoCoins.UWP.ViewModels
{
    public class ShellViewModel : Observable
    {
        private const string PanoramicStateName = "PanoramicState";
        private const string WideStateName = "WideState";
        private const string NarrowStateName = "NarrowState";
        private const double WideStateMinWindowWidth = 640;
        private const double PanoramicStateMinWindowWidth = 1024;
        private readonly NavigationService _navigationService;

        private SplitViewDisplayMode _displayMode = SplitViewDisplayMode.CompactInline;

        private bool _isPaneOpen;

        private ICommand _itemSelected;

        private object _lastSelectedItem;

        private ICommand _openPaneCommand;

        private ObservableCollection<ShellNavigationItem> _primaryItems = new ObservableCollection<ShellNavigationItem>();

        private ObservableCollection<ShellNavigationItem> _secondaryItems = new ObservableCollection<ShellNavigationItem>();

        private ICommand _stateChangedCommand;

        public ShellViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, value);
        }

        public SplitViewDisplayMode DisplayMode
        {
            get => _displayMode;
            set => Set(ref _displayMode, value);
        }

        public ObservableCollection<ShellNavigationItem> PrimaryItems
        {
            get => _primaryItems;
            set => Set(ref _primaryItems, value);
        }

        public ObservableCollection<ShellNavigationItem> SecondaryItems
        {
            get => _secondaryItems;
            set => Set(ref _secondaryItems, value);
        }

        public ICommand OpenPaneCommand
        {
            get
            {
                if (_openPaneCommand == null)
                {
                    _openPaneCommand = new RelayCommand(() => IsPaneOpen = !_isPaneOpen);
                }

                return _openPaneCommand;
            }
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                if (_itemSelected == null)
                {
                    _itemSelected = new RelayCommand<ItemClickEventArgs>(ItemSelected);
                }

                return _itemSelected;
            }
        }

        public ICommand StateChangedCommand
        {
            get
            {
                if (_stateChangedCommand == null)
                {
                    _stateChangedCommand = new RelayCommand<VisualStateChangedEventArgs>(args => GoToState(args.NewState.Name));
                }

                return _stateChangedCommand;
            }
        }

        private void InitializeState(double windowWith)
        {
            if (windowWith < WideStateMinWindowWidth)
            {
                GoToState(NarrowStateName);
            }
            else if (windowWith < PanoramicStateMinWindowWidth)
            {
                GoToState(WideStateName);
            }
            else
            {
                GoToState(PanoramicStateName);
            }
        }

        private void GoToState(string stateName)
        {
            switch (stateName)
            {
                case PanoramicStateName:
                    DisplayMode = SplitViewDisplayMode.CompactInline;
                    break;
                case WideStateName:
                    DisplayMode = SplitViewDisplayMode.CompactInline;
                    IsPaneOpen = false;
                    break;
                case NarrowStateName:
                    DisplayMode = SplitViewDisplayMode.Overlay;
                    IsPaneOpen = false;
                    break;
                default:
                    break;
            }
        }

        public void Initialize(Frame frame)
        {
            _navigationService.HomePage = typeof(DashboardPage);
            _navigationService.Frame = frame;
            _navigationService.Frame.Navigated += NavigationService_Navigated;
            PopulateNavItems();

            InitializeState(Window.Current.Bounds.Width);
        }

        private void PopulateNavItems()
        {
            _primaryItems.Clear();
            _secondaryItems.Clear();

            // More on Segoe UI Symbol icons: https://docs.microsoft.com/windows/uwp/style/segoe-ui-symbol-font
            // Edit String/en-US/Resources.resw: Add a menu item title for each page
            _primaryItems.Add(ShellNavigationItem.FromType<DashboardPage>("Shell_DashboardPage".GetLocalized(), new Uri("ms-appx:///Assets/Images/Dashboard.png")));
            _primaryItems.Add(ShellNavigationItem.FromType<PortfolioPage>("Shell_PortfolioPage".GetLocalized(), new Uri("ms-appx:///Assets/Images/Portfolio.png")));
            _primaryItems.Add(ShellNavigationItem.FromType<AlertsPage>("Shell_AlertsPage".GetLocalized(), new Uri("ms-appx:///Assets/Images/Alert.png")));
            _primaryItems.Add(ShellNavigationItem.FromType<NewsFeedPage>("Shell_NewsPage".GetLocalized(), new Uri("ms-appx:///Assets/Images/News.png")));
            _primaryItems.Add(ShellNavigationItem.FromType<CoinsPage>("Shell_CoinsPage".GetLocalized(), new Uri("ms-appx:///Assets/Images/Coins.png")));
            //var wallet = ShellNavigationItem.FromType<CoinsPage>("Shell_WalletPage".GetLocalized(), Symbol.Document);
            //wallet.IsEnabled = false;
            //_primaryItems.Add(wallet);
            var settingsItem = ShellNavigationItem.FromType<SettingsPage>("Shell_SettingsPage".GetLocalized(), Symbol.Setting);
            _secondaryItems.Add(settingsItem);
            var donateItem = ShellNavigationItem.FromType<SupportUsPage>("Shell_SupportUsPage".GetLocalized(), Symbol.Like);
            _secondaryItems.Add(donateItem);

            _navigationService.TopPages.AddRange(_primaryItems.Select(item => item.PageType).Concat(_secondaryItems.Select(item => item.PageType)));
        }

        private void ItemSelected(ItemClickEventArgs args)
        {
            if (DisplayMode == SplitViewDisplayMode.CompactOverlay || DisplayMode == SplitViewDisplayMode.Overlay)
            {
                IsPaneOpen = false;
            }
            Navigate(args.ClickedItem);
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            var navigationItem = PrimaryItems?.FirstOrDefault(i => i.PageType == e?.SourcePageType);
            if (navigationItem == null)
            {
                navigationItem = SecondaryItems?.FirstOrDefault(i => i.PageType == e?.SourcePageType);
            }

            if (navigationItem != null)
            {
                ChangeSelected(_lastSelectedItem, navigationItem);
                _lastSelectedItem = navigationItem;
            }
        }

        private void ChangeSelected(object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                (oldValue as ShellNavigationItem).IsSelected = false;
            }
            if (newValue != null)
            {
                (newValue as ShellNavigationItem).IsSelected = true;
            }
        }

        private void Navigate(object item)
        {
            var navigationItem = item as ShellNavigationItem;
            if (navigationItem != null)
            {
                _navigationService.Navigate(navigationItem.PageType);
            }
        }
    }
}
