using CounterStrikeSharp.API;
using MySqlConnector;

namespace KRating;

public partial class KRating
{
    public override string ModuleName => "KRating";
    public override string ModuleAuthor => "ff";
    public override string ModuleVersion => "0.0.1";

    public const int ConfigVersion = 1;

    private static bool tableExists = false;

    public List<KPlayer> players = new();
    public string DatabaseConnectionString = string.Empty;
    public MySqlConnection? connection = null;
}
