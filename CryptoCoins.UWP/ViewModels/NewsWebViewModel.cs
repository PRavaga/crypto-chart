using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.ViewModels.Common;

namespace CryptoCoins.UWP.ViewModels
{
    public class NewsWebViewModel : ViewModelBase
    {
        public class NavigationParameter
        {
            public string Uri { get; set; }
            public string Title { get; set; }
        }
        private readonly NavigationService _navigationService;

        private DataState _dataState;
        private IDisposable _loadingOperation;

        private Uri _uri;

        public NewsWebViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public Uri Uri
        {
            get => _uri;
            set => Set(ref _uri, value);
        }

        public string Title { get; set; }

        public WebView WebView { get; private set; }

        public DataState DataState
        {
            get => _dataState;
            set => Set(ref _dataState, value);
        }

        public ProgressState ProgressState { get; } = new ProgressState();

        public void Initialize(WebView webView)
        {
            WebView = webView;
            WebView.DOMContentLoaded += WebViewOnDomContentLoaded;
            WebView.NavigationFailed += WebViewOnNavigationFailed;
            WebView.NavigationStarting += WebViewOnNavigationStarting;
        }

        private void WebViewOnDomContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            DataState = DataState.Available;
            _loadingOperation?.Dispose();
        }

        private void WebViewOnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            DataState = DataState.NotReady;
            _loadingOperation = ProgressState.BeginOperation();
        }

        private RelayCommand _shareCommand;

        public RelayCommand ShareCommand => _shareCommand ?? (_shareCommand = new RelayCommand(() =>
        {
            var shareManager = DataTransferManager.GetForCurrentView();
            shareManager.DataRequested+=ShareManagerOnDataRequested;
            DataTransferManager.ShowShareUI();
        }));

        private void ShareManagerOnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = string.Format("NewsWebPage_ShareTitle".GetLocalized(),Title);
            args.Request.Data.Properties.Description = "NewsWebPage_ShareDescription".GetLocalized();
            args.Request.Data.SetText(string.Format("NewsWebPage_ShareText".GetLocalized(), Title));
            args.Request.Data.SetWebLink(Uri);
        }

        private void WebViewOnNavigationFailed(object sender, WebViewNavigationFailedEventArgs webViewNavigationFailedEventArgs)
        {
            DataState = DataState.Unavailable;
        }

        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is NavigationParameter navParameter && Uri.TryCreate(navParameter.Uri, UriKind.Absolute, out var uri))
            {
                Uri = uri;
                Title = navParameter.Title;
                WebView.Navigate(uri);
            }
            else
            {
                _navigationService.GoBack();
            }
        }

        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new RelayCommand(() =>
        {
            WebView.Refresh();
        }));
    }
}
