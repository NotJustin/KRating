using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Entities;
using Dapper;

namespace KRating;


[MinimumApiVersion(87)]
public partial class KRating : BasePlugin, IPluginConfig<KRatingConfig>
{
    public override void Load(bool hotReload)
    {
        BuildDatabaseConnectionString();
        GetDatabaseConnection();
        // If we're hotloading, load all players AFTER building the table.
        BuildKRatingTable(hotReload);

        RegisterListener<Listeners.OnClientAuthorized>((int playerSlot, SteamID steamid) =>
        {
            CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player.UserId != null && player.IsValid)
            {
                players.Add(new KPlayer(this, steamid.SteamId64));
            }
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            CCSPlayerController player = @event.Userid;
            if (player.UserId == null || !player.IsValid || player.IsBot || player.IsHLTV)
            {
                return HookResult.Continue;
            }
            Console.WriteLine("finding index");
            int index = players.FindIndex(kPlayer => kPlayer.steamid64 == player.SteamID);
            if (index != -1)
            {
                Console.WriteLine("index is NOT -1");
                players[index].Store();
                players.RemoveAt(index);
            }
            Console.WriteLine("continue");
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            CCSPlayerController victim = @event.Userid;
            CCSPlayerController attacker = @event.Attacker;
            if (!victim.IsValid || victim.IsBot || victim.IsHLTV || victim.UserId == null)
            {
                return HookResult.Continue;
            }
            if (!attacker.IsValid || attacker.IsBot || attacker.IsHLTV || attacker.UserId == null)
            {
                return HookResult.Continue;
            }
            KPlayer? kVictim = null;
            KPlayer? kAttacker = null;
            players.ForEach(player =>
            {
                if (player.steamid64 == victim.SteamID)
                {
                    kVictim = player;
                }
                else if (player.steamid64 == attacker.SteamID)
                {
                    kAttacker = player;
                }
            });
            if (kVictim == null || kAttacker == null)
            {
                return HookResult.Continue;
            }
            float weaponModifier = Config.Points.DefaultModifier;
            if (@event.Weapon.Equals("awp"))
            {
                weaponModifier = Config.Points.AwpModifier;
            }
            else if (@event.Weapon.Equals("knife"))
            {
                weaponModifier = Config.Points.KnifeModifier;
            }

            int amount = (int)Math.Floor((double)kAttacker.GetPoints() / kVictim.GetPoints() * weaponModifier * Config.Points.Multiplier);
            //  [ALL] Magic8Ball.gloveworks‎ [SPEC]: -5 points [995] killed by ff [1005]
            kVictim.SubtractPoints(amount);
            kAttacker.AddPoints(amount);
            string victimMessage = string.Format($" \x0F-{amount}\x01 points [{kVictim.GetPoints()}] killed by \x10{attacker.PlayerName}\x01 [{kAttacker.GetPoints()}]");
            string attackerMessage = string.Format($" \x06+{amount}\x01 points [{kAttacker.GetPoints()}] killed \x10{victim.PlayerName}\x01 [{kVictim.GetPoints()}]");
            victim.PrintToChat(victimMessage);
            attacker.PrintToChat(attackerMessage);
            return HookResult.Continue;
        });
    }

    public override void Unload(bool hotReload)
    {
        players.ForEach(kPlayer => kPlayer.Store());
    }

    public class KPlayer
    {
        private readonly KRating kRating;
        private int points;
        public readonly ulong steamid64;
        public bool newPlayer = false;
        public KPlayer(KRating kRating, ulong steamid64)
        {
            this.kRating = kRating;
            this.steamid64 = steamid64;
            Load();
        }

        public int GetPoints()
        {
            return points;
        }

        public void AddPoints(int amount)
        {
            points += amount;
        }

        public void SubtractPoints(int amount)
        {
            points -= amount;
        }

        public void Store()
        {
            // If this is null we would've thrown an exception previously.
            if (kRating.connection == null)
            {
                Console.WriteLine("null connection");
                return;
            }

            // If the table does not exist, we cannot store anything.
            if (!tableExists)
            {
                Console.WriteLine("exception");
                throw new Exception("[KRating] Attempted to store points before verifying KRating table exists!");
            }

            // If new player still has default points, there is no information worth storing.
            if (newPlayer && points == kRating.Config.Points.StartingPoints)
            {
                Console.WriteLine("new player");
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await kRating.connection.ExecuteAsync(@"
                        INSERT INTO `KRating` (`steamid64`, `points`)
                        VALUES (@steamid64, @points)
                        ON DUPLICATE KEY
                        UPDATE `points` = @points;"
                , new { steamid64, points });
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });
        }

        private void Load()
        {
            // If this is null we would've thrown an exception previously.
            if (kRating.connection == null)
            {
                return;
            }

            // If the table does not exist, we cannot store anything.
            if (!tableExists)
            {
                throw new Exception("[KRating] Attempted to load points before verifying KRating table exists!");
            }

            Task.Run(async () =>
            {
                dynamic? result = await kRating.connection.QuerySingleOrDefaultAsync(@"
                                    SELECT points FROM `KRating` WHERE `steamid64` = @steamid64"
                , new { steamid64 });
                if (result == null)
                {
                    points = kRating.Config.Points.StartingPoints;
                }
                else
                {
                    IDictionary<string, object> row = (IDictionary<string, object>)result;
                    points = (int)row["points"];
                }
            });

        }
    }
}