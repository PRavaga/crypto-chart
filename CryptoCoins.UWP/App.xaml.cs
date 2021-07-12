using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.System;
using Windows.System.Diagnostics;
using Windows.System.UserProfile;
using Windows.UI.Xaml;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Helpers.Logging;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Views;
using MetroLog;
using MetroLog.Targets;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Push;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using LogLevel = MetroLog.LogLevel;

namespace CryptoCoins.UWP
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private const string HockeyappIdsPath = "HockeyAppIds";
        private static ILogger Logger;
        private static InMemoryLogTarget _inMemoryLogTarget;
        private readonly Lazy<ActivationService> _activationService;
        private readonly Lazy<Task> _appInitializing;
        private string _userId;
        private MetroLogFileTarget _streamingFileTarget;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            ConfigureMobileCenter();
            ConfigureLogging();
            ConfigureMemoryTracking();
            _appInitializing = new Lazy<Task>(InitializeAppAsync);
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            EnteredBackground += App_EnteredBackground;
            LeavingBackground += App_LeavingBackground;
            Suspending += App_Suspending;
            Resuming += App_Resuming;

            //Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        private void ConfigureMemoryTracking()
        {
            MemoryManager.AppMemoryUsageLimitChanging += (sender, args) =>
            {
                Logger.Warn($"AppMemoryUsageLimitChanging from {args.OldLimit} to {args.NewLimit}");
            };
            MemoryManager.AppMemoryUsageIncreased += (sender, o) =>
            {
                Logger.Warn($"AppMemoryUsageIncreased {MemoryManager.AppMemoryUsageLevel}");
            };
            MemoryManager.AppMemoryUsageDecreased += (sender, o) =>
            {
                Logger.Warn($"AppMemoryUsageDecreased {MemoryManager.AppMemoryUsageLevel}");
            };
        }

        private ActivationService ActivationService => _activationService.Value;

        private void ConfigureMobileCenter()
        {
            AppCenter.SetCountryCode(GlobalizationPreferences.HomeGeographicRegion);
            AppCenter.Start("f6bc2df2-54ff-48c9-bbe1-ddf04e27cc1c", typeof(Push), typeof(Analytics));
            /*Crashes.GetErrorAttachments = GetErrorAttachments;
            AppCenter.Start("f6bc2df2-54ff-48c9-bbe1-ddf04e27cc1c", typeof(Push), typeof(Analytics));
            */
        }

        /*private IEnumerable<ErrorAttachmentLog> GetErrorAttachments(ErrorReport report)
        {
            var sessionId = Guid.NewGuid().ToString();
            yield return ErrorAttachmentLog.AttachmentWithText($"HockeyApp user id: {_userId}" + Environment.NewLine +
                                                               $"HockeyApp session id: {sessionId}" + Environment.NewLine +
                                                               MemoryUsage() + Environment.NewLine +
                                                               _inMemoryLogTarget.LogLines.Aggregate(new StringBuilder(), (builder, s) => builder.AppendLine(s),
                                                                   builder => builder.ToString()), "Log");
        }*/

        private void App_Resuming(object sender, object e)
        {
            Logger.Trace("OnResuming");
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            Logger.Trace($"OnSuspending from process: {ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId}");
        }

        private static string MemoryUsage()
        {
            return $"Memory usage {MemoryManager.AppMemoryUsageLevel}: {MemoryManager.AppMemoryUsage:N0} / {MemoryManager.AppMemoryUsageLimit:N0}";
        }

        private async Task ConfigureHockeyAppContacts()
        {
            _userId = null;
            var idsFile = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(HockeyappIdsPath);
            if (idsFile != null && idsFile.IsOfType(StorageItemTypes.File))
            {
                try
                {
                    _userId = await FileIO.ReadTextAsync((StorageFile) idsFile);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to load HockeyApp userId", e);
                }
            }
            if (_userId == null || !Guid.TryParse(_userId, out var _))
            {
                _userId = Guid.NewGuid().ToString();
                try
                {
                    var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(HockeyappIdsPath, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, _userId);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to save HockeyApp userId", e);
                }
            }
            HockeyClient.Current.UpdateContactInfo(_userId, null);
        }

        private void ConfigureLogging()
        {
            _inMemoryLogTarget = new InMemoryLogTarget();
            _streamingFileTarget = new MetroLogFileTarget {FileNamingParameters = {IncludeSession = true}};
            var logConfig = new LoggingConfiguration {IsEnabled = true};
            logConfig.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
            logConfig.AddTarget(LogLevel.Trace, LogLevel.Fatal, _streamingFileTarget);
            logConfig.AddTarget(LogLevel.Trace, LogLevel.Fatal, _inMemoryLogTarget);
            LogManagerFactory.DefaultConfiguration = logConfig;
            Logger = LogManagerFactory.DefaultLogManager.GetLogger<App>();

            var sessionId = Guid.NewGuid().ToString();
            HockeyClient.Current.Configure("9e52094407a44f2f96a2e063fd9847cb")
                .SetExceptionDescriptionLoader(exception => exception.ExpandException() + Environment.NewLine +
                                                            $"HockeyApp user id: {_userId}" + Environment.NewLine +
                                                            $"HockeyApp session id: {sessionId}" + Environment.NewLine +
                                                            MemoryUsage() + Environment.NewLine +
                                                            _inMemoryLogTarget.LogLines.Aggregate(new StringBuilder(), (builder, s) => builder.AppendLine(s),
                                                                builder => builder.ToString()));
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Logger.Trace(
                $"OnLaunched from {e.PreviousExecutionState} state, processId: {ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId}, thread id: {Environment.CurrentManagedThreadId}");
            if (!e.PrelaunchActivated)
            {
                await _appInitializing.Value;
                await ActivationService.ActivateAsync(e);
            }
        }


        private async Task InitializeAppAsync()
        {
            await ConfigureHockeyAppContacts();
            await LoadLogs();
        }

        private async Task LoadLogs()
        {
            try
            {
                var logFolder = await ApplicationData.Current.LocalFolder.TryGetItemAsync("MetroLogs");
                if (logFolder != null && logFolder.IsOfType(StorageItemTypes.Folder))
                {
                    var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, null);
                    queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.None,
                        new[] { "System.DateCreated" });

                    var queryResults = ((StorageFolder)logFolder).CreateFileQueryWithOptions(queryOptions);
                    var logFiles = await queryResults.GetFilesAsync();

                    var currentLogFilename = _streamingFileTarget.FileNamingParameters.GetFilename(new LogWriteContext(),
                        new LogEventInfo(LogLevel.Debug, string.Empty, string.Empty, null));
                    var logFile = logFiles.OrderBy(file => file.DateCreated).LastOrDefault(file => file.Name != currentLogFilename);
                    if (logFile != null)
                    {
                        using (var readStream = await logFile.OpenSequentialReadAsync())
                        using (var netStream = readStream.AsStreamForRead())
                        using (var reader = new StreamReader(netStream))
                        {
                            _inMemoryLogTarget.Write("<<<<< Start of the previous log >>>>>");

                            string line;
                            try
                            {
                                while ((line = await reader.ReadLineAsync()) != null)
                                {
                                    _inMemoryLogTarget.Write(line);
                                }
                            }
                            catch (IOException e)
                            {
                                Logger.Error("Failed to read previous log", e);
                            }
                            _inMemoryLogTarget.Write("<<<<< End of the previous log >>>>>");
                        }
                    }
                    else
                    {
                        Logger.Info("There are no previous log files");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to load previous log file to memory log", e);
            }
        }

        /// <summary>
        ///     Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        /// <param name="args">Event data for the event.</param>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            Logger.Trace($"OnActivated {args.Kind} from {args.PreviousExecutionState}, processId: {ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId}");
            await ActivationService.ActivateAsync(args);
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Logger.Trace("OnEnteredBackground");
            var deferral = e.GetDeferral();
            try
            {
                await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Logger.Trace("OnLeavingBackground");
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            Logger.Trace(
                $"OnBackgroundActivated {args.TaskInstance.Task.Name}, process id: {ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId}, thread id: {Environment.CurrentManagedThreadId}");
            var deferral = args.TaskInstance.GetDeferral();
            try
            {
                await _appInitializing.Value;
                await ActivationService.ActivateAsync(args);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(DashboardPage));
        }
    }
}
