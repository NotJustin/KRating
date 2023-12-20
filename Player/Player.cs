namespace KRating
{
    public partial class Player
    {
        public Player()
        {

        }
        public Player(string username, ulong steamid64, int points, bool newPlayer, bool doLoad)
        {
            Username = username;
            Steamid64 = steamid64;
            Points = points;
            PointsOnLoad = points;
            NewPlayer = newPlayer;
            if (doLoad)
            {
                Task.Run(LoadAsync);
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