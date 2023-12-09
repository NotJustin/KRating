using MySqlConnector;

namespace KRating
{
    public partial class Player
    {
        public Player(MySqlConnection connection, ulong steamid64, int points, bool newPlayer, bool doLoad)
        {
            Connection = connection;
            Steamid64 = steamid64;
            Points = points;
            PointsOnLoad = points;
            NewPlayer = newPlayer;
            if (doLoad)
            {
                Task.Run(Load);
            }
        }
        // Only used by line:
        // IEnumerable<Player> results = await connection.QueryAsync<Player>(Queries.loadAllPlayers, new { steamid64s = steamid64s.ToArray() });
        public Player(ulong steamid64, int points)
        {
            Steamid64 = steamid64;
            Points = points;
        }
    }
}