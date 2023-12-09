using Dapper;
using MySqlConnector;

namespace KRating;

public partial class Rating
{
    public static class Queries
    {
        public const string buildKRatingTable = @"
            CREATE TABLE IF NOT EXISTS `KRating`(
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

        public const string storeAllPlayers =@"
            INSERT INTO `KRating` (`steamid64`, `points`)
            VALUES(@steamid64, @points)
            ON DUPLICATE KEY
            UPDATE `points` = VALUES(`points`)";
    }
    private void BuildDatabaseConnectionString()
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
    }
    public async Task BeginDatabaseConnection(List<ulong> steamid64s)
    {
        connection = new(DatabaseConnectionString);
        connection.Open();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            throw new Exception("[KRating] Unable connect to database!");
        }
        await BuildKRatingTable(steamid64s);
    }

    public async Task BuildKRatingTable(List<ulong> steamid64s)
    {
        MySqlTransaction transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(Queries.buildKRatingTable, transaction: transaction);
        await transaction.CommitAsync();

        tableExists = true;
        await LoadAllPlayers(steamid64s);
    }

    public async Task LoadAllPlayers(List<ulong> steamid64s)
    {
        IEnumerable<Player> results = await connection.QueryAsync<Player>(Queries.loadAllPlayers, new { steamid64s = steamid64s.ToArray() });
        // Grab all players from the database.
        foreach (Player result in results)
        {
            players.Add(new Player(connection, result.Steamid64, result.Points, false, false));
        }
        // Mark each of these players as an existing player.
        foreach (Player player in players)
        {
            player.Connection = connection;
            player.NewPlayer = false;
        }
        // Grab remaining online players that were not in the database.
        foreach (ulong steamid64 in steamid64s)
        {
            bool exists = false;
            foreach (Player player in players)
            {
                if (player.Steamid64 == steamid64)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                players.Add(new Player(connection, steamid64, Config.Points.StartingPoints, true, true));
            }
        }
    }

    public async Task StoreAllPlayers()
    {
        await connection.ExecuteAsync(Queries.storeAllPlayers, players);
    }
}