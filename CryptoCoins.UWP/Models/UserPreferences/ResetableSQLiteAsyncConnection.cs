using System.Threading.Tasks;
using SQLite;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class ResetableSQLiteAsyncConnection : SQLiteAsyncConnection

    {
        public ResetableSQLiteAsyncConnection(string databasePath, bool storeDateTimeAsTicks = true) : base(databasePath, storeDateTimeAsTicks)
        {
        }

        public ResetableSQLiteAsyncConnection(string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true) : base(databasePath, openFlags,
            storeDateTimeAsTicks)
        {
        }

        public async Task Reopen()
        {

        }
    }
}
