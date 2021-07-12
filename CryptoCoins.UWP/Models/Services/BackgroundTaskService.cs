using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Platform.Activation;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using Microsoft.Practices.ServiceLocation;

namespace CryptoCoins.UWP.Models.Services
{
    internal class BackgroundTaskService : ActivationHandler<BackgroundActivatedEventArgs>
    {
        public static IEnumerable<BackgroundTask> BackgroundTasks => backgroundTasks.Value;

        private static readonly Lazy<IEnumerable<BackgroundTask>> backgroundTasks = 
            new Lazy<IEnumerable<BackgroundTask>>(() => CreateInstances());

        public async Task RegisterBackgroundTasks()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();
            foreach (var task in BackgroundTasks)
            {
                //TODO: registration shouldn't create an instance
                if (!(task is SyncBackgroundTask))
                {
                    task.Register();
                }
            }
        }

        public static BackgroundTaskRegistration GetBackgroundTasksRegistration<T>() where T : BackgroundTask
        {
            if (!BackgroundTaskRegistration.AllTasks.Any(t => t.Value.Name == typeof(T).Name))
            {
                // This condition should not be met, if so it means the background task was not registered correctly.
                // Please check CreateInstances to see if the background task was properly added to the BackgroundTasks property.
                return null;
            }
            return (BackgroundTaskRegistration)BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == typeof(T).Name).Value;
        }

        public async Task Start(IBackgroundTaskInstance taskInstance)
        {
            var task = BackgroundTasks.FirstOrDefault(b => b.Match(taskInstance?.Task?.Name));

            if (task == null)
            {
                // This condition should not be met, if so it means the background task to start was not found in the background tasks managed by this service. 
                // Please check CreateInstances to see if the background task was properly added to the BackgroundTasks property.
                return;
            }

            await task.RunAsync(taskInstance);
        }

        protected override async Task HandleInternalAsync(BackgroundActivatedEventArgs args)
        {
            await Start(args.TaskInstance);
        }

        private static IEnumerable<BackgroundTask> CreateInstances()
        {
            var tasks = new List<BackgroundTask> {ServiceLocator.Current.GetInstance<TileUpdateTask>(), ServiceLocator.Current.GetInstance<AlertsUpdateTask>(), ServiceLocator.Current.GetInstance<SyncBackgroundTask>()};

            return tasks;
        }
    }
}
