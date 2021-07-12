using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.StorageEntities;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.ViewModels;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class UserPreferencesService : Observable
    {
        private const uint MigrationInitial = 0;
        private const uint MigrationTransactions = 1;

        public static readonly string[] OrderedCurrencies =
        {
            "BTC", "ETH", "USD", "EUR", "AUD", "CAD", "GBP", "CHF", "PLN", "NOK", "CNY", "KRW", "JPY", "SGD", "ZAR", "RUB", "INR", "BRL", "TRY", "THB", "PHP"
        };

        public static readonly string[] FiatCurrencies =
        {
            "USD", "EUR", "AUD", "CAD", "GBP", "CHF", "PLN", "NOK", "CNY", "KRW", "JPY", "SGD", "ZAR", "RUB", "INR", "BRL", "TRY", "THB", "PHP"
        };

        private readonly Lazy<Task> _initTask;

        private readonly StorageService _storageService;
        private ObservableCollection<ConversionPreference> _conversionPreferences;
        private ObservableCollection<CryptoCurrencyPreference> _cryptoCurrenciesPreferences;
        private ObservableCollection<CurrencyPreference> _currencyPreferences;
        private DisplayPreference _displayPreference;
        private FeaturedPreference _featuredPreference;
        private readonly SqLiteConnectionProvider _sqLiteConnectionProvider;

        public event Action<PreferencesUpdatedEventArg> PreferencesUpdated;

        public UserPreferencesService(StorageService storageService, SqLiteConnectionProvider connectionProvider)
        {
            _storageService = storageService;
            _sqLiteConnectionProvider = connectionProvider;
            _initTask = new Lazy<Task>(InitInternalAsync, LazyThreadSafetyMode.ExecutionAndPublication);

            connectionProvider.ConnectionReopened += OnConnectionReopened;
        }

        private async void OnConnectionReopened()
        {
            await InitInternalAsync();
            PreferencesUpdated?.Invoke(new PreferencesUpdatedEventArg(UpdateAction.Reset));
        }

        public ObservableCollection<CryptoCurrencyPreference> CryptoCurrenciesPreferences
        {
            get => _cryptoCurrenciesPreferences;
            private set => Set(ref _cryptoCurrenciesPreferences, value);
        }

        public ObservableCollection<CurrencyPreference> CurrencyPreferences
        {
            get => _currencyPreferences;
            set => Set(ref _currencyPreferences, value);
        }

        public ObservableCollection<ConversionPreference> ConversionPreferences
        {
            get => _conversionPreferences;
            set => Set(ref _conversionPreferences, value);
        }

        public FeaturedPreference FeaturedPreference
        {
            get => _featuredPreference;
            set => Set(ref _featuredPreference, value);
        }

        public DisplayPreference DisplayPreference
        {
            get => _displayPreference;
            set => Set(ref _displayPreference, value);
        }

        public Task InitAsync()
        {
            return _initTask.Value;
        }

        public bool IsInvestmentCurrency(string currencyCode)
        {
            return FiatCurrencies.Contains(currencyCode);
        }

        private async Task InitInternalAsync()
        {
            await EnsureMigrated().ConfigureAwait(false);
            await _sqLiteConnectionProvider.Execute((c) =>c.RunInTransactionAsync(Init)).ConfigureAwait(false);
            CryptoCurrenciesPreferences = new ObservableCollection<CryptoCurrencyPreference>(await LoadCryptoPreferences().ConfigureAwait(false));
            CurrencyPreferences = new ObservableCollection<CurrencyPreference>(await LoadCurrencyPreferences().ConfigureAwait(false));
            ConversionPreferences = new ObservableCollection<ConversionPreference>(await LoadConversionPreferences().ConfigureAwait(false));
            await SyncConversionPreferences().ConfigureAwait(false);
            FeaturedPreference = await LoadFeaturedPreference().ConfigureAwait(false);
            DisplayPreference = await LoadDisplayPreference().ConfigureAwait(false);
        }

        public static bool TableExists<T>(SQLiteConnection connection)
        {
            const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
            var cmd = connection.CreateCommand(cmdText, typeof(T).Name);
            return cmd.ExecuteScalar<string>() != null;
        }


        private async Task EnsureMigrated()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            var currentVersion = await connection.ExecuteScalarAsync<uint>("pragma user_version;").ConfigureAwait(false);
            var targetVersion = MigrationTransactions;
            await Migrate(currentVersion, targetVersion);
            if (targetVersion > currentVersion)
            {
                await connection.ExecuteAsync($"pragma user_version = {targetVersion};").ConfigureAwait(false);
            }
        }

        private async Task Migrate(uint currentVersion, uint targetVersion)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            if (currentVersion <= MigrationInitial)
            {
                if (targetVersion >= MigrationTransactions)
                {
                    var holdingsTable = await connection.ExecuteScalarAsync<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='CurrencyHoldings'");
                    if (holdingsTable != null)
                    {
                        var holdings = await connection.Table<CurrencyHoldings>().ToListAsync().ConfigureAwait(false);
                        await connection.RunInTransactionAsync(c =>
                        {
                            c.CreateTable<HoldingsTransaction>();
                            c.CreateTable<HoldingsSummary>();
                            var transactions = holdings.Select(h =>
                            {
                                var amount = decimal.MaxValue;
                                try
                                {
                                    amount = Convert.ToDecimal(h.Amount);
                                }
                                catch (OverflowException)
                                {
                                }

                                return new HoldingsTransaction(h.Currency, amount, DateTime.Now.Date, null);
                            }).ToList();
                            var summaries = transactions.Select(transaction => new HoldingsSummary()
                            {
                                Amount = transaction.Amount,
                                Currency = transaction.BaseCode
                            });
                            c.InsertAll(transactions);
                            c.InsertAll(summaries);
                            c.DropTable<CurrencyHoldings>();
                        }).ConfigureAwait(false);
                    }
                }
            }
        }

        private static void Init(SQLiteConnection connection)
        {
            var exists = TableExists<ConversionPreference>(connection);
            connection.CreateTable(typeof(ConversionPreference));
            if (!exists)
            {
                connection.Insert(new ConversionPreference {From = "BTC", To = "USD", DisplayOrder = 0, FeaturedDisplayOrder = 0, IsFeatured = true});
                connection.Insert(new ConversionPreference {From = "ETH", To = "USD", DisplayOrder = 1, FeaturedDisplayOrder = 1, IsFeatured = true});
            }

            exists = TableExists<CryptoCurrencyPreference>(connection);
            connection.CreateTable(typeof(CryptoCurrencyPreference));
            if (!exists)
            {
                connection.Insert(new CryptoCurrencyPreference {Code = "BTC", IsShown = true});
                connection.Insert(new CryptoCurrencyPreference {Code = "ETH", IsShown = true});
                connection.Insert(new CryptoCurrencyPreference {Code = "LTC", IsShown = true});
            }

            exists = TableExists<CurrencyPreference>(connection);
            connection.CreateTable(typeof(CurrencyPreference));
            if (!exists)
            {
                connection.Insert(new CurrencyPreference {Code = "USD", IsShown = true});
                connection.Insert(new CurrencyPreference {Code = "ETH", IsShown = true});
                connection.Insert(new CurrencyPreference {Code = "BTC", IsShown = true});
                connection.Insert(new CurrencyPreference {Code = "EUR", IsShown = false});
                connection.Insert(new CurrencyPreference {Code = "CNY", IsShown = false});
                connection.Insert(new CurrencyPreference {Code = "KRW", IsShown = false});
                connection.Insert(new CurrencyPreference {Code = "RUB", IsShown = false});
            }

            connection.CreateTable<HoldingsTransaction>();
            connection.CreateTable<HoldingsSummary>();
            connection.CreateTable<AlertModel>();
        }

        private async Task<List<CryptoCurrencyPreference>> LoadCryptoPreferences()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            var result = await connection.Table<CryptoCurrencyPreference>().ToListAsync().ConfigureAwait(false);
            return result;
        }

        private async Task<List<CurrencyPreference>> LoadCurrencyPreferences()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            var currencyPrefs = await connection.Table<CurrencyPreference>().ToListAsync().ConfigureAwait(false);
            var saved = currencyPrefs.ToDictionary(preference => preference.Code);
            return OrderedCurrencies.Select(s =>
            {
                if (saved.TryGetValue(s, out var v))
                {
                    return v;
                }

                return new CurrencyPreference {Code = s, IsShown = false};
            }).ToList();
        }

        private async Task<List<ConversionPreference>> LoadConversionPreferences()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.Table<ConversionPreference>().ToListAsync().ConfigureAwait(false);
        }

        private async Task SyncConversionPreferences()
        {
            var prefs = ConversionPreferences;
            var order = prefs.Max(pref => pref.DisplayOrder);
            var featuredOrder = prefs.Max(pref => pref.FeaturedDisplayOrder);

            var missingPrefs = new List<ConversionPreference>();
            var existingPrefMap = prefs.AddToDictionary(pref => pref.From + "_" + pref.To);
            foreach (var cryptoPref in CryptoCurrenciesPreferences.OrderByDescending(pref => pref.Code))
            {
                var from = cryptoPref.Code;
                foreach (var currPref in CurrencyPreferences.OrderByDescending(pref => pref.Code))
                {
                    var to = currPref.Code;
                    if (!existingPrefMap.ContainsKey(from + "_" + to))
                    {
                        missingPrefs.Add(new ConversionPreference
                        {
                            From = from,
                            To = to,
                            DisplayOrder = ++order,
                            FeaturedDisplayOrder = ++featuredOrder,
                            IsFeatured = false
                        });
                    }
                }
            }

            foreach (var pref in missingPrefs)
            {
                prefs.Add(pref);
            }
            
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            await connection.InsertAllAsync(missingPrefs).ConfigureAwait(false);
        }

        public async Task<bool> ModifyCryptoPreference(CryptoCurrencyPreference pref)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            if (CryptoCurrenciesPreferences.Contains(pref))
            {
                return await connection.UpdateAsync(pref).ConfigureAwait(false) > 0;
            }

            CryptoCurrenciesPreferences.Add(pref);
            await SyncConversionPreferences().ConfigureAwait(false);
            return await connection.InsertAsync(pref).ConfigureAwait(false) > 0;
        }

        public async Task<bool> ModifyCurrencyPreference(CurrencyPreference pref)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            if (CurrencyPreferences.Contains(pref))
            {
                return await connection.UpdateAsync(pref).ConfigureAwait(false) > 0;
            }

            CurrencyPreferences.Add(pref);
            await SyncConversionPreferences().ConfigureAwait(false);
            return await connection.InsertAsync(pref).ConfigureAwait(false) > 0;
        }

        public async Task<bool> UpdateConversionPreference(IEnumerable<ConversionPreference> pref)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.UpdateAllAsync(pref).ConfigureAwait(false) > 0;
        }

        public async Task<bool> UpdateConversionPreference(ConversionPreference pref)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.UpdateAsync(pref).ConfigureAwait(false) > 0;
        }


        public async Task<bool> UpdateCurrencyPreference(CurrencyPreference pref)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.InsertOrReplaceAsync(pref).ConfigureAwait(false) > 0;
        }

        private async Task<FeaturedPreference> LoadFeaturedPreference()
        {
            var pref = await _storageService.Load<FeaturedPreference>().ConfigureAwait(false);
            return pref ?? new FeaturedPreference
            {
                RangeMinutes = SettingsViewModel.RangeDay,
                Steps = 24
            };
        }

        public Task<bool> UpdateFeaturedPreference(FeaturedPreference featuredPreference)
        {
            FeaturedPreference = featuredPreference;
            return _storageService.Save(featuredPreference);
        }

        private async Task<DisplayPreference> LoadDisplayPreference()
        {
            var pref = await _storageService.Load<DisplayPreference>().ConfigureAwait(false);
            return pref ?? new DisplayPreference
            {
                IsAlertsEnabled = true
            };
        }

        public Task<bool> UpdateDisplayPreference(DisplayPreference displayPreference)
        {
            DisplayPreference = displayPreference;
            return _storageService.Save(displayPreference);
        }

        /*public async Task<HoldingsTransaction> GetHoldings(int id)
        {
            return await connection.Table<HoldingsTransaction>().Where(holdings => holdings.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<List<HoldingsTransaction>> GetHoldings()
        {
            return await connection.Table<HoldingsTransaction>().ToListAsync().ConfigureAwait(false);
        }*/

        

        public async Task<List<AlertModel>> GetAlerts()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.Table<AlertModel>().ToListAsync().ConfigureAwait(false);
        }

        public async Task AddAlert(AlertModel alert)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            await connection.InsertAsync(alert).ConfigureAwait(false);
        }

        public async Task UpdateAlert(AlertModel alert)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            await connection.UpdateAsync(alert).ConfigureAwait(false);
        }

        public async Task DeleteAlert(AlertModel alert)
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            await connection.DeleteAsync(alert).ConfigureAwait(false);
        }

        public async Task<int> AlertsCount()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.Table<AlertModel>().CountAsync();
        }

        public async Task<int> HoldingsCount()
        {
            var connection = await _sqLiteConnectionProvider.GetConnection().ConfigureAwait(false);
            return await connection.Table<HoldingsSummary>().CountAsync();
        }
    }
}
