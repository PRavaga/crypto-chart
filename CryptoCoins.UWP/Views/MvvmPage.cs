using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using CryptoCoins.UWP.ViewModels.Common;
using MetroLog;
using Microsoft.AppCenter.Analytics;

namespace CryptoCoins.UWP.Views
{
    public abstract class MvvmPage : Page
    {
        private readonly ILogger _log;

        protected MvvmPage()
        {
            _log = LogManagerFactory.DefaultLogManager.GetLogger(GetType());
        }

        private ViewModelBase VmBase => (ViewModelBase) DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _log.Trace($"Navigated to {GetType().Name}");
            Analytics.TrackEvent("Page shown", new Dictionary<string, string>{{"Name", GetType().Name}});
            base.OnNavigatedTo(e);
            VmBase?.NavigatedTo(e.Parameter);
            Application.Current.LeavingBackground += OnLeavingBackground;
            Application.Current.EnteredBackground += OnEnteredBackground;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _log.Trace($"Navigated from {GetType().Name}");
            Application.Current.LeavingBackground -= OnLeavingBackground;
            Application.Current.EnteredBackground -= OnEnteredBackground;
            base.OnNavigatedFrom(e);
            VmBase?.NavigatedFrom(e.Parameter);
        }

        private void OnEnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            VmBase?.Deactivate();
        }

        private void OnLeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            VmBase?.Activate();
        }
    }
}
