using System;
using System.Threading.Tasks;
using CryptoCoins.UWP.Platform.Dialogs;
using MetroLog;

namespace CryptoCoins.UWP.Models.Services
{
    public class DialogService
    {
        private static readonly ILogger Logger = LogManagerFactory.DefaultLogManager.GetLogger<NavigationService>();

        public async Task<object> ShowAsync<T>(object parameter = null) where T : MvvmContentDialog, new()
        {
            var dialog = new T();
            dialog.SetArgument(parameter);
            Logger.Trace($"Opening dialog {typeof(T).Name}");
            await dialog.ShowAsync();
            return dialog.Result;
        }
    }
}
