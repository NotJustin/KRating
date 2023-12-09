using MySqlConnector;
using CounterStrikeSharp.API.Modules.Utils;

namespace KRating;

public partial class Rating
{
    public override string ModuleName => "KRating";
    public override string ModuleAuthor => "ff";
    public override string ModuleVersion => "0.0.2";

    public const int ConfigVersion = 2;

    public bool tableExists = false;

    public List<Player> players = new();
    public string DatabaseConnectionString = string.Empty;
    public MySqlConnection connection = null!;

    public readonly char[] colors = {
        ChatColors.Grey,
        ChatColors.LightBlue,
        ChatColors.DarkBlue,
        ChatColors.Purple,
        ChatColors.LightPurple,
        ChatColors.LightRed,
        ChatColors.Gold
    };
}
