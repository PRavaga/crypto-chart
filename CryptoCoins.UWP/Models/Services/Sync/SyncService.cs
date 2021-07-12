using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using CryptoCoins.UWP.Models.Exceptions.Sync;
using CryptoCoins.UWP.Models.UserPreferences;
using MetroLog;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.Toolkit.Uwp.Services.OneDrive;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace CryptoCoins.UWP.Models.Services.Sync
{
    public class SyncService
    {
        public const string SyncFolder = "SharedData";
        public const string SyncStatusFilename = "SyncStatus";
        public const string SyncedModificationDateFilename = "SyncedModificationDate";
        public const string KnownRemoteModificationDateFilename = "KnownRemoteModificationDate";
        public const string LastSucceedSyncDateFilename = "LastSyncDate";
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SyncService>();
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly SqLiteConnectionProvider _connectionProvider;
        private bool _isSignedIn;

        public SyncService(SqLiteConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public DateTimeOffset? LastSyncDate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LastSucceedSyncDateFilename, out var v))
                {
                    return (DateTimeOffset?) v;
                }

                return default(DateTimeOffset?);
            }
            private set => ApplicationData.Current.LocalSettings.Values[LastSucceedSyncDateFilename] = value;
        }

        public DateTimeOffset? SyncedLocalModificationDate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(SyncedModificationDateFilename, out var v))
                {
                    return (DateTimeOffset?) v;
                }

                return default(DateTimeOffset?);
            }
            private set => ApplicationData.Current.LocalSettings.Values[SyncedModificationDateFilename] = value;
        }

        public DateTimeOffset? KnownRemoteModificationDate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(KnownRemoteModificationDateFilename, out var v))
                {
                    return (DateTimeOffset?) v;
                }

                return default(DateTimeOffset?);
            }
            private set => ApplicationData.Current.LocalSettings.Values[KnownRemoteModificationDateFilename] = value;
        }

        public async Task<bool> IsLocalModified()
        {
            var dbModification = await LocalDbModificationDate();
            var lastSync = SyncedLocalModificationDate;
            return dbModification > lastSync;
        }

        private void LoadStatus()
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            if (settings.TryGetValue(LastSucceedSyncDateFilename, out var v))
            {
                LastSyncDate = (DateTimeOffset?) v;
            }

            if (settings.TryGetValue(SyncedModificationDateFilename, out v))
            {
                SyncedLocalModificationDate = (DateTimeOffset?) v;
            }
        }

        public async Task SignOut()
        {
            try
            {
                await OneDriveService.Instance.LogoutAsync();
                _isSignedIn = false;
            }
            catch (ServiceException e)
            {
                throw new SyncException("Can't logout");
            }
        }

        public async Task SignIn(bool silentLogin = false)
        {
            try
            {
                OneDriveService.Instance.Initialize(OneDriveScopes.AppFolder | OneDriveScopes.OfflineAccess, silentLogin?OnlineIdAuthenticationProvider.PromptType.DoNotPrompt:OnlineIdAuthenticationProvider.PromptType.PromptIfNeeded);
                if (!await OneDriveService.Instance.LoginAsync())
                {
                    throw new SyncException("Can't login");
                }

                _isSignedIn = true;
            }
            catch (ServiceException e)
            {
                throw new SyncException("Can't login", e);
            }
            catch (Exception e)
            {
                throw new SyncException("Can't login", e);
            }
        }

        public async Task Sync(bool silentLogin = false)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_isSignedIn)
                {
                    await SignIn(silentLogin);
                }

                try
                {
                    var syncFolder = await OpenSyncFolder();
                    var remoteStatus = await LoadRemoteStatus(syncFolder);
                    var syncedLocalModificationDate = SyncedLocalModificationDate;
                    var knownRemoteModificationDate = KnownRemoteModificationDate;

                    if (remoteStatus != null)
                    {
                        if (!knownRemoteModificationDate.HasValue || remoteStatus.SyncDate != knownRemoteModificationDate)
                        {
                            Log.Info("Loading db from OneDrive, known remote modification date: {0}, current remote modification date: {1}", knownRemoteModificationDate,
                                remoteStatus.SyncDate);
                            await Load(syncFolder, remoteStatus);
                        }
                        else
                        {
                            var localModificationDate = await LocalDbModificationDate();
                            if (localModificationDate > syncedLocalModificationDate)
                            {
                                Log.Info("Saving db to OneDrive, last known modification date: {0}, current modification date: {1}", syncedLocalModificationDate,
                                    localModificationDate);
                                await Save();
                            }
                            else
                            {
                                Log.Info("Db is already in sync, last known modification date: {0}, current modification date: {1}", syncedLocalModificationDate,
                                    localModificationDate);
                            }
                        }
                    }
                    else
                    {
                        Log.Info("Saving db to OneDrive for the first time");
                        await Save();
                    }

                    LastSyncDate = DateTimeOffset.Now;
                }
                catch (ServiceException e)
                {
                    Log.Error("Failed to sync", e);
                    throw new SyncException("Can't sync", e);
                }
                catch (Exception e)
                {
                    Log.Error("Failed to sync", e);
                    throw new SyncException("Can't sync", e);
                }
            }
        }

        private async Task Load(OneDriveStorageFolder syncFolder, SyncStatus status)
        {
            var sharedDbFile = await syncFolder.GetFileAsync(SqLiteConnectionProvider.DatabasePath);
            using (var remoteStream = await sharedDbFile.OpenAsync())
            {
                var buffer = new byte[remoteStream.Size];
                var localBuffer = await remoteStream.ReadAsync(buffer.AsBuffer(), (uint) remoteStream.Size, InputStreamOptions.ReadAhead);
                await _connectionProvider.ReplaceBd(localBuffer);
            }

            SyncedLocalModificationDate = await LocalDbModificationDate();
            KnownRemoteModificationDate = status.SyncDate;
            Log.Info("Loaded db from OneDrive, modification date: {0}", SyncedLocalModificationDate);
        }

        private async Task<StorageFile> OpenUserPreferencesDb()
        {
            return await ApplicationData.Current.LocalFolder.GetFileAsync(SqLiteConnectionProvider.DatabasePath);
        }

        private async Task<DateTimeOffset> LocalDbModificationDate()
        {
            var dbFile = await OpenUserPreferencesDb();
            var baseProps = await dbFile.GetBasicPropertiesAsync();
            return baseProps.DateModified;
        }

        private async Task<OneDriveStorageFolder> OpenSyncFolder()
        {
            var appFolder = await OneDriveService.Instance.AppRootFolderAsync().ConfigureAwait(false);
            return await appFolder.CreateFolderAsync(SyncFolder, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
        }

        private async Task<OneDriveStorageFile> SaveChanges(OneDriveStorageFolder syncFolder)
        {
            var dbFile = await OpenUserPreferencesDb().ConfigureAwait(false);
            using (var localStream = await dbFile.OpenReadAsync())
            {
                return await syncFolder.CreateFileAsync(dbFile.Name, CreationCollisionOption.ReplaceExisting, localStream).ConfigureAwait(false);
            }
        }

        private async Task Save()
        {
            var localModificationDate = await LocalDbModificationDate();

            var syncFolder = await OpenSyncFolder();
            await SaveChanges(syncFolder);
            await SaveRemoteStatus(syncFolder, new SyncStatus {SyncDate = localModificationDate});
            SyncedLocalModificationDate = localModificationDate;
            KnownRemoteModificationDate = localModificationDate;
            Log.Info("Saved db to OneDrive, local: {0}", localModificationDate);
        }

        private async Task<SyncStatus> LoadRemoteStatus(OneDriveStorageFolder syncFolder)
        {
            try
            {
                var syncStatusFile = await syncFolder.GetFileAsync(SyncStatusFilename);
                using (var stream = await syncStatusFile.OpenAsync())
                using (var streamReader = new StreamReader(stream.AsStream()))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    var status = serializer.Deserialize<SyncStatus>(jsonReader);
                    return status;
                }
            }
            catch (ServiceException)
            {
                return null;
            }
        }

        private async Task SaveRemoteStatus(OneDriveStorageFolder syncFolder, SyncStatus status)
        {
            var jsonText = JsonConvert.SerializeObject(status);
            var bytes = Encoding.ASCII.GetBytes(jsonText);
            using (var stream = new MemoryStream(bytes))

            using (var isd = stream.AsRandomAccessStream())
            {
                await syncFolder.CreateFileAsync(SyncStatusFilename, CreationCollisionOption.ReplaceExisting, isd);
            }
        }
    }
}
