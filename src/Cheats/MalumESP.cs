using System;
using UnityEngine;

namespace MalumMenu;

public static class MalumESP
{
    private static bool _freecamActive;
    private static bool _resolutionChangeNeeded;
    public static void SporeCloudVision(Mushroom mushroom)
    {
        if (CheatToggles.noShadows)
        {
            // Change the Z axis position of spore clouds as to make players appear above them
            mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, -1);
            return;
        }

        // Normal Z axis position: 5f
        mushroom.sporeMask.transform.position = new Vector3(mushroom.sporeMask.transform.position.x, mushroom.sporeMask.transform.position.y, 5f);
    }

    public static bool IsFullbrightActive()
    {
        // Fullbright is automatically activated when zooming out, spectating other players, or "freecamming"
        // This is done to avoid issues with shadows

        return CheatToggles.noShadows || Camera.main.orthographicSize > 3f || Camera.main.gameObject.GetComponent<FollowerCamera>().Target != PlayerControl.LocalPlayer;
    }

    public static void ZoomOut(HudManager hudManager)
    {
        if (CheatToggles.zoomOut)
        {
            if (hudManager.Chat.IsOpenOrOpening || PlayerCustomizationMenu.Instance || (Utils.isLobby && (FriendsListUI.Instance.IsOpen ||
                GameStartManager.Instance.LobbyInfoPane.LobbyViewSettingsPane.gameObject.active || GameStartManager.Instance.RulesEditPanel))) return;

            _resolutionChangeNeeded = true;

            if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Zoom out
            {

                // Both the main camera and the UI camera need to be adjusted

                Camera.main.orthographicSize++;
                hudManager.UICamera.orthographicSize++;

                // Utils.AdjustResolution() seems to be needed to properly sync the game's UI
                // after a change in orthographicSize

                Utils.AdjustResolution();

            }
            else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                // Zoom in
                if (!(Camera.main.orthographicSize > 3f)) return; // Never go below the default orthographicSize: 3f

                Camera.main.orthographicSize--;
                hudManager.UICamera.orthographicSize--;

                Utils.AdjustResolution();
            }
        }
        else
        {
            // orthographicSize is reset to default value: 3f
            Camera.main.orthographicSize = 3f;
            hudManager.UICamera.orthographicSize = 3f;

            // Utils.AdjustResolution() is invoked one last time to prevent issues with UI
            if (_resolutionChangeNeeded)
            {
                Utils.AdjustResolution();
                _resolutionChangeNeeded = false;
            }
        }
    }

    /*public static void MeetingNametags(MeetingHud meetingHud)
    {
        try
        {
            foreach (var playerState in meetingHud.playerStates)
            {
                // Fetch the NetworkedPlayerInfo of each playerState
                var data = GameData.Instance.GetPlayerById(playerState.TargetPlayerId);

                if (data.IsNull() || data.Disconnected || data.Outfits[PlayerOutfitType.Default].IsNull()) continue;

                // Update the Player's nametag appropriately
                playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                // Move and resize the nametag to prevent it overlapping with colorblind text
                if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                    playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    // Reset the position and scale of the nametag to default values (they're kinda weird but whatever)
                    playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                    playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                }
            }
        } catch { }
    }*/

    /*public static void MeetingNametags(MeetingHud meetingHud)
    {
        if (meetingHud == null ||
            meetingHud.playerStates == null ||
            GameData.Instance == null)
        {
            return;
        }

        foreach (var playerState in meetingHud.playerStates)
        {
            try
            {
                if (playerState == null || playerState.NameText == null)
                {
                    continue;
                }

                var data = GameData.Instance.GetPlayerById(playerState.PlayerId);

                if (data == null ||
                    data.Disconnected ||
                    data.Role == null)
                {
                    continue;
                }

                if (!data.Outfits.TryGetValue(
                        PlayerOutfitType.Default,
                        out var defaultOutfit) ||
                    defaultOutfit == null)
                {
                    continue;
                }

                playerState.NameText.text =
                    Utils.GetNameTag(
                        data,
                        defaultOutfit.PlayerName
                    );

                if (CheatToggles.seeRoles &&
                    CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition =
                        new Vector3(0.33f, 0.08f, 0f);

                    playerState.NameText.transform.localScale =
                        new Vector3(0.75f, 0.75f, 0.75f);
                }
                else if (CheatToggles.seeRoles ||
                         CheatToggles.seePlayerInfo)
                {
                    playerState.NameText.transform.localPosition =
                        new Vector3(0.3384f, 0.1125f, -0.1f);

                    playerState.NameText.transform.localScale =
                        new Vector3(0.9f, 1f, 1f);
                }
                else
                {
                    playerState.NameText.transform.localPosition =
                        new Vector3(0.3384f, 0.0311f, -0.1f);

                    playerState.NameText.transform.localScale =
                        new Vector3(0.9f, 1f, 1f);
                }
            }
            catch (Exception)
            {
            }
        }
    }*/

    public static void MeetingNametags(MeetingHud meetingHud)
    {
        if (meetingHud?.playerStates == null || GameData.Instance == null) return;

        // 1. Calculate UI layout ONCE outside the loop. 
        // This saves performance and prevents creating redundant Vector3s in memory.
        Vector3 targetPosition;
        Vector3 targetScale;

        if (CheatToggles.seeRoles && CheatToggles.seePlayerInfo)
        {
            targetPosition = new Vector3(0.33f, 0.08f, 0f);
            targetScale = new Vector3(0.75f, 0.75f, 0.75f);
        }
        else if (CheatToggles.seeRoles || CheatToggles.seePlayerInfo)
        {
            targetPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
            targetScale = new Vector3(0.9f, 1f, 1f);
        }
        else
        {
            targetPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
            targetScale = new Vector3(0.9f, 1f, 1f);
        }

        // 2. Iterate through players and apply data
        foreach (var playerState in meetingHud.playerStates)
        {
            // Compact null checks replace the slow, empty try-catch block
            if (playerState?.NameText == null) continue;

            var data = GameData.Instance.GetPlayerById(playerState.PlayerId);
            if (data == null || data.Disconnected || data.Role == null) continue;

            if (!data.Outfits.TryGetValue(PlayerOutfitType.Default, out var defaultOutfit) || defaultOutfit == null) continue;

            // Apply Name
            playerState.NameText.text = Utils.GetNameTag(data, defaultOutfit.PlayerName);

            // Apply Transform (cache the transform reference to avoid repeated native C++ calls)
            Transform nameTransform = playerState.NameText.transform;
            nameTransform.localPosition = targetPosition;
            nameTransform.localScale = targetScale;
        }
    }

    public static void PlayerNametags(PlayerPhysics playerPhysics)
    {
        // 1. Cache the Player reference to keep the code readable and efficient
        var player = playerPhysics?.myPlayer;

        // 2. Proactive null checks completely replace the slow try-catch block
        if (player?.Data == null || player.cosmetics?.nameText == null || player.CurrentOutfit == null)
        {
            return;
        }

        // 3. Update the Name tag
        string newNameTag = Utils.GetNameTag(player.Data, player.CurrentOutfit.PlayerName);
        player.cosmetics.SetName(newNameTag);

        // 4. Calculate the Y offset to prevent overlapping
        float yOffset = 0f;

        if (CheatToggles.seeRoles) yOffset += 0.093f;
        if (CheatToggles.seePlayerInfo) yOffset += 0.093f;

        // 5. Apply the transform once
        player.cosmetics.nameText.transform.localPosition = new Vector3(0f, yOffset, 0f);
    }

    public static void ChatNametags(ChatBubble chatBubble)
    {
        try
        {
            // Update the Player's nametag appropriately
            chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);

            // Adjust the chatBubble's size to the new nametag to prevent issues
            chatBubble.NameText.ForceMeshUpdate(true, true);
            chatBubble.Background.size = new Vector2(5.52f, 0.2f + chatBubble.NameText.GetNotDumbRenderedHeight() + chatBubble.TextArea.GetNotDumbRenderedHeight());
            chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);

        }
        catch { }
    }

    public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
    {
        try
        {

            if (playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
            {
                playerPhysics.myPlayer.Visible = CheatToggles.seeGhosts;
            }
        }
        catch { }
    }

    public static void FreecamCheat()
    {
        if (CheatToggles.freecam)
        {
            // Completely disable FollowerCamera
            if (!_freecamActive)
            {

                Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = false;
                Camera.main.gameObject.GetComponent<FollowerCamera>().Target = null;

                _freecamActive = true;

            }

            // Prevent the Player from moving while in freecam
            PlayerControl.LocalPlayer.moveable = false;

            // Get keyboard input
            var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);

            // Change the camera's position depending on the keyboard input
            // Speed: 10f
            Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;

        }
        else
        {
            // Re-enable FollowerCamera & movement once freecam is disabled
            if (!_freecamActive) return;
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().enabled = true;
            Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            _freecamActive = false;
        }
    }
}
