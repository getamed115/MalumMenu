using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHud_Update
{
    public static HashSet<int> votedPlayers = new HashSet<int>();

    public static void Prefix(MeetingHud __instance)
    {
        // Early exit to reduce nesting
        if (__instance.state >= MeetingHud.MeetingStates.Results) return;

        foreach (var playerArea in __instance.playerStates)
        {
            if (!playerArea) continue;

            HandleLiveVoteBlooping(__instance, playerArea);
            RevealPlayerVotes(playerArea);
        }

        // Reveal skipped votes
        if (__instance.SkippedVoting)
        {
            __instance.SkippedVoting.SetActive(CheatToggles.revealVotes);
        }
    }

    public static void Postfix(MeetingHud __instance)
    {
        MalumESP.MeetingNametags(__instance);
    }

    private static void HandleLiveVoteBlooping(MeetingHud hud, PlayerVoteArea playerArea)
    {
        var playerData = GameData.Instance.GetPlayerById(playerArea.PlayerId);
        if (playerData == null || playerData.Disconnected) return;

        // Check if a valid vote has been cast
        bool hasVoted = playerArea.VotedForId != PlayerVoteArea.HasNotVoted &&
                        playerArea.VotedForId != PlayerVoteArea.MissedVote &&
                        playerArea.VotedForId != PlayerVoteArea.DeadVote;

        if (hasVoted && votedPlayers.Add(playerArea.PlayerId)) // Add() returns true if it's a new addition
        {
            if (playerArea.VotedForId == PlayerVoteArea.SkippedVote)
            {
                if (hud.SkippedVoting)
                {
                    hud.BloopAVoteIcon(playerData, 0, hud.SkippedVoting.transform);
                }
            }
            else
            {
                // Use LINQ to find the target Player area cleanly
                var targetArea = hud.playerStates.FirstOrDefault(p => p.PlayerId == playerArea.VotedForId);
                if (targetArea != null)
                {
                    hud.BloopAVoteIcon(playerData, 0, targetArea.transform);
                }
            }
        }
    }

    private static void RevealPlayerVotes(PlayerVoteArea playerArea)
    {
        var voteSpreader = playerArea.GetComponent<VoteSpreader>();
        if (!voteSpreader) return;

        foreach (var spriteRenderer in voteSpreader.Votes)
        {
            spriteRenderer.gameObject.SetActive(CheatToggles.revealVotes);
        }
    }
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHud_PopulateResults
{
    public static void Prefix(MeetingHud __instance)
    {
        // Clear all standard Player vote icons
        foreach (var playerArea in __instance.playerStates)
        {
            if (playerArea)
            {
                ClearVotes(playerArea.GetComponent<VoteSpreader>());
            }
        }

        // Clear skipped vote icons
        if (__instance.SkippedVoting)
        {
            ClearVotes(__instance.SkippedVoting.GetComponent<VoteSpreader>());
        }

        MeetingHud_Update.votedPlayers.Clear();
    }

    private static void ClearVotes(VoteSpreader voteSpreader)
    {
        if (!voteSpreader || voteSpreader.Votes.Count == 0) return;

        foreach (var spriteRenderer in voteSpreader.Votes)
        {
            Object.DestroyImmediate(spriteRenderer);
        }

        voteSpreader.Votes.Clear();
    }
}
