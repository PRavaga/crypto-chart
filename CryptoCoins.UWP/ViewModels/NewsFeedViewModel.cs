using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Models.Services.Entries.Compare;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.Collection;
using CryptoCoins.UWP.ViewModels.Common;
using CryptoCoins.UWP.Views;
using Nito.AsyncEx;
using Nito.Mvvm;

namespace CryptoCoins.UWP.ViewModels
{
    public class NewsFeedViewModel : ViewModelBase
    {
        public const string NewsSourcesPrefFilename = "NewsPrefs";

        private readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1);
        private readonly NavigationService _navigationService;
        private readonly NewsService _newsService;
        private DataState _dataState;
        private FilterCollection<NewsModel> _news;
        private List<NewsSource> _newsSources;
        private RelayCommand<NewsModel> _openNewsCommand;
        private AsyncCommand _refreshNewsCommand;
        private CancellationTokenSource _saveNewsSourceTokenSource = new CancellationTokenSource();
        private RelayCommand<NewsModel> _shareCommand;
        private readonly StorageService _storageService;

        public NewsFeedViewModel(NewsService newsService, NavigationService navigationService, StorageService storageService)
        {
            _newsService = newsService;
            _navigationService = navigationService;
            _storageService = storageService;
            News = new FilterCollection<NewsModel>(new LambdaComparer<NewsModel>((x, y) =>
                -x.PublicationTime.CompareTo(y.PublicationTime)));
        }

        public DataState DataState
        {
            get => _dataState;
            set => Set(ref _dataState, value);
        }

        public List<NewsSource> NewsSources
        {
            get => _newsSources;
            set => Set(ref _newsSources, value);
        }

        public AsyncCommand RefreshNewsCommand => _refreshNewsCommand ?? (_refreshNewsCommand = new AsyncCommand(async o => { await RefreshNews(); }));

        public RelayCommand<NewsModel> OpenNewsCommand =>
            _openNewsCommand ?? (_openNewsCommand = new RelayCommand<NewsModel>(e =>
            {
                _navigationService.Navigate<NewsWebPage>(
                    new NewsWebViewModel.NavigationParameter {Uri = e.Link.AbsoluteUri, Title = e.Title});
            }));

        public ProgressState ProgressState { get; } = new ProgressState();

        public FilterCollection<NewsModel> News
        {
            get => _news;
            set => Set(ref _news, value);
        }

        public RelayCommand<NewsModel> ShareCommand => _shareCommand ?? (_shareCommand = new RelayCommand<NewsModel>(m =>
        {
            var shareManager = DataTransferManager.GetForCurrentView();
            shareManager.DataRequested += (sender, args) =>
            {
                args.Request.Data.Properties.Title = string.Format("NewsWebPage_ShareTitle".GetLocalized(), m.Title);
                args.Request.Data.Properties.Description = "NewsWebPage_ShareDescription".GetLocalized();
                args.Request.Data.SetText(string.Format("NewsWebPage_ShareText".GetLocalized(), m.Title));
                args.Request.Data.SetWebLink(m.Link);
            };
            DataTransferManager.ShowShareUI();
        }));

        public async Task RefreshNews(CancellationToken token = default(CancellationToken))
        {
            DataState = DataState.NotReady;
            using (ProgressState.BeginOperation())
            {
                News.Clear();
                var newsSources = _newsSources.Where(source => source.IsEnabled).ToList();
                if (newsSources.Count == 0)
                {
                    DataState = DataState.Empty;
                    return;
                }
                _newsService.SetNewsSources(newsSources);
                var newsLoading = _newsService.LoadNews().Select(task => task.ContinueWith(t =>
                    {
                        foreach (var newsModel in t.Result)
                        {
                            News.Add(newsModel);
                        }

                        if (t.Result.Count > 0)
                        {
                            DataState = DataState.Cached;
                        }
                    }, token,
                    TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()));
                await Task.WhenAll(newsLoading);
                token.ThrowIfCancellationRequested();
                if (News.Count == 0)
                {
                    DataState = DataState.Unavailable;
                }
                else
                {
                    DataState = DataState.Available;
                }
            }
        }

        private async Task Initialize()
        {
            var newsSources = await _storageService.Load<List<NewsSource>>(NewsSourcesPrefFilename);
            if (newsSources == null)
            {
                newsSources = _newsService.AvailableNewsSources;
                foreach (var newsSource in newsSources)
                {
                    newsSource.IsEnabled = true;
                }
            }
            else
            {
                newsSources.UpdateElements(_newsService.AvailableNewsSources, source => source.Source, source => source.Source,
                    (x, y) => { x.Url = y.Url; });
            }

            NewsSources = newsSources;
        }

        public async void OnNewsSourcesChanged()
        {
            _saveNewsSourceTokenSource?.Cancel();
            _saveNewsSourceTokenSource = new CancellationTokenSource();
            try
            {
                await _asyncLock.WaitAsync(_saveNewsSourceTokenSource.Token);
                await _storageService.Save(NewsSources, NewsSourcesPrefFilename, _saveNewsSourceTokenSource.Token);
                await RefreshNews();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        public override async void OnNavigatedTo(object parameter)
        {
            await Initialize();
            await RefreshNews();
        }
    }
}
