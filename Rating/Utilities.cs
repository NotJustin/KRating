using Microsoft.Extensions.Logging;

namespace KRating;
public partial class Rating
{
    public bool IsSeventhBracket(int points)
    {
        return !IsSixthBracket(points);
    }
    public bool IsSixthBracket(int points)
    {
        return !IsFifthBracket(points) && points >= Config.Points.StartingPoints - 1 * RatingDistance;
    }
    public bool IsFifthBracket(int points)
    {
        return !IsFourthBracket(points) && points >= Config.Points.StartingPoints;
    }
    public bool IsFourthBracket(int points)
    {
        return !IsThirdBracket(points) && points >= Config.Points.StartingPoints + 1 * RatingDistance;
    }
    public bool IsThirdBracket(int points)
    {
        return !IsSecondBracket(points) && points >= Config.Points.StartingPoints + 2 * RatingDistance;
    }
    public bool IsSecondBracket(int points)
    {
        return !IsFirstBracket(points) && points >= Config.Points.StartingPoints + 3 * RatingDistance;
    }
    public bool IsFirstBracket(int points)
    {
        return points >= Config.Points.StartingPoints + 4 * RatingDistance;
    }
    public Dictionary<char, Predicate<int>> PointColors => new() {
        { Config.Colors.Find(kColor => kColor.Bracket == 1)!.Color, IsFirstBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 2)!.Color, IsSecondBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 3)!.Color, IsThirdBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 4)!.Color, IsFourthBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 5)!.Color, IsFifthBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 6)!.Color, IsSixthBracket },
        { Config.Colors.Find(kColor => kColor.Bracket == 7)!.Color, IsSeventhBracket }
    };
    public char GetPlayerColor(int points)
    {
        foreach (var entry in PointColors)
        {
            if (entry.Value(points))
            {
                return entry.Key;
            }
        }
        Logger.LogError("[KRating] Failed to find bracket for points {points}!", points);
        return '\x01';
    }
    public int GetPointsToExchange(Player attacker, Player victim, string weapon)
    {
        float weaponMultiplier = WeaponMultiplierMap.ContainsKey(weapon) ? WeaponMultiplierMap[weapon] : Config.Points.DefaultModifier;

        // First get the amount ignoring the min/max possible points allowed to be exchanged
        int amount = (int)Math.Floor((double)victim.Points / attacker.Points * weaponMultiplier * Config.Points.Multiplier);

        // Now make sure the amount does not break the min/max bounds.
        amount = Math.Min(Math.Max(amount, Config.Points.MinPointExchange), Config.Points.MaxPointExchange);

        return amount;
    }
    public static string GetFormattedMessageForPointExchange(string playerName, int playerPoints, char playerColor, string opponentName, int opponentPoints, char opponentColor, int amount, bool positive)
    {
        return string.Format($" \x0f{(positive ? "\x06+" : "\x0f-")}{amount}\x01 points [{playerColor}{playerPoints:n0}\x01] killed {(positive ? "" : "by")} \x10{opponentName}\x01 [{opponentColor}{opponentPoints:n0}\x01]");
    }
    public static string GetFormattedMessageForPointsCommand(int points, char color)
    {
        return string.Format($" You have [{color}{points:n0}\x01] points.");
    }
    public List<string> GetFormattedMessageForTopTenCommand(IEnumerable<Player> players)
    {
        List<string> topTenList = new() { "Top Ten Players:" };
        foreach (Player player in players)
        {
            topTenList.Add($"{(player.Username == "" ? "N/A" : player.Username)} - [{GetPlayerColor(player.Points)}{player.Points}\x01]");
        }
        return topTenList;
    }
}
