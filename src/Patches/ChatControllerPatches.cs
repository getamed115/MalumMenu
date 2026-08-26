using HarmonyLib;
using System;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
public static class ChatController_AddChat
{
    // Prefix patch of ChatController.AddChat to receive ghost messages if CheatSettings.seeGhosts is enabled even if LocalPlayer is alive
    // Basically does what the original method did with the required modifications
    public static bool Prefix(PlayerControl sourcePlayer, string chatText, bool censor, ChatController __instance)
    {
        // Simply run original method if seeGhosts is disabled or LocalPlayer already dead
        if (!CheatToggles.seeGhosts || PlayerControl.LocalPlayer.Data.IsDead) return true;

        // Ensure both the source Player sending the message and the local Player exist
        if (!sourcePlayer || !PlayerControl.LocalPlayer) return true;

        // Cache Player data references
        NetworkedPlayerInfo localPlayerData = PlayerControl.LocalPlayer.Data;
        NetworkedPlayerInfo sourcePlayerData = sourcePlayer.Data;

        // Ensure underlying data is valid before processing
        if (sourcePlayerData == null || localPlayerData == null) return true; // Remove isDead check for LocalPlayer

        // Fetch a pooled chat bubble instance from the chat controller's object pool
        ChatBubble pooledBubble = __instance.GetPooledBubble();

        try
        {
            // Set the parent transform to the chat scroller inner container and reset scale
            pooledBubble.transform.SetParent(__instance.scroller.Inner);
            pooledBubble.transform.localScale = Vector3.one;

            // Determine if the message was sent by the local Player to align the bubble accordingly
            bool isLocalPlayer = sourcePlayer == PlayerControl.LocalPlayer;
            if (isLocalPlayer)
            {
                pooledBubble.SetRight();
            }
            else
            {
                pooledBubble.SetLeft();
            }

            // Check if the Player voted during a meeting to display the vote indicator on the chat bubble
            bool didVote = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);

            // Apply Player cosmetics and Name configuration to the chat bubble
            pooledBubble.SetCosmetics(sourcePlayerData);
            __instance.SetChatBubbleName(pooledBubble, sourcePlayerData, sourcePlayerData.IsDead, didVote, PlayerNameColor.Get(sourcePlayerData), null);

            // Assign the chat text and align all active chat layout elements
            pooledBubble.SetText(chatText);
            pooledBubble.AlignChildren();
            __instance.AlignAllBubbles();

            // Trigger the notification dot bounce animation if the chat window is closed and not currently animating
            if (!__instance.IsOpenOrOpening && __instance.notificationRoutine == null)
            {
                __instance.notificationRoutine = __instance.StartCoroutine(__instance.BounceDot());
            }

            // Play audio cue and set up notification toast if the message is from another Player and chat is closed
            if (!isLocalPlayer && !__instance.IsOpenOrOpening)
            {
                SoundManager.Instance.PlaySound(__instance.messageSound, false).pitch = 0.5f + sourcePlayer.PlayerId / 15f;
                __instance.chatNotification.SetUp(sourcePlayer, chatText);
            }
        }
        catch (Exception exception)
        {
            // Log any rendering errors and return the pooled bubble back to the pool to prevent memory/UI leaks
            ChatController.Logger.Error(exception.ToString(), null);
            __instance.chatBubblePool.Reclaim(pooledBubble);
        }

        return false; // Skips the original method completely
    }
}
