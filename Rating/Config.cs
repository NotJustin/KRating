using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace KRating;

public partial class Rating
{
    public required Config Config { get; set; }
    public Dictionary<string, float> WeaponMultiplierMap { get; set; } = new();
    public void OnConfigParsed(Config config)
    {
        if (config.Version != ConfigVersion) throw new Exception($"Your config version ({config.Version}) is outdated. Delete it and reload the plugin to generate config version ({ConfigVersion}).");

        if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
        {
            throw new Exception($"Missing database credentials in config.");
        }
        Config = config;
        Config.Weapons.ForEach(weapon => WeaponMultiplierMap.Add(weapon.Name, (float)weapon.Multiplier));
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

    public class KPoints
    {
        [JsonPropertyName("StartingPoints")]
        public int StartingPoints { get; set; } = 10000;
        [JsonPropertyName("DefaultModifier")]
        public float DefaultModifier { get; set; } = 1.0f;
        public float Multiplier { get; set; } = 6f;
        [JsonPropertyName("MinPointExchange")]
        public int MinPointExchange { get; set; } = 10;
        [JsonPropertyName("MaxPointExchange")]
        public int MaxPointExchange { get; set; } = 60;
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
}