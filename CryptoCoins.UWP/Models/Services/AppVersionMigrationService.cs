using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services
{
    public class AppVersionMigrationService
    {
        public const string VersionKey = "Version";

        public void MigrateIfNeeded()
        {
            var version = Package.Current.Id.Version;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(VersionKey, out object value) && value is string json)
            {
                try
                {
                    var storedVersion = JsonConvert.DeserializeObject<PackageVersion>(json);
                    if (!version.Equals(storedVersion))
                    {
                        Migrate();
                        SetStoredVersion(version);
                    }
                }
                catch (JsonException)
                {
                    SetStoredVersion(version);
                }
            }
            else
            {
                SetStoredVersion(version);
            }
        }

        private void Migrate()
        {
            BackgroundExecutionManager.RemoveAccess();
        }

        private void SetStoredVersion(PackageVersion version)
        {
            var settings = ApplicationData.Current.LocalSettings.Values;
            var json = JsonConvert.SerializeObject(version);
            if (settings.ContainsKey(VersionKey))
            {
                settings[VersionKey] = json;
            }
            else
            {
                settings.Add(VersionKey, json);
            }
        }
    }
}
