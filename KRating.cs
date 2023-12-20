using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

namespace KRating;

[MinimumApiVersion(101)]
public partial class Rating : BasePlugin, IPluginConfig<Config>
{
    public override void Load(bool hotReload)
    {
        Player.Logger = Logger;
        Player.DatabaseConnectionString = BuildDatabaseConnectionString();
        RegisterListeners();
        // Need to pass a separate list of onlinePlayers instead of passing CCSPlayerController
        // because CCSPlayerController may change and cause the "Native <...> was invoked on a non-main thread.
        // By using this list we're just caching the info we need.
        List<Player> onlinePlayers = new();
        // This is not how I want to use this. I want this to be lateLoad.
        // This will create duplicates if I am actually hotloading...
        // So, I am instead commenting it out until I get an answer on what the appropriate thing would be to do here...
        /*if (hotReload)
        {
            Utilities.GetPlayers().ForEach(player =>
            {
                if (player.IsValid && !player.IsBot && !player.IsHLTV && player.UserId != null)
                {
                    onlinePlayers.Add(new(){ Username = player.PlayerName, Steamid64 = player.SteamID });
                }
            });
        }*/
        Task.Run(async () =>
        {
            await BeginDatabaseConnection(hotReload, onlinePlayers);
        });
    }
    public override void Unload(bool hotReload)
    {
        Task.Run(StoreAllPlayersAsync);
    }
}