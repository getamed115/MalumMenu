namespace MalumMenu.Utilities
{
    /// <summary>
    /// Provides helper methods for controlling chat UI visibility and focus behaviour
    /// </summary>
    internal static class ChatUtils
    {
        /// <summary>
        /// Updates the chat UI state (visibility, focus, submit permissions) based on the current game context
        /// </summary>
        /// <param Name="chat">The chat controller to update</param>
        public static void HandleChat(ChatController chat)
        {
            // Use Unity's native implicit boolean checks rather than `?.` to ensure 
            // the objects haven't been destroyed by the C++ engine.
            if (Utils.isLobby || !chat || !chat.freeChatField || !chat.freeChatField.textArea) return;

            var textArea = chat.freeChatField.textArea;

            if (!IsChatUiActive())
            {
                CloseChat();
                chat.freeChatField.SetCanSubmit(true);

                if (!textArea.hasFocus) textArea.GiveFocus();

                chat.gameObject.SetActive(false);
                return;
            }

            chat.gameObject.SetActive(true);

            // Safe Unity checking without `?.` 
            bool isDead = PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead;
            bool canWrite = Utils.isMeeting || isDead;

            chat.freeChatField.SetCanSubmit(canWrite);

            // Inline the focus logic to remove unnecessary private methods
            if (canWrite && !textArea.hasFocus)
                textArea.GiveFocus();
            else if (!canWrite && textArea.hasFocus)
                textArea.LoseFocus();
        }

        /// <summary>
        /// Opens the in-game chat window if it isn't already open or opening
        /// </summary>
        public static void OpenChat()
        {
            // Always check InstanceExists for DestroyableSingletons to prevent accidentally 
            // creating a new instance of the HUD manager while a scene is unloading.
            if (!DestroyableSingleton<HudManager>.InstanceExists) return;

            var hudChat = DestroyableSingleton<HudManager>.Instance.Chat;
            if (!hudChat || hudChat.IsOpenOrOpening) return;

            hudChat.chatScreen.SetActive(true);

            // Safer Player checks
            if (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.NetTransform)
            {
                PlayerControl.LocalPlayer.NetTransform.Halt();
            }

            hudChat.StartCoroutine(hudChat.CoOpen());

            if (DestroyableSingleton<FriendsListManager>.InstanceExists)
            {
                DestroyableSingleton<FriendsListManager>.Instance.SetFriendButtonColor(true);
            }

            if (hudChat.chatNotification && hudChat.chatNotification.gameObject.activeSelf)
            {
                hudChat.chatNotification.Close();
            }
        }

        /// <summary>
        /// Determines whether the chat UI should currently be active/visible
        /// </summary>
        /// <returns><c>true</c> if the chat UI should be shown; otherwise, <c>false</c></returns>
        public static bool IsChatUiActive()
        {
            // Rely completely on implicit bools. This accurately queries the Unity C++ side 
            // instead of just the C# wrapper.
            return CheatToggles.enableChat
                || MeetingHud.Instance
                || !ShipStatus.Instance
                || (PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead);
        }

        /// <summary>
        /// Force-closes the chat window if it's currently open or opening
        /// </summary>
        public static void CloseChat()
        {
            if (!DestroyableSingleton<HudManager>.InstanceExists) return;

            var hudChat = DestroyableSingleton<HudManager>.Instance.Chat;
            if (hudChat && hudChat.IsOpenOrOpening)
            {
                hudChat.ForceClosed();
            }
        }
    }
}
