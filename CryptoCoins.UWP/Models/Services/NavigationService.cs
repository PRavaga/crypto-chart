using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using MetroLog;

namespace CryptoCoins.UWP.Models.Services
{
    public class NavigationService
    {
        private static readonly ILogger Logger = LogManagerFactory.DefaultLogManager.GetLogger<NavigationService>();
        private Frame _frame;

        public Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
                }

                return _frame;
            }
            set => _frame = value;
        }

        public Type HomePage { get; set; }

        public List<Type> TopPages { get; } = new List<Type>();

        public bool CanGoBack => Frame.CanGoBack;
        public bool CanGoForward => Frame.CanGoForward;

        public void GoBack()
        {
            Frame.GoBack();
            Navigated?.Invoke(this, null);
        }

        public void GoForward()
        {
            Frame.GoForward();
            Navigated?.Invoke(this, null);
        }

        public bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            // Don't open the same page multiple times
            Logger.Trace($"Navigating to {pageType.Name}");
            if (Frame.Content?.GetType() != pageType)
            {
                if (Frame.Navigate(pageType, parameter, infoOverride))
                {
                    KeepOnlyHomePage(pageType);
                    Navigated?.Invoke(this, null);
                    return true;
                }
            }
            return false;
        }

        public void KeepOnlyHomePage(Type currentPage)
        {
            for (var i = _frame.BackStackDepth - 1; i >= 0; i--)
            {
                var entry = _frame.BackStack[i];
                if (TopPages.Contains(currentPage) && (currentPage == HomePage || entry.SourcePageType != HomePage))
                {
                    _frame.BackStack.RemoveAt(i);
                }
            }
        }

        public void DistinctBackStack(Type currentPage)
        {
            for (var i = _frame.BackStackDepth - 1; i >= 0; i--)
            {
                var entry = _frame.BackStack[i];
                if (entry.SourcePageType == currentPage)
                {
                    _frame.BackStack.RemoveAt(i);
                }
            }
        }

        public bool Navigate<T>(object parameter = null, NavigationTransitionInfo infoOverride = null) where T : Page
        {
            return Navigate(typeof(T), parameter, infoOverride);
        }

        public event EventHandler Navigated;
    }
}
