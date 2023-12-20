using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace KRating;

public partial class Rating
{
    public static class Queries
    {
        public const string buildKRatingTable = @"
            CREATE TABLE IF NOT EXISTS `KRating`(
                `username` VARCHAR(32) NULL,
                `steamid64` BIGINT UNSIGNED NOT NULL PRIMARY KEY,
                `points` INT NOT NULL)
            ENGINE=InnoDB
            DEFAULT CHARSET=utf8mb3
            COLLATE=utf8mb3_unicode_ci";
        public const string loadAllPlayers = @"
            SELECT steamid64 as Steamid64,
                   points as Points
            FROM `KRating`
            WHERE `steamid64` in @steamid64s";
        public const string storeAllPlayers = @"
            INSERT INTO `KRating` (`username`, `steamid64`, `points`)
            VALUES(@Username, @Steamid64, @Points)
            ON DUPLICATE KEY
            UPDATE `username` = VALUES(`username), `points` = VALUES(`points`)";
        public const string getTopTen = @"
                SELECT username, points
                FROM `KRating`
                ORDER BY points DESC
                LIMIT 10";
    }
    private string BuildDatabaseConnectionString()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = Config.Database.Host,
            UserID = Config.Database.User,
            Password = Config.Database.Password,
            Database = Config.Database.Name,
            Port = (uint)Config.Database.Port,
            ConvertZeroDateTime = true,
            SslMode = MySqlSslMode.None
        };
        DatabaseConnectionString = builder.ConnectionString;
        return DatabaseConnectionString;
    }
    public async Task BeginDatabaseConnection(bool hotReload, List<Player> onlinePlayers)
    {
        using MySqlConnection connection = new(DatabaseConnectionString);
        await connection.OpenAsync();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            Logger.LogError("[KRating] Unable connect to database!");
        }
        await BuildKRatingTable(hotReload, onlinePlayers);
    }
    public async Task BuildKRatingTable(bool hotReload, List<Player> onlinePlayers)
    {
        try
        {
            using MySqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync();
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(Queries.buildKRatingTable, transaction: transaction);
            await transaction.CommitAsync();
            tableExists = true;
            if (hotReload && onlinePlayers.Count > 0)
            {
                await LoadAllPlayersAsync(onlinePlayers);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("{Message}", ex.ToString());
        }
    }
    public async Task LoadAllPlayersAsync(List<Player> onlinePlayers)
    {
        try
        {
            using MySqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync();
            IEnumerable<Player> results = await connection.QueryAsync<Player>(Queries.loadAllPlayers, new { steamid64s = onlinePlayers.Select(player => player.Steamid64).ToArray() });
            // Add all players from the database to the player list.
            foreach (Player result in results)
            {
                // Asserting that the player exists in the list of onlinePlayers because we just filtered onlinePlayers to grab these steamids in the first place.
                players.Add(new Player(onlinePlayers.Find(onlinePlayer => onlinePlayer.Steamid64 == result.Steamid64)!.Username, result.Steamid64, result.Points, false, false));
            }
            // Add all remaining online players that were not in the database to the player list.
            foreach (Player player in onlinePlayers.FindAll(onlinePlayer => !players.Exists(player => onlinePlayer.Steamid64 == player.Steamid64)))
            {
                players.Add(new Player(player.Username, player.Steamid64, Config.Points.StartingPoints, true, true));
            }
        }
        catch(Exception ex)
        {
            Logger.LogError("{Message}", ex.ToString());
        }
    }
    public async Task StoreAllPlayersAsync()
    {
        try
        {
            using MySqlConnection connection = new(DatabaseConnectionString);
            await connection.OpenAsync();
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(Queries.storeAllPlayers, players, transaction: transaction);
            await transaction.CommitAsync();
        }
        catch(Exception ex)
        {
            Logger.LogError("{Message}", ex.ToString());
        }
    }
    public async Task<List<string>> GetTopTenAsync()
    {
        using MySqlConnection connection = new(DatabaseConnectionString);
        await connection.OpenAsync();
        IEnumerable<Player> results = await connection.QueryAsync<Player>(Queries.getTopTen);
        return GetFormattedMessageForTopTenCommand(results);
    }
}