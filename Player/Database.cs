using Dapper;

namespace KRating
{
    public partial class Player
    {
        public static class Queries
        {
            public const string storePlayer = @"
                INSERT INTO `KRating` (`steamid64`, `points`)
                VALUES (@steamid64, @points)
                ON DUPLICATE KEY
                UPDATE `points` = @points;";

            public const string loadPlayer = @"
                SELECT points
                FROM `KRating`
                WHERE `steamid64` = @steamid64";
        }

        public async Task Store()
        {
            // If points have not changed, there is nothing to store.
            if (Points == PointsOnLoad)
            {
                return;
            }
            await Connection.ExecuteAsync(Queries.storePlayer, new { Steamid64, Points });
        }

        private async Task Load()
        {
            dynamic? result = await Connection.QuerySingleOrDefaultAsync(Queries.loadPlayer, new { Steamid64 });
            if (result != null)
            {
                IDictionary<string, object> row = (IDictionary<string, object>)result;
                Points = (int)row["points"];
                NewPlayer = false;
            }
        }
    }
}