using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Notifications;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Views.Formatter;
using MetroLog;
using Microsoft.Graphics.Canvas;
using Microsoft.Toolkit.Uwp.Notifications;
using DateTime = System.DateTime;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public class TileGenerator : IDisposable
    {
        private const int WideTileWidth = 310;
        private const int WideTileHeight = 150;
        private const int WideTileOffsetY = 74;
        private const int LargeTileWidth = 310;
        private const int LargeTileHeight = 310;
        private const int LargeTileOffsetY = 120;
        private static readonly Color FillChartColor = Color.FromArgb(102, 255, 255, 255);
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<TileGenerator>();
        private readonly CanvasDevice _device;
        private readonly CanvasRenderTarget _largeCanvas;
        private readonly string _tilesFolderName;
        private readonly CanvasRenderTarget _wideCanvas;
        private IStorageFolder _folder;

        public TileGenerator(float dpi, string tilesFolderName)
        {
            _tilesFolderName = tilesFolderName;
            _device = CanvasDevice.GetSharedDevice();
            _wideCanvas = new CanvasRenderTarget(_device, WideTileWidth, WideTileHeight, dpi);
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                _largeCanvas = new CanvasRenderTarget(_device, LargeTileWidth, LargeTileHeight, dpi);
            }
        }

        public void Dispose()
        {
            _device?.Dispose();
            _wideCanvas?.Dispose();
            _largeCanvas?.Dispose();
        }

        public async Task<TileNotification> GenerateTile(DetailedConversionInfo info, string prefix, CancellationToken token)
        {
            if (_folder == null)
            {
                _folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(_tilesFolderName, CreationCollisionOption.OpenIfExists).AsTask(token);
            }

            using (var drawingSession = _wideCanvas.CreateDrawingSession())
            {
                drawingSession.Antialiasing = CanvasAntialiasing.Antialiased;
                drawingSession.Clear(Colors.Transparent);
                ChartRenderer.RenderData(drawingSession, WideTileWidth, WideTileHeight - WideTileOffsetY, 0, WideTileOffsetY, Colors.White, FillChartColor,
                    2f, info.RateHourlyHistory);
            }

            var wideFilename = $"{prefix}_{IOHelper.ReplaceInvalidFilenameChars(info.From)}_{IOHelper.ReplaceInvalidFilenameChars(info.To)}_wide.png";
            var wideFile = await _folder.CreateFileAsync(wideFilename, CreationCollisionOption.GenerateUniqueName).AsTask(token);
            using (var stream = await wideFile.OpenAsync(FileAccessMode.ReadWrite).AsTask(token))
            {
                await _wideCanvas.SaveAsync(stream, CanvasBitmapFileFormat.Png).AsTask(token);
            }
            await DeleteOutdatedTiles(Path.GetFileNameWithoutExtension(wideFilename), Path.GetFileNameWithoutExtension(wideFile.Name), token);

            var tileContent = new TileContent
            {
                Visual = new TileVisual
                {
                    TileMedium = MediumTile(info),
                    TileWide = WideTile(info, wideFile.Path)
                }
            };

            if (_largeCanvas != null)
            {
                using (var drawingSession = _largeCanvas.CreateDrawingSession())
                {
                    drawingSession.Antialiasing = CanvasAntialiasing.Antialiased;
                    drawingSession.Clear(Colors.Transparent);
                    ChartRenderer.RenderData(drawingSession, LargeTileWidth, LargeTileHeight - LargeTileOffsetY, 0, LargeTileOffsetY, Colors.White, FillChartColor,
                        3f, info.RateHourlyHistory);
                }

                var largeFilename = $"{prefix}_{IOHelper.ReplaceInvalidFilenameChars(info.From)}_{IOHelper.ReplaceInvalidFilenameChars(info.To)}_large.png";
                var largeFile = await _folder.CreateFileAsync(largeFilename, CreationCollisionOption.GenerateUniqueName).AsTask(token);
                using (var stream = await largeFile.OpenAsync(FileAccessMode.ReadWrite).AsTask(token))
                {
                    await _largeCanvas.SaveAsync(stream, CanvasBitmapFileFormat.Png).AsTask(token);
                }
                await DeleteOutdatedTiles(Path.GetFileNameWithoutExtension(largeFilename), Path.GetFileNameWithoutExtension(largeFile.Name), token);


                tileContent.Visual.TileLarge = LargeTile(info, largeFile.Path);
            }

            return new TileNotification(tileContent.GetXml());
        }

        public async Task DeleteOutdatedTiles(string originalFilename, string savedFilename, CancellationToken token)
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(_tilesFolderName, CreationCollisionOption.OpenIfExists).AsTask(token);
            foreach (var file in (await folder.GetFilesAsync().AsTask(token)).Where(file => file.Name.Contains(originalFilename) && !file.Name.Contains(savedFilename)))
            {
                try
                {
                    await file.DeleteAsync().AsTask(token);
                }
                catch (Exception e)
                {
                    Log.Warn($"Failed to delete live tile background {file.Name}", e);
                }
            }
        }

        public async Task DeleteOutdatedTiles(CancellationToken token)
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(_tilesFolderName, CreationCollisionOption.OpenIfExists).AsTask(token);
            foreach (var file in (await folder.GetFilesAsync().AsTask(token)).Where(file => (DateTime.Now - file.DateCreated).TotalDays >= 1d))
            {
                try
                {
                    await file.DeleteAsync().AsTask(token);
                }
                catch (Exception e)
                {
                    Log.Warn($"Failed to delete live tile background {file.Name}", e);
                }
            }
        }


        private static TileBinding MediumTile(ConversionInfo info)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveText
                        {
                            Text = Currency.FormatFromTo(info.From, info.To),
                            HintStyle = AdaptiveTextStyle.Body
                        },
                        new AdaptiveText
                        {
                            Text = Currency.FormatRate(info.Rate, info.To, 10),
                            HintStyle = AdaptiveTextStyle.Body
                        },
                        new AdaptiveText
                        {
                            Text = $"{Currency.SignSymbol(info.Change24)} {info.Change24:P2}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    }
                }
            };
        }

        private static TileBinding WideTile(ConversionInfo info, string chartPath)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    BackgroundImage = new TileBackgroundImage {Source = chartPath, HintOverlay = 0},
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"{info.From} - {info.To}",
                                            HintStyle = AdaptiveTextStyle.Base
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = Currency.FormatRate(info.Rate, info.To, 10),
                                            HintStyle = AdaptiveTextStyle.Base,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        },
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = Currency.FormatVolume24Abbr(info.Volume24, info.From),
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"{Currency.SignSymbol(info.Change24)} {info.Change24:P2}",
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private static TileBinding LargeTile(DetailedConversionInfo info, string chartPath)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    BackgroundImage = new TileBackgroundImage {Source = chartPath, HintOverlay = 0},
                    TextStacking = TileTextStacking.Top,
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"{info.From} - {info.To}",
                                            HintStyle = AdaptiveTextStyle.Base
                                        }
                                    }
                                },

                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = Currency.FormatRate(info.Rate, info.To, 10),
                                            HintStyle = AdaptiveTextStyle.Base,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        },
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = Currency.FormatVolume24Abbr(info.Volume24, info.From),
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 50,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"{Currency.SignSymbol(info.Change24)} {info.Change24:P2}",
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        },
                        new AdaptiveText
                        {
                            Text = ""
                        },
                        new AdaptiveText
                        {
                            Text = $"Min {Currency.FormatRate(info.Min, info.To)}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        },
                        new AdaptiveText
                        {
                            Text = $"Max {Currency.FormatRate(info.Max, info.To)}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    }
                }
            };
        }
    }
}
