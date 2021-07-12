using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Extensions;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Views.Formatter;
using MetroLog;
using Microsoft.Toolkit.Uwp.Notifications;
using Nito.AsyncEx;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public class AlertsUpdateTask : BackgroundTask
    {
        private const string NotificationGroupName = "RateAlert";
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<AlertsUpdateTask>();
        private readonly AsyncLock asyncLock = new AsyncLock();
        private readonly CryptoService _cryptoService;
        private readonly UserPreferencesService _preferencesService;
        private CancellationTokenSource _cancellationTokenSource;

        public AlertsUpdateTask(UserPreferencesService preferencesService, CryptoService cryptoService)
        {
            _preferencesService = preferencesService;
            _cryptoService = cryptoService;
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

        public override async Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var deferral = taskInstance.GetDeferral();
            try
            {
                try
                {
                    await _preferencesService.InitAsync();
                    await CheckConversions(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    Log.Warn("Alert update task was cancelled", e);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        public async Task CheckConversions(CancellationToken token)
        {
            if (_preferencesService.DisplayPreference.IsAlertsEnabled)
            {
                using (asyncLock.Lock(new CancellationToken(true)))
                {
                    try
                    {
                        var alerts = await _preferencesService.GetAlerts();
                        Log.Info($"Alerts count: {alerts.Count}");
                        var froms = alerts.Select(model => model.FromCode).Distinct().ToList();
                        var tos = alerts.Select(model => model.ToCode).Distinct().ToList();
                        if (froms.Count > 0 && tos.Count > 0)
                        {
                            var conversions = await _cryptoService.GetConversions(froms, tos, token);
                            await CheckConversions(alerts, conversions, token);
                        }
                    }
                    catch (ApiException e)
                    {
                        Log.Error("Failed to check alerts", e);
                    }
                }
            }
        }

        public async Task CheckConversions(List<ConversionInfo> infos, CancellationToken token)
        {
            if (_preferencesService.DisplayPreference.IsAlertsEnabled)
            {
                using (asyncLock.Lock(new CancellationToken(true)))
                {
                    var alerts = await _preferencesService.GetAlerts();
                    await CheckConversions(alerts, infos, token);
                }
            }
        }

        private async Task CheckConversions(List<AlertModel> alerts, List<ConversionInfo> conversions, CancellationToken token)
        {
            foreach (var alert in alerts)
            {
                var info = conversions.FirstOrDefault(i => i.From == alert.FromCode && i.To == alert.ToCode);
                if (info != null)
                {
                    if (alert.IsAlertTriggered(info.Rate))
                    {
                        var toast = CreateAlertNotification(alert, info);
                        ToastNotificationManager.CreateToastNotifier().Show(toast);

                        alert.Fire(info.Rate);
                    }
                    else
                    {
                        alert.SetArmed(info.Rate);
                    }
                    await _preferencesService.UpdateAlert(alert).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    Log.Warn($"Couldn't find conversion infromation for alert: From {alert.FromCode}, To {alert.ToCode}");
                }
            }
        }

        private ToastNotification CreateAlertNotification(AlertModel alert, ConversionInfo info)
        {
            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText {Text = string.Format("AlertNotification_Title".GetLocalized(), alert.FromCode, $"{Currency.CurrencySymbol(info.To)}{info.Rate}")},
                            new AdaptiveText
                            {
                                Text = string.Format(
                                    (alert.TargetMode == AlertTargetMode.Above ? "AlertNotification_ContentAbove" : "AlertNotification_ContentBelow").GetLocalized(),
                                    alert.FromCode, $"{Currency.CurrencySymbol(info.To)}{alert.TargetValue}")
                            }
                        }
                    }
                }
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml()) {Group = NotificationGroupName, Tag = alert.Id.ToString()};

            return toast;
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
