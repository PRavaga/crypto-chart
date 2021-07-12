using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Api;
using CryptoCoins.UWP.Models.Services.Sync;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.ViewModels.Converters;
using Microsoft.Practices.ServiceLocation;
using SQLite;

namespace CryptoCoins.UWP.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            InitServiceLocator();
        }

        public DashboardViewModel Dashboard => ServiceLocator.Current.GetInstance<DashboardViewModel>();
        public CoinsViewModel Coins => ServiceLocator.Current.GetInstance<CoinsViewModel>();
        public SettingsViewModel Settings => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public SupportUsViewModel SupportUs => ServiceLocator.Current.GetInstance<SupportUsViewModel>();
        public ShellViewModel Shell => ServiceLocator.Current.GetInstance<ShellViewModel>();
        public PortfolioViewModel Portfolio => ServiceLocator.Current.GetInstance<PortfolioViewModel>();
        public TransactionViewModel Transaction => ServiceLocator.Current.GetInstance<TransactionViewModel>();
        public AlertDialogViewModel AlertDialog => ServiceLocator.Current.GetInstance<AlertDialogViewModel>();
        public AlertsViewModel Alerts => ServiceLocator.Current.GetInstance<AlertsViewModel>();
        public NewsFeedViewModel NewsFeed => ServiceLocator.Current.GetInstance<NewsFeedViewModel>();
        public NewsWebViewModel NewsWeb => ServiceLocator.Current.GetInstance<NewsWebViewModel>();
        public CoinInfoWebViewModel CoinInfoWeb => ServiceLocator.Current.GetInstance<CoinInfoWebViewModel>();

        public ProgressService Pregress => ServiceLocator.Current.GetInstance<ProgressService>();

        private void InitServiceLocator()
        {
            var builder = new ContainerBuilder();

            RegisterServices(builder);
            var container = builder.Build();

            // Set the service locator to an AutofacServiceLocator.
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<DashboardViewModel>();
            builder.RegisterType<CoinsViewModel>();
            builder.RegisterType<SettingsViewModel>();
            builder.RegisterType<SupportUsViewModel>();
            builder.RegisterType<ShellViewModel>();
            builder.RegisterType<PortfolioViewModel>();
            builder.RegisterType<TransactionViewModel>();
            builder.RegisterType<AlertsViewModel>();
            builder.RegisterType<AlertDialogViewModel>();
            builder.RegisterType<NewsFeedViewModel>();
            builder.RegisterType<NewsWebViewModel>();
            builder.RegisterType<CoinInfoWebViewModel>();

            builder.RegisterType<CryptoApi>().SingleInstance();
            builder.RegisterType<CryptoService>().SingleInstance();
            builder.RegisterType<UserPreferencesService>().SingleInstance();
            builder.RegisterType<StorageService>().SingleInstance();
            builder.RegisterType<ProgressService>().SingleInstance();
            builder.RegisterType<NavigationService>().SingleInstance();
            builder.RegisterType<AppVersionMigrationService>().SingleInstance();
            builder.RegisterType<DialogService>().SingleInstance();
            builder.RegisterType<TileUpdateTask>().SingleInstance();
            builder.RegisterType<AlertsUpdateTask>().SingleInstance();
            builder.RegisterType<NewsService>().SingleInstance();
            builder.RegisterType<HoldingsService>().SingleInstance();
            builder.RegisterType<HoldingsStorage>().SingleInstance();
            builder.RegisterType<SqLiteConnectionProvider>().SingleInstance();
            builder.RegisterType<HoldingsConverter>().SingleInstance();
            builder.RegisterType<SyncService>().SingleInstance();
            builder.RegisterType<SyncBackgroundTask>().SingleInstance();
            builder.RegisterType<DonationsApi>().SingleInstance();
            builder.RegisterType<CoinmarketcapApi>().SingleInstance();
        }
    }
}
