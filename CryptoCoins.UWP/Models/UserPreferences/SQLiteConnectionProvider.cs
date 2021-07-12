using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class SqLiteConnectionProvider
    {
        public const string DatabasePath = "UserPreferences";
        private Task<SQLiteAsyncConnection> _lazyConnection;

        public SqLiteConnectionProvider()
        {
            InitializeLazyConnection();
        }

        private void InitializeLazyConnection()
        {
            _lazyConnection = Task.FromResult(new SQLiteAsyncConnection(DatabasePath));
        }

        public event Action ConnectionReopened;


        public Task<SQLiteAsyncConnection> GetConnection()
        {
            return _lazyConnection;
        }

        private async Task<SQLiteAsyncConnection> ReplaceAndGetConnection(IBuffer buffer)
        {
            var connection = await GetConnection();
            await connection.CloseAsync();
            var dbFile = await ApplicationData.Current.LocalFolder.GetFileAsync(DatabasePath);
            using (var fileTransaction = await dbFile.OpenTransactedWriteAsync(StorageOpenOptions.None))
            {
                await fileTransaction.Stream.WriteAsync(buffer);
                await fileTransaction.CommitAsync();
            }

            return new SQLiteAsyncConnection(DatabasePath);
        }

        public async Task Execute(Func<SQLiteAsyncConnection,Task> func)
        {
            var connection = await GetConnection();
            await func(connection);
        }
        public async Task<T> Execute<T>(Func<SQLiteAsyncConnection,Task<T>> func)
        {
            var connection = await GetConnection();
            return await func(connection);
        }

        public async Task ReplaceBd(IBuffer buffer)
        {
            _lazyConnection = ReplaceAndGetConnection(buffer);
            await _lazyConnection;

            ConnectionReopened?.Invoke();
        }
    }
}
