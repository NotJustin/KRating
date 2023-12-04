using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace KRating;

public partial class KRating
{
    public required KRatingConfig Config { get; set; }
    public void OnConfigParsed(KRatingConfig config)
    {
        if (config.Version != ConfigVersion) throw new Exception($"You have a wrong config version. Delete it and restart the server to get the right version ({ConfigVersion})!");

        if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
        {
            throw new Exception($"You need to setup Database credentials in config!");
        }
        Config = config;
    }
}
public class KRatingConfig : BasePluginConfig
{
    public override int Version { get; set; } = 1;

    [JsonPropertyName("Database")]
    public KDatabase Database { get; set; } = new();
    [JsonPropertyName("Points")]
    public KPoints Points { get; set; } = new();

    public class KPoints
    {
        [JsonPropertyName("StartingPoints")]
        public int StartingPoints { get; set; } = 1000;
        [JsonPropertyName("DefaultModifier")]
        public float DefaultModifier { get; set; } = 1.0f;
        [JsonPropertyName("AwpModifier")]
        public float AwpModifier { get; set; } = 0.5f;
        [JsonPropertyName("KnifeModifier")]
        public float KnifeModifier { get; set; } = 3.0f;
        [JsonPropertyName("Multiplier")]
        public float Multiplier { get; set; } = 5f;
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