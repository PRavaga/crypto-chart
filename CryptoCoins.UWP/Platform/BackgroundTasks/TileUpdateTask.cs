using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System.Diagnostics;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.ViewModels.Converters;
using CryptoCoins.UWP.ViewModels.Entities;
using MetroLog;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public sealed class TileUpdateTask : BackgroundTask
    {
        public enum SecondaryTileType
        {
            Conversion, Portfolio
        }
        private const string DpiSettingsKey = "LiveTileDpi";
        private const string PrimaryTiles = "LiveTileAssets";
        private const string SecondaryTilePrefix = "s";
        private const string PrimaryTilePrefix = "p";
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<TileUpdateTask>();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly CryptoService _cryptoService;
        private readonly UserPreferencesService _prefService;
        private readonly HoldingsService _holdingsService;
        private readonly HoldingsConverter _holdingsConverter;
        public int IsRunning;

        public TileUpdateTask(CryptoService cryptoService, UserPreferencesService prefService, HoldingsService holdingsService, HoldingsConverter holdingsConverter)
        {
            _cryptoService = cryptoService;
            _prefService = prefService;
            _holdingsService = holdingsService;
            _holdingsConverter = holdingsConverter;
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

                // TODO UWPTemplates: Define your trigger here and set your conditions
                // Note conditions are optional
                // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/create-and-register-an-inproc-background-task

                ApplicationData.Current.LocalSettings.Values[DpiSettingsKey] = DisplayInformation.GetForCurrentView().LogicalDpi;
                builder.AddCondition(new SystemCondition(SystemConditionType.SessionConnected));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                builder.SetTrigger(new TimeTrigger(15, false));
                builder.Register();
            }
        }

        public async Task<bool> PinSecondaryPortfolioTile(string toCode)
        {
            if (!IsSecondaryPortfolioTilePinned(toCode))
            {
                var tileId = GetSecondaryPortfolioTileId(toCode);
                var tile = new SecondaryTile(tileId, $"Portfolio {toCode}", tileId, new Uri("ms-appx:///Assets/Square150x150Logo.png"), TileSize.Default);
                tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");
                tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");
                tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                //tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");
                if (await tile.RequestCreateAsync())
                {
                    await UpdatePinnedPortfolioTiles(CancellationToken.None);
                    return true;
                }
            }
            return false;
        }

        private async Task UpdatePinnedPortfolioTiles(CancellationToken cancellationToken)
        {
            var holdings = await _holdingsService.GetHoldings();
            holdings = holdings.Where(summary => !_prefService.IsInvestmentCurrency(summary.Currency)).ToList();
            cancellationToken.ThrowIfCancellationRequested();
            if (holdings.Count > 0)
            {
                var tiles = await SecondaryTile.FindAllAsync().AsTask(cancellationToken);
                var toCodes = tiles.Where(tile => GetSecondaryTileType(tile.TileId) == SecondaryTileType.Portfolio).Select(tile => GetPortfolioToCode(tile.TileId)).ToList();
                if (toCodes.Count > 0)
                {
                    var tileGenerator = new PortfolioTileGenerator();
                    var froms = holdings.Select(h => h.Currency).ToList();
                    try
                    {
                        Log.Info($"Updating portfolio tiles. Holdings count {froms.Count}, target currencies count: {toCodes.Count}");
                        var conversions = await _cryptoService.GetConversions(froms, toCodes, cancellationToken);

                        foreach (var toCode in toCodes)
                        {
                            var filteredConversions = conversions.Where(info => info.To == toCode);
                            var result = holdings.Join(filteredConversions, summary => summary.Currency, info => info.From, (summary, info) =>
                            {
                                var r = _holdingsConverter.Convert(summary);
                                r.UpdateFromCurrentRates(info);
                                r.UpdateChangeFromCurrentRates(info);
                                return r;
                            }).ToList();

                            var tile = await tileGenerator.GeneratePortfolioTile(result, toCode, cancellationToken);
                            var secondaryTileUpdater =
                                TileUpdateManager.CreateTileUpdaterForSecondaryTile(GetSecondaryPortfolioTileId(toCode));
                            secondaryTileUpdater.Clear();
                            secondaryTileUpdater.Update(tile);
                        }
                    }
                    catch (ApiException e)
                    {
                        Log.Error("Couldn't update pinned portfolio tiles. Online service is unavailable", e);
                    }
                }
            }
        }

        public async Task<bool> PinSecondaryTile(DetailedConversionInfo info)
        {
            if (!IsSecondaryTilePinned(info) && Interlocked.CompareExchange(ref IsRunning, 1, 0) == 0)
            {
                try
                {
                    var tileId = GetSecondaryTileId(info);
                    var tile = new SecondaryTile(tileId, $"{info.From} - {info.To}", tileId, new Uri("ms-appx:///Assets/Square150x150Logo.png"), TileSize.Default);
                    tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");
                    tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");
                    tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                    tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");
                    if (await tile.RequestCreateAsync())
                    {
                        await UpdatePinnedTiles(new List<DetailedConversionInfo> {info}, CancellationToken.None);
                        return true;
                    }
                }
                finally
                {
                    IsRunning = 0;
                }
            }
            return false;
        }

        public async Task<bool> UnpinSecondaryTile(ConversionInfo info)
        {
            if (IsSecondaryTilePinned(info))
            {
                var tileId = GetSecondaryTileId(info);
                var tile = new SecondaryTile(tileId);
                return await tile.RequestDeleteAsync();
            }
            return false;
        }

        public async Task<bool> UnpinSecondaryPortfolioTile(string toCode)
        {
            if (IsSecondaryPortfolioTilePinned(toCode))
            {
                var tileId = GetSecondaryPortfolioTileId(toCode);
                var tile = new SecondaryTile(tileId);
                return await tile.RequestDeleteAsync();
            }
            return false;
        }

        public static bool IsSecondaryTilePinned(ConversionInfo info)
        {
            return SecondaryTile.Exists(GetSecondaryTileId(info));
        }

        public static bool IsSecondaryPortfolioTilePinned(string toCode)
        {
            return SecondaryTile.Exists(GetSecondaryPortfolioTileId(toCode));
        }
        public static bool IsSecondaryTilePinned(string from, string to)
        {
            return SecondaryTile.Exists(GetSecondaryTileId(from, to));
        }

        public override async Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var deferral = taskInstance.GetDeferral();
            try
            {
                if (Interlocked.CompareExchange(ref IsRunning, 1, 0) == 0)
                {
                    try
                    {
                        _cancellationTokenSource = new CancellationTokenSource();
                        await UpdateTiles(_cancellationTokenSource.Token);
                        await UpdatePinnedPortfolioTiles(_cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        Log.Info("Tiles update was cancelled", e);
                    }
                    catch (Exception e) when (_cancellationTokenSource.IsCancellationRequested)
                    {
                        //Suppose that task can be cancelled any time from any thread. In such case it's possible that we didn't catch OperationCancelledException.
                        Log.Error("Caught exception after task was cancalled", e);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Skipping exception", e);
                    }
                    finally
                    {
                        IsRunning = 0;
                    }
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        public async Task UpdateTiles(IList<DetailedConversionInfo> featuredConversions, IList<DetailedConversionInfo> pinnedConversions, CancellationToken token)
        {
            Log.Info($"Updating tiles from process: {ProcessDiagnosticInfo.GetForCurrentProcess().ProcessId}");
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
            tileUpdater.Clear();

            var dpi = GetTileDpi();
            var secondaryTiles = await SecondaryTile.FindAllAsync().AsTask(token);
            var secondaryTilesMap = secondaryTiles.Where(tile => GetSecondaryTileType(tile.TileId)==SecondaryTileType.Conversion).ToDictionary(tile => tile.TileId);
            using (var tileGenerator = new TileGenerator(dpi, PrimaryTiles))
            {
                foreach (var info in featuredConversions)
                {
                    var infoId = GetSecondaryTileId(info);
                    var toast = await tileGenerator.GenerateTile(info, PrimaryTilePrefix, token);
                    tileUpdater.Update(toast);
                    if (secondaryTilesMap.ContainsKey(infoId))
                    {
                        toast = await tileGenerator.GenerateTile(info, SecondaryTilePrefix, token);
                        var secondaryTileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(infoId);
                        secondaryTileUpdater.Clear();
                        secondaryTileUpdater.Update(toast);
                    }
                    await UpdatePinnedTiles(pinnedConversions, secondaryTilesMap, tileGenerator, token);
                }
            }
        }

        private static float GetTileDpi()
        {
            float dpi = 96;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(DpiSettingsKey, out var v) && v is float savedDpi)
            {
                dpi = savedDpi;
            }
            return dpi;
        }

        public async Task UpdatePinnedTiles(IList<DetailedConversionInfo> conversions, CancellationToken token)
        {
            var dpi = GetTileDpi();
            var secondaryTiles = await SecondaryTile.FindAllAsync().AsTask(token);
            var secondaryTilesMap = secondaryTiles.Where(tile => GetSecondaryTileType(tile.TileId) == SecondaryTileType.Conversion).ToDictionary(tile => tile.TileId);
            using (var tileGenerator = new TileGenerator(dpi, PrimaryTiles))
            {
                await UpdatePinnedTiles(conversions, secondaryTilesMap, tileGenerator, token);
            }
        }

        private async Task UpdatePinnedTiles(IList<DetailedConversionInfo> conversions, Dictionary<string, SecondaryTile> tileMap, TileGenerator tileGenerator, CancellationToken token)
        {
            foreach (var info in conversions)
            {
                var infoId = GetSecondaryTileId(info);
                if (tileMap.ContainsKey(infoId))
                {
                    var toast = await tileGenerator.GenerateTile(info, SecondaryTilePrefix, token);
                    var secondaryTileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(infoId);
                    secondaryTileUpdater.Clear();
                    secondaryTileUpdater.Update(toast);
                }
            }
        }

        private static string GetSecondaryTileId(ConversionInfo info)
        {
            return GetSecondaryTileId(info.From, info.To);
        }

        private static string GetSecondaryPortfolioTileId(string toCode)
        {
            return $"portfolio_{toCode}";
        }

        public static (string from, string to) GetConversionInfo(string tileId)
        {
            var separatorIndex = tileId.IndexOf("_", StringComparison.Ordinal);
            return (tileId.Substring(0, separatorIndex), tileId.Substring(separatorIndex + 1));
        }

        public static string GetPortfolioToCode(string tileId)
        {
            var separatorIndex = tileId.IndexOf("_", StringComparison.Ordinal);
            return tileId.Substring(separatorIndex + 1);
        }

        private static SecondaryTileType GetSecondaryTileType(string tileId)
        {
            if (tileId.StartsWith("portfolio_"))
            {
                return SecondaryTileType.Portfolio;
            }
            return SecondaryTileType.Conversion;
        }

        private static string GetSecondaryTileId(string from, string to)
        {
            return $"{from}_{to}";
        }

        private async Task UpdateTiles(CancellationToken cancellationToken)
        {
            await _prefService.InitAsync();
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var conversions = await Task.WhenAll(_cryptoService.GetFeaturedConversions(5, cancellationToken));
                var pinnedTiles = (await SecondaryTile.FindAllAsync().AsTask(cancellationToken)).ToArray();
                var pinnedToLoad = await Task.WhenAll(pinnedTiles.Where(tile => GetSecondaryTileType(tile.TileId) == SecondaryTileType.Conversion).Select(tile => GetConversionInfo(tile.TileId))
                    .Where(info => !conversions.Any(conversionInfo => conversionInfo.From == info.from && conversionInfo.To == info.to))
                    .Select(info => _cryptoService.GetDetailedConversion(info.from, info.to, cancellationToken)));
                Log.Info($"Updating conversion tiles. Featured count: {conversions.Length}, pinned count: {pinnedTiles.Length} (need to load: {pinnedToLoad.Length}");
                await UpdateTiles(conversions, pinnedToLoad, cancellationToken);
            }
            catch (ApiException e)
            {
                Log.Error("Failed to load conversions. Aborting", e);
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
