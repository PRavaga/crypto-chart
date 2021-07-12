using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.UserPreferences;
using Microsoft.Practices.ServiceLocation;

namespace CryptoCoins.UWP.Platform.Activation
{
    internal class DefaultLaunchActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        private readonly Type _navElement;
        private readonly NavigationService _navigationService;
    
        public DefaultLaunchActivationHandler(Type navElement)
        {
            _navElement = navElement;
            _navigationService = ServiceLocator.Current.GetInstance<NavigationService>();
        }
    
        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            _navigationService.Navigate(_navElement, args.Arguments);

            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            // None of the ActivationHandlers has handled the app activation
            return _navigationService.Frame.Content == null;
        }
    }
}
