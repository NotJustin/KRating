using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API;

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
                throw new Exception("[KRating] Attempted to load points before verifying KRating table exists!");
            }
            CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player.UserId != null && player.IsValid)
            {
                players.Add(new Player(connection, steamid.SteamId64, Config.Points.StartingPoints, true, true));
            }
        });
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        // If the table does not exist, we cannot store anything.
        if (!tableExists)
        {
            throw new Exception("[KRating] Attempted to store points before verifying KRating table exists!");
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
                await players[index].Store();
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
            throw new Exception("[KRating] Failed to find kVictim or kAttacker in list of Players!");
        }
        float weaponMultiplier = WeaponMultiplierMap.ContainsKey(@event.Weapon) ? WeaponMultiplierMap[@event.Weapon] : Config.Points.DefaultModifier;
        // First get the amount ignoring the min/max possible points allowed to be exchanged
        int amount = (int)Math.Floor((double)kAttacker.Points / kVictim.Points * weaponMultiplier * Config.Points.Multiplier);
        // Now make sure the amount does not break the min/max bounds.
        amount = Math.Min(Math.Max(amount, Config.Points.MinPointExchange), Config.Points.MaxPointExchange);
        kVictim.Points -= amount;
        kAttacker.Points += amount;
        char victimColor, attackerColor;
        int step = 4;
        float ratingDistance = Config.Points.StartingPoints * Config.Points.RatingScale * Config.Points.Multiplier * Config.Points.MinPointExchange / Config.Points.MaxPointExchange;
        while (kAttacker.Points < Config.Points.StartingPoints + ratingDistance * step)
        {
            --step;
        }
        attackerColor = colors[step + 2];
        step = 4;
        while (kVictim.Points < Config.Points.StartingPoints + ratingDistance * step)
        {
            --step;
        }
        victimColor = colors[step + 2];
        string victimMessage = string.Format($" \x0F-{amount}\x01 points [{victimColor}{kVictim.Points:n0}\x01] killed by \x10{attacker.PlayerName}\x01 [{attackerColor}{kAttacker.Points:n0}\x01]");
        string attackerMessage = string.Format($" \x06+{amount}\x01 points [{attackerColor}{kAttacker.Points:n0}\x01] killed \x10{victim.PlayerName}\x01 [{victimColor}{kVictim.Points:n0}\x01]");
        victim.PrintToChat(victimMessage);
        attacker.PrintToChat(attackerMessage);
        return HookResult.Continue;
    }
}
