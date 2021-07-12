using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using CryptoCoins.UWP.Models.Exceptions.Sync;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Sync;
using MetroLog;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public class SyncBackgroundTask : BackgroundTask
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SyncBackgroundTask>();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly SyncService _syncService;
        private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(14);
        public const string LastSucceedSyncDateFilename = "LastSyncDate";

        public SyncBackgroundTask(SyncService syncService)
        {
            _syncService = syncService;
        }

        public override void Register()
        {
            var taskName = GetType().Name;

            if (BackgroundTaskRegistration.AllTasks.All(t => t.Value.Name != taskName))
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = taskName
                };

                builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                builder.SetTrigger(new TimeTrigger(15, false));
                builder.Register();
            }
        }

        public void Unregister()
        {
            var taskName = GetType().Name;
            var registeredTask = BackgroundTaskRegistration.AllTasks.Select(pair => pair.Value).FirstOrDefault(t=> t.Name == taskName);
            if (registeredTask != null)
            {
                registeredTask.Unregister(false);
            }
        }

        public bool IsRegistered
        {
            get
            {
                var taskName = GetType().Name;
                return BackgroundTaskRegistration.AllTasks.Any(pair => pair.Value.Name == taskName);
            }
        }

        public override async Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var lastSync = _syncService.LastSyncDate;
            //var lastSync = DateTimeOffset.MinValue;
            if (lastSync == null || lastSync + SyncInterval < DateTimeOffset.Now)
            {
                try
                {
                    await _syncService.Sync(true).ConfigureAwait(false);
                }
                catch (SyncException ex)
                {
                    Log.Error("Can't sync", ex);
                }
            }
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            base.OnCanceled(sender, reason);
            Log.Info($"Background task cancellation was requested. Reason: {reason}");
            _cancellationTokenSource?.Cancel();
        }

        public override void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            base.OnCompleted(sender, args);
            Log.Info("Background task completed");
        }
    }
}
