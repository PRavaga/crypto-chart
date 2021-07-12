using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Syndication;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using CryptoCoins.UWP.Models.Services.Entries;
using MetroLog;

namespace CryptoCoins.UWP.Models.Services
{
    public class NewsService
    {
        private static readonly ILogger Logger = LogManagerFactory.DefaultLogManager.GetLogger<NewsService>();
        private static readonly List<NewsSource> StaticNewsSources = new List<NewsSource> {
            new NewsSource("Bitcoin magazine","https://bitcoinmagazine.com/feed/"),
            new NewsSource("Coin telegraph", "https://cointelegraph.com/rss"),
            new NewsSource("Coindesk","https://feeds.feedburner.com/CoinDesk"),
            new NewsSource("Cryptocoins News","https://www.cryptocoinsnews.com/news/feed/"),
            new NewsSource("Newsbtc","http://www.newsbtc.com/feed/"),
            new NewsSource("CryptoCompare","https://www.cryptocompare.com/api/external/newsletter/"),
            new NewsSource("99bitcoins","https://99bitcoins.com/feed/"),
            new NewsSource("Bitcoinist","http://bitcoinist.com/feed/"),
            new NewsSource("Bitcoin.com","https://news.bitcoin.com/feed/"),
            new NewsSource("Ethnews","https://www.ethnews.com/rss.xml"),
            new NewsSource("Livebitcoinnews","http://www.livebitcoinnews.com/feed/"),
            new NewsSource("Coinspeaker","https://feeds.feedburner.com/coinspeaker/"),
            new NewsSource("Trustnodes","http://www.trustnodes.com/feed"),
            new NewsSource("Themerkle","http://themerkle.com/feed/")
        };
        private readonly SyndicationClient _client = new SyndicationClient();
        private List<NewsSource> _sources;

        public NewsService()
        {
            SetNewsSources(StaticNewsSources);
        }

        public List<NewsSource> AvailableNewsSources => StaticNewsSources;

        public void SetNewsSources(List<NewsSource> sources)
        {
            _sources = sources;
        }

        public IEnumerable<Task<List<NewsModel>>> LoadNews()
        {
            foreach (var source in _sources)
            {
                yield return LoadNews(source);
            }
        }

        private async Task<List<NewsModel>> LoadNews(NewsSource source)
        {
            var result = new List<NewsModel>();
            try
            {
                _client.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(source.Url);
                var feed = await _client.RetrieveFeedAsync(uri);
                foreach (var item in feed.Items)
                {
                    var newsModel = new NewsModel
                    {
                        Link = item.Links.FirstOrDefault()?.Uri,
                        Title = item.Title.Text,
                        PublicationTime = item.PublishedDate.LocalDateTime,
                        Source = source.Source,
                        MediaLink = item.Links.FirstOrDefault(link => link.Uri.AbsoluteUri.Contains(".jpg"))?.Uri
                    };
                    if (newsModel.MediaLink == null)
                    {
                        Regex jpegRegex = new Regex(@"<img ([^>]* )?src=[\""\'](?<image>[^\""\']*\.jpe?g)[\""\']", RegexOptions.IgnoreCase);
                        var jpegMatch = jpegRegex.Match(item.Summary.Text);
                        if (jpegMatch.Success && Uri.TryCreate(jpegMatch.Groups["image"].Value, UriKind.Absolute, out var jpegUri))
                        {
                            newsModel.MediaLink = jpegUri;
                        }
                    }
                    result.Add(newsModel);
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"Can't retrieve news feed {source.Source}, {source.Url}", e);
            }
            return result;
        }
    }
}
