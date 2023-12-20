using Microsoft.Extensions.Logging;

namespace KRating;

public partial class Player
{
    public static string DatabaseConnectionString { get; set; } = string.Empty;
    public static ILogger? Logger { get; set; }
    public int Points { get; set; }
    public string Username { get; set; } = "";
    public ulong Steamid64 { get; set; }
    public bool NewPlayer { get; set; }
    public int PointsOnLoad { get; set; }
    public bool Loaded { get; set; } = false;
}