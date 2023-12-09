using MySqlConnector;

namespace KRating;

public partial class Player
{
    public MySqlConnection Connection { get; set; }
    public int Points { get; set; }
    public ulong Steamid64 { get; set; }
    public bool NewPlayer { get; set; }
    public int PointsOnLoad { get; set; }
}