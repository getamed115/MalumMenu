using AmongUs.Data.Player;
using HarmonyLib;
using InnerNet;

namespace MalumMenu;

[HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
public static class ChatBubble_SetName
{
    public static void Postfix(ChatBubble __instance)
    {
        MalumESP.ChatNametags(__instance);
    }
}

[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
public static class PlayerBanData_BanMinutesLeft_Getter
{
    // Postfix patch of PlayerBanData.BanMinutesLeft Getter method to remove disconnect penalty
    public static void Postfix(PlayerBanData __instance, ref int __result)
    {
        if (!CheatToggles.avoidPenalties) return;

        __instance.BanPoints = 0f; // Removes all BanPoints
        __result = 0; // Removes all BanMinutes
    }
}

[HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
public static class GameContainer_SetupGameInfo
{
    // Moved to class level as a private constant
    private const string Separator = "<#0000>000000000000000</color>";

    public static void Postfix(GameContainer __instance)
    {
        // Early exit with added null safety checks to prevent game crashes
        if (!CheatToggles.seeLobbyInfo || __instance?.gameListing == null || __instance?.capacity == null)
            return;

        var listing = __instance.gameListing;

        // Store the original capacity before overwriting it
        string originalCapacity = __instance.capacity.text;
        string hostName = listing.TrueHostName;
        string lobbyCode = GameCode.IntToGameName(listing.GameId);
        string platform = Utils.PlatformTypeToString(listing.Platform);

        // Use string.Join to perfectly stack lines without ugly + "\n" + chains
        __instance.capacity.text = string.Join("\n",
            $"<size=40%>{Separator}",
            hostName,
            originalCapacity,
            $"<#fb0>{lobbyCode}</color>",
            $"<#b0f>{platform}</color>",
            $"{Separator}</size>"
        );
    }
}
