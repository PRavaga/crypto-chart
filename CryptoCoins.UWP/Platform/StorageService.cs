using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using MetroLog;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Platform
{
    public class StorageService
    {
        public const string DataFolder = "Data";
        private readonly HashAlgorithm _hashAlgorithm = SHA256.Create();
        protected readonly ILogger Logger = LogManagerFactory.DefaultLogManager.GetLogger<StorageService>();

        public async Task<bool> SaveCached<T>(T data, TimeSpan cacheDuration, string filename)
        {
            var cacheInfoFilename = filename + "_Cache";
            await Save(DateTime.Now + cacheDuration, cacheInfoFilename).ConfigureAwait(false);
            return await Save(data).ConfigureAwait(false);
        }

        public async Task<bool> SaveCached<T>(T data, TimeSpan cacheDuration)
        {
            var filename = GetSafeFilename<T>();
            return await SaveCached(data, cacheDuration, filename).ConfigureAwait(false);
        }

        public async Task<T> LoadCached<T>(string filename)
        {
            var cacheInfoFilename = filename + "_Cache";
            var expDate = await Load<DateTime>(cacheInfoFilename).ConfigureAwait(false);
            if (DateTime.Now <= expDate)
            {
                var result = await Load<T>().ConfigureAwait(false);
                return result;
            }

            return default(T);
        }

        public async Task<T> LoadCached<T>()
        {
            var filename = GetSafeFilename<T>();
            return await LoadCached<T>(filename).ConfigureAwait(false);
        }

        public Task<bool> Save<T>(T data)
        {
            return Save(data, out var _);
        }

        public Task<bool> Save<T>(T data, out string filename)
        {
            filename = GetSafeFilename<T>();
            return Save(data, filename);
        }

        public async Task<bool> Save<T>(T data, string filename, CancellationToken token = default(CancellationToken))
        {
            try
            {
                var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(DataFolder, CreationCollisionOption.OpenIfExists).AsTask(token).ConfigureAwait(false);
                var json = JsonConvert.SerializeObject(data);
                await folder.WriteTextToFileAsync(json, filename, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
                return true;
            }
            catch (JsonException e)
            {
                Logger.Error("Failed to serialize data", e);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to save data", e);
            }

            return false;
        }

        public Task<T> Load<T>(T def = default(T))
        {
            return Load<T>(GetSafeFilename<T>());
        }

        public string GetSafeFilename<T>()
        {
            var hash = _hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(typeof(T).FullName));
            return hash.Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2")), sb => sb.ToString());
        }

        public async Task<T> Load<T>(string filename, T def = default(T))
        {
            StorageFile file = null;
            var result = def;
            try
            {
                var folderItem = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(DataFolder).AsTask().ConfigureAwait(false);
                if (folderItem is StorageFolder folder)
                {
                    var fileItem = await folder.TryGetItemAsync(filename).AsTask().ConfigureAwait(false);
                    file = fileItem as StorageFile;
                    if (file != null)
                    {
                        var json = await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
                        result = JsonConvert.DeserializeObject<T>(json);
                        return result;
                    }
                }
                else
                {
                    Logger.Warn($"No data loaded from file '{filename}' because folder '{DataFolder}' doesn't exist");
                }
            }
            catch (JsonException e)
            {
                Logger.Error("Failed to deserialize data", e);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load data", e);
            }
            return result;
        }
    }
}
