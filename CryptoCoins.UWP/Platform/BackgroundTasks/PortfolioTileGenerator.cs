using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Notifications;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.ViewModels.Entities;
using CryptoCoins.UWP.Views.Formatter;
using Microsoft.Toolkit.Uwp.Notifications;

namespace CryptoCoins.UWP.Platform.BackgroundTasks
{
    public class PortfolioTileGenerator
    {
        public Task<TileNotification> GeneratePortfolioTile(List<HoldingsSummary> infos, string targetCurrency, CancellationToken token)
        {
            var toCode = targetCurrency;
            var toSymbol = toCode != null ? Currency.CurrencySymbol(toCode) : null;
            var value = infos.Aggregate(0d, (d, info) => d + info.Value);

            var changeValue = infos.Aggregate(0d, (d, info) => d + (double)info.Amount* (double.IsNaN(info.Change) ? 0d : info.Change));
            var changePercent = changeValue / (value - changeValue);

            var tileContent = new TileContent
            {
                Visual = new TileVisual
                {
                    TileMedium = MediumPortfolioTile(toCode, toSymbol, value, changeValue, changePercent),
                    TileWide = WidePortfolioTile(toCode, toSymbol, value, changeValue, changePercent)
                }
            };
            if (AnalyticsInfo.VersionInfo.DeviceFamily != "Windows.Mobile")
            {
                //var min = infos.Aggregate(0d, (d, info) => d + info.Holdings.Amount * (double.IsNaN(info.ConversionInfo.Min) ? 0d : info.ConversionInfo.Min));
                //var max = infos.Aggregate(0d, (d, info) => d + info.Holdings.Amount * (double.IsNaN(info.ConversionInfo.Max) ? 0d : info.ConversionInfo.Max));
                //tileContent.Visual.TileLarge = LargePortfolioTile(toCode, toSymbol, value, changeValue, changePercent);
            }
            return Task.FromResult(new TileNotification(tileContent.GetXml()));
        }

        private TileBinding LargePortfolioTile(string toCode, string toSymbol, double value, double changeValue, double changePercent)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 70,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"Portfolio - {toCode}",
                                            HintStyle = AdaptiveTextStyle.Base
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 30,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = "24H",
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        },
                        new AdaptiveText
                        {
                            Text = $"{toSymbol} {Value.FormatNumber(value, 10 - toSymbol.Length - 1, CurrencyHelper.IsFiatCurrency(toCode) ? 2 : int.MaxValue)}",
                            HintStyle = AdaptiveTextStyle.Base
                        },
                        new AdaptiveText
                        {
                            Text = $"{toSymbol}{changeValue} {SignSymbol(changePercent)} {changePercent:P2}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                        /*new AdaptiveText()
                        {
                            Text = string.Empty
                        },
                        new AdaptiveText()
                        {
                            Text = $"Min {toSymbol} {min}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        },
                        new AdaptiveText()
                        {
                            Text = $"Max {toSymbol} {max}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }*/
                    }
                }
            };
        }


        private TileBinding WidePortfolioTile(string toCode, string toSymbol, double value, double changeValue, double changePercent)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 70,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = $"Portfolio - {toCode}",
                                            HintStyle = AdaptiveTextStyle.Base
                                        }
                                    }
                                },
                                new AdaptiveSubgroup
                                {
                                    HintWeight = 30,
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = "24H",
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                            HintAlign = AdaptiveTextAlign.Right
                                        }
                                    }
                                }
                            }
                        },
                        new AdaptiveText
                        {
                            Text = $"{toSymbol} {Value.FormatNumber(value, 10 - toSymbol.Length - 1, CurrencyHelper.IsFiatCurrency(toCode) ? 2 : int.MaxValue)}",
                            HintStyle = AdaptiveTextStyle.Base
                        },
                        new AdaptiveText
                        {
                            Text = $"{toSymbol}{Value.FormatNumber(changeValue,int.MaxValue, CurrencyHelper.IsFiatCurrency(toCode) ? 2 : int.MaxValue)} {SignSymbol(changePercent)} {changePercent:P2}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    }
                }
            };
        }

        private TileBinding MediumPortfolioTile(string toCode, string toSymbol, double value, double changeValue, double changePercent)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveText
                        {
                            Text = $"Portfolio {toCode}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        },
                        new AdaptiveText
                        {
                            Text = $"{toSymbol} {Value.FormatNumber(value, 10 - toSymbol.Length - 1, CurrencyHelper.IsFiatCurrency(toCode) ? 2 : int.MaxValue)}",
                            HintStyle = AdaptiveTextStyle.Body
                        },
                        new AdaptiveText
                        {
                            Text = $"{SignSymbol(changePercent)} {changePercent:P2}",
                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                        }
                    }
                }
            };
        }


        private static string SignSymbol(double value)
        {
            return value > 0 ? "\u25B2" : (value == 0d ? "" : "\u25BC");
        }
    }
}
