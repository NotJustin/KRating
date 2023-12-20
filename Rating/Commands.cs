using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace KRating;
public partial class Rating
{
	[ConsoleCommand("css_points", "Displays callers points in chat")]
	public void DisplayPoints(CCSPlayerController? caller, CommandInfo command)
	{
		if (caller == null || caller.IsBot || caller.IsHLTV || !caller.IsValid)
		{
			return;
		}
		Player? player = players.Find(player => player.Steamid64 == caller.SteamID);
        if (player == null)
        {
            Logger.LogError("[KRating] Failed to find player in list of players!");
			return;
        }
		string message = GetFormattedMessageForPointsCommand(player.Points, GetPlayerColor(player.Points));
        caller.PrintToChat(message);
    }
    [ConsoleCommand("css_top10", "Displays top10 in chat")]
    public void DisplayTopTen(CCSPlayerController? caller, CommandInfo command)
    {
        if (caller == null || caller.IsBot || caller.IsHLTV || !caller.IsValid)
        {
            return;
        }
        Player? player = players.Find(player => player.Steamid64 == caller.SteamID);
        if (player == null)
        {
            Logger.LogError("[KRating] Failed to find player in list of players!");
            return;
        }
        Task.Run(GetTopTenAsync).ContinueWith(task =>
        {
            Server.NextFrame(() =>
            {
                task.Result.ForEach(line =>
                {
                    caller.PrintToChat(line);
                });
            });
        });
    }
}
