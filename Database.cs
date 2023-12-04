using CounterStrikeSharp.API;
using Dapper;
using MySqlConnector;

namespace KRating;

public partial class KRating
{
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
    public void GetDatabaseConnection()
    {
        connection = new(DatabaseConnectionString);
        connection.Open();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            throw new Exception("[KRating] Unable connect to database!");
        }
    }

    public void BuildKRatingTable(bool hotReload)
    {
        if (connection == null)
        {
            return;
        }
        Task.Run(async () =>
        {
            MySqlTransaction transaction = await connection.BeginTransactionAsync();
            string buildKRatingTable = @"
                CREATE TABLE IF NOT EXISTS `KRating`
                (`steamid64` BIGINT NOT NULL PRIMARY KEY,
                `points` INT NOT NULL)
                ENGINE=InnoDB
                DEFAULT CHARSET=utf8mb3
                COLLATE=utf8mb3_unicode_ci";

            await connection.ExecuteAsync(buildKRatingTable, transaction: transaction);
            await transaction.CommitAsync();

            tableExists = true;

            Server.NextFrame(() =>
            {
                if (hotReload)
                {
                    Utilities.GetPlayers().ForEach(player =>
                    {
                        if (player.UserId == null)
                        {
                            return;
                        }
                        players.Add(new KPlayer(this, player.SteamID));
                    });
                }
            });
        });
    }
}