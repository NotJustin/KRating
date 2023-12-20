using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace KRating;

public partial class Rating
{
    public required Config Config { get; set; }
    public Dictionary<string, float> WeaponMultiplierMap { get; set; } = new();
    public void OnConfigParsed(Config config)
    {
        if (config.Version != ConfigVersion)
        {
            Logger.LogError("Your config version ({config.Version}) is outdated. Delete it and reload the plugin to generate config version ({ConfigVersion}).", config.Version, ConfigVersion);
            return;
        }

        if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
        {
            Logger.LogError("Missing database credentials in config.");
            return;
        }
        Config = config;
        Config.Weapons.ForEach(weapon => WeaponMultiplierMap.Add(weapon.Name, (float)weapon.Multiplier));
        Config.Colors.ForEach(color => color.Color = Config.ConvertColorNameToChar(color.Name));
        RatingDistance = Config.Points.StartingPoints * Config.Points.RatingScale * Config.Points.Multiplier * Config.Points.MinPointExchange / Config.Points.MaxPointExchange;
    }
}
public class Config : BasePluginConfig
{
    public override int Version { get; set; } = Rating.ConfigVersion;
    [JsonPropertyName("Database")]
    public KDatabase Database { get; set; } = new();
    [JsonPropertyName("Points")]
    public KPoints Points { get; set; } = new();
    [JsonPropertyName("Weapons")]
    public List<KWeapon> Weapons { get; set; } = new();
    [JsonPropertyName("Colors")]
    public List<KColor> Colors { get; set; } = new()
    {
        new() { Name = "Grey", Bracket = 7 },
        new() { Name = "LightBlue", Bracket = 6 },
        new() { Name = "DarkBlue", Bracket = 5 },
        new() { Name = "Purple", Bracket = 4 },
        new() { Name = "LightPurple", Bracket = 3 },
        new() { Name = "LightRed", Bracket = 2 },
        new() { Name = "Gold", Bracket = 1 },
    };
    public class KPoints
    {
        [JsonPropertyName("StartingPoints")]
        public int StartingPoints { get; set; } = 10000;
        [JsonPropertyName("DefaultModifier")]
        public float DefaultModifier { get; set; } = 1.0f;
        public float Multiplier { get; set; } = 60f;
        [JsonPropertyName("MinPointExchange")]
        public int MinPointExchange { get; set; } = 20;
        [JsonPropertyName("MaxPointExchange")]
        public int MaxPointExchange { get; set; } = 120;
        [JsonPropertyName("RatingScale")]
        public float RatingScale { get; set; } = 0.5f;
    }
    public class KWeapon
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("Multiplier")]
        public double Multiplier { get; set; } = 1.0;
    }
    public class KDatabase
    {
        [JsonPropertyName("Host")]
        public string Host { get; set; } = "";
        [JsonPropertyName("Port")]
        public int Port { get; set; } = 3306;
        [JsonPropertyName("User")]
        public string User { get; set; } = "";
        [JsonPropertyName("Password")]
        public string Password { get; set; } = "";
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";
    }
    public class KColor
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("Bracket")]
        public int Bracket { get; set; }
        public char Color;
    }
    public static char ConvertColorNameToChar(string name)
    {
        return name switch
        {
            "White" => ChatColors.White,
            "Darkred" => ChatColors.Darkred,
            "LightYellow" => ChatColors.LightYellow,
            "LightBlue" => ChatColors.LightBlue,
            "Olive" => ChatColors.Olive,
            "Lime" => ChatColors.Lime,
            "Red" => ChatColors.Red,
            "LightPurple" => ChatColors.LightPurple,
            "Purple" => ChatColors.Purple,
            "Grey" => ChatColors.Grey,
            "Yellow" => ChatColors.Yellow,
            "Gold" => ChatColors.Gold,
            "Silver" => ChatColors.Silver,
            "Blue" => ChatColors.Blue,
            "DarkBlue" => ChatColors.DarkBlue,
            "BlueGrey" => ChatColors.BlueGrey,
            "Magenta" => ChatColors.Magenta,
            "LightRed" => ChatColors.LightRed,
            "Orange" => ChatColors.Orange,
            _ => ChatColors.Default,
        };
    }
}