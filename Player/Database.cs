using Microsoft.Extensions.Logging;
using Dapper;
using MySqlConnector;

namespace KRating
{
    public partial class Player
    {
        public static class Queries
        {
            public const string storePlayer = @"
                INSERT INTO `KRating` (`username`, `steamid64`, `points`)
                VALUES (@Username, @Steamid64, @Points)
                ON DUPLICATE KEY
                UPDATE `username` = @Username, `points` = @Points;";
            public const string loadPlayer = @"
                SELECT points
                FROM `KRating`
                WHERE `steamid64` = @Steamid64";
        }
        public async Task StoreAsync()
        {
            // Cannot store player information before they have finished loading.
            // In case they joined and immediately disconnected,
            // potentially before the load finished... this would prevent overwriting.
            if (!Loaded)
            {
                return;
            }
            // If points have not changed, there is nothing to store.
            if (Points == PointsOnLoad)
            {
                return;
            }
            try
            {
                using MySqlConnection connection = new(DatabaseConnectionString);
                await connection.OpenAsync();
                MySqlTransaction transaction = await connection.BeginTransactionAsync();
                await connection.ExecuteAsync(Queries.storePlayer, this, transaction: transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                if (Logger == null)
                {
                    return;
                }
                Logger.LogError("{Message}", ex.ToString());
            }
        }
        private async Task LoadAsync()
        {
            try
            {
                using MySqlConnection connection = new(DatabaseConnectionString);
                await connection.OpenAsync();
                dynamic? result = await connection.QuerySingleOrDefaultAsync(Queries.loadPlayer, this);
                if (result != null)
                {
                    IDictionary<string, object> row = (IDictionary<string, object>)result;
                    Points = (int)row["points"];
                    NewPlayer = false;
                }
                Loaded = true;
            }
            catch(Exception ex)
            {
                if (Logger == null)
                {
                    return;
                }
                Logger.LogError("{Message}", ex.ToString());
            }
        }
    }
}