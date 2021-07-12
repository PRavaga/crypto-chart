using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public abstract class BackgroundTask
    {
        public abstract void Register();

        public abstract Task RunAsyncInternal(IBackgroundTaskInstance taskInstance);

        public virtual void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            sender.Canceled -= OnCanceled;
            sender.Task.Completed -= OnCompleted;
        }

        public virtual void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            sender.Completed -= OnCompleted;
        }

        public bool Match(string name)
        {
            return (name == GetType().Name);
        }

        public async Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnCanceled;
            taskInstance.Task.Completed += OnCompleted;

            await RunAsyncInternal(taskInstance);

            taskInstance.Canceled -= OnCanceled;
        }
    }
}
