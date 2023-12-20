using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;

namespace KRating;

public partial class Rating
{
    public void RegisterListeners()
    {
        RegisterListener<Listeners.OnClientAuthorized>((int playerSlot, SteamID steamid) =>
        {
            // If the table does not exist, we cannot store anything.
            if (!tableExists)
            {
                Logger.LogError("[KRating] Attempted to load points before verifying KRating table exists!");
                return;
            }
            CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player.UserId != null && player.IsValid)
            {
                players.Add(new Player(player.PlayerName, steamid.SteamId64, Config.Points.StartingPoints, true, true));
            }
        });
    }
    [GameEventHandler]
    public HookResult OnPlayerChangeName(EventPlayerChangename @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        if (player.UserId == null || !player.IsValid || player.IsBot || player.IsHLTV)
        {
            return HookResult.Continue;
        }
        int index = players.FindIndex(kPlayer => kPlayer.Steamid64 == player.SteamID);
        if (index != -1)
        {
            players[index].Username = @event.Newname;
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        // If the table does not exist, we cannot store anything.
        if (!tableExists)
        {
            Logger.LogError("[KRating] Attempted to store points before verifying KRating table exists!");
            return HookResult.Continue;
        }
        CCSPlayerController player = @event.Userid;
        if (player.UserId == null || !player.IsValid || player.IsBot || player.IsHLTV)
        {
            return HookResult.Continue;
        }
        int index = players.FindIndex(kPlayer => kPlayer.Steamid64 == player.SteamID);
        if (index != -1)
        {
            Task.Run(async () =>
            {
                await players[index].StoreAsync();
                players.RemoveAt(index);
            });
        }
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController victim = @event.Userid;
        CCSPlayerController attacker = @event.Attacker;
        Player? kVictim = null, kAttacker = null;
        if (!victim.IsValid || victim.IsBot || victim.IsHLTV || victim.UserId == null)
        {
            return HookResult.Continue;
        }
        if (!attacker.IsValid || attacker.IsBot || attacker.IsHLTV || attacker.UserId == null)
        {
            return HookResult.Continue;
        }
        players.ForEach(player =>
        {
            if (player.Steamid64 == victim.SteamID)
            {
                kVictim = player;
            }
            else if (player.Steamid64 == attacker.SteamID)
            {
                kAttacker = player;
            }
        });
        if (kVictim == null || kAttacker == null)
        {
            Logger.LogError("[KRating] Failed to find kVictim or kAttacker in list of Players!");
            return HookResult.Continue;
        }
        int amount = GetPointsToExchange(kAttacker, kVictim, @event.Weapon);
        kVictim.Points -= amount;
        kAttacker.Points += amount;
        victim.PrintToChat(GetFormattedMessageForPointExchange(victim.PlayerName, 
                                                               kVictim.Points,
                                                               GetPlayerColor(kVictim.Points),
                                                               attacker.PlayerName,
                                                               kAttacker.Points,
                                                               GetPlayerColor(kAttacker.Points),
                                                               amount,
                                                               false));
        attacker.PrintToChat(GetFormattedMessageForPointExchange(attacker.PlayerName,
                                                                 kAttacker.Points,
                                                                 GetPlayerColor(kAttacker.Points),
                                                                 victim.PlayerName,
                                                                 kVictim.Points,
                                                                 GetPlayerColor(kVictim.Points),
                                                                 amount,
                                                                 true));
        return HookResult.Continue;
    }
}
