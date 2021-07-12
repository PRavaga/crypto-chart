using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Platform.Activation;
using MetroLog;
using Microsoft.Practices.ServiceLocation;

namespace CryptoCoins.UWP.Models.Services
{
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        // TODO UWPTemplates: For more information regarding the application lifecycle and how to handle suspend and resume, please see:
        // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SuspendAndResumeService>();
        private const string stateFilename = "suspensionState";

        // TODO UWPTemplates: This event is fired just before the app enters in background. Subscribe to this event if you want to save your current state.
        public event EventHandler<OnBackgroundEnteringEventArgs> OnBackgroundEntering;

        public async Task SaveStateAsync()
        {
            var suspensionState = new SuspensionState()
            {
                SuspensionDate = DateTime.Now
            };

            var target = OnBackgroundEntering?.Target.GetType();
            var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target);

            OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

            try
            {
                await ApplicationData.Current.LocalCacheFolder.SaveAsync(stateFilename, onBackgroundEnteringArgs);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save suspension state", e);
            }
        }

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await RestoreStateAsync();
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return args.PreviousExecutionState == ApplicationExecutionState.Terminated;
        }

        private async Task RestoreStateAsync()
        {
            var saveState = await ApplicationData.Current.LocalCacheFolder.ReadAsync<OnBackgroundEnteringEventArgs>(stateFilename);
            if (saveState?.Target != null && typeof(Page).IsAssignableFrom(saveState.Target))
            {
                var navigationService = ServiceLocator.Current.GetInstance<NavigationService>();
                navigationService.Navigate(saveState.Target, saveState.SuspensionState);
            }
        }
    }
}
