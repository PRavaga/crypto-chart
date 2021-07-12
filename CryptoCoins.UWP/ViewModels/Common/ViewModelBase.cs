using System.Threading.Tasks;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Platform;
using MetroLog;
using Microsoft.Practices.ServiceLocation;

namespace CryptoCoins.UWP.ViewModels.Common
{
    public abstract class ViewModelBase : Observable
    {
        public const string NavigationParameterFolder = "NavigationParameter";
        protected readonly ILogger Logger;

        protected ViewModelBase()
        {
            Logger = LogManagerFactory.DefaultLogManager.GetLogger(GetType().FullName);
        }

        public bool IsActive { get; private set; }

        /// <summary>
        ///     Override this method to handle navigation to a page associated with this <see cref="ViewModelBase" />
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void OnNavigatedTo(object parameter)
        {
        }

        public void NavigatedTo(object parameter)
        {
            IsActive = true;
            OnNavigatedTo(parameter);
        }

        /// <summary>
        ///     Override this method to handle navigation from a page associated with this <see cref="ViewModelBase" />
        /// </summary>
        /// <param name="parameter"></param>
        public virtual void OnNavigatedFrom(object parameter)
        {
        }

        public void NavigatedFrom(object parameter)
        {
            IsActive = false;
            OnNavigatedFrom(parameter);
        }

        public virtual void OnActivated()
        {
        }

        public void Activate()
        {
            IsActive = true;
            OnActivated();
        }

        public virtual void OnDeactivated()
        {
        }

        public void Deactivate()
        {
            IsActive = false;
            OnDeactivated();
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameter"></param>
        /// <returns>
        ///     Id that can be used to retrieve the parameter using <see cref="GetNavigationParameter{T}" />. Returns
        ///     <see langword="null" /> if case of failure.
        /// </returns>
        protected async Task<string> SetNavigationParameter<T>(T parameter)
        {
            var storageService = ServiceLocator.Current.GetInstance<StorageService>();
            var filename = storageService.GetSafeFilename<T>() + GetType().Name;
            await storageService.Save(parameter, filename);
            return filename;
        }


        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///     Paremeter that was set using <see cref="SetNavigationParameter{T}" />. Returns <see langword="null" /> in case
        ///     of failure.
        /// </returns>
        protected async Task<T> GetNavigationParameter<T>(string parameterId)
        {
            var storageService = ServiceLocator.Current.GetInstance<StorageService>();
            return await storageService.Load<T>(parameterId);
        }
    }
}
