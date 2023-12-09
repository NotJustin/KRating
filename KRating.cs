using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;

namespace KRating;

[MinimumApiVersion(101)]
public partial class Rating : BasePlugin, IPluginConfig<Config>
{
    public override void Load(bool hotReload)
    {
        RegisterListeners();
        BuildDatabaseConnectionString();
        List<ulong> steamid64s = new();
        Utilities.GetPlayers().ForEach(player =>
        {
            if (player.IsValid && !player.IsBot && !player.IsHLTV && player.UserId != null)
            {
                steamid64s.Add(player.SteamID);
            }
        });
        Task.Run(async () =>
        {
            await BeginDatabaseConnection(steamid64s);
        });
    }

    public override void Unload(bool hotReload)
    {
        Task.Run(StoreAllPlayers);
    }
}