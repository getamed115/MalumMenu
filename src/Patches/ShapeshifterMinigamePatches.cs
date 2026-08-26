using AmongUs.Data;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
public static class ShapeshifterMinigame_Begin
{
    private const int ColumnsPerRow = 3;

    // Prefix patch of ShapeshifterMinigame.Begin to implement Player pick menu logic
    public static bool Prefix(ShapeshifterMinigame __instance)
    {
        if (!PlayerPickMenu.isActive) return true; // Open normal shapeshifter menu if not active

        List<NetworkedPlayerInfo> playerList = PlayerPickMenu.customPlayerList;

        __instance.potentialVictims = new List<ShapeshifterPanel>();
        var selectableElements = new List<UiElement>();

        for (int i = 0; i < playerList.Count; i++)
        {
            var playerData = playerList[i];
            var panel = CreatePanel(__instance, i, playerData);

            __instance.potentialVictims.Add(panel);
            selectableElements.Add(panel.Button);
        }

        ControllerManager.Instance.OpenOverlayMenu(
            __instance.name,
            __instance.BackButton,
            __instance.DefaultButtonSelected,
            selectableElements,
            false);

        PlayerPickMenu.isActive = false;

        return false; // Skip original method when active
    }

    // Instantiates and configures a single shapeshifter panel for the given Player.
    private static ShapeshifterPanel CreatePanel(ShapeshifterMinigame instance, int index, NetworkedPlayerInfo playerData)
    {
        int column = index % ColumnsPerRow;
        int row = index / ColumnsPerRow;

        var panel = Object.Instantiate(instance.PanelPrefab, instance.transform);
        panel.transform.localPosition = new Vector3(
            instance.XStart + column * instance.XOffset,
            instance.YStart + row * instance.YOffset,
            -1f);

        panel.SetPlayer(index, playerData, (Il2CppSystem.Action)(() =>
        {
            PlayerPickMenu.targetPlayerData = playerData; // Save targeted Player
            PlayerPickMenu.customAction.Invoke();          // Custom action set by openPlayerPickMenu
            instance.Close();
        }));

        if (playerData.Object != null)
        {
            ApplyNameTagLayout(panel, playerData);
        }

        return panel;
    }

    // Sets the Name tag text and positions/scales it based on active info toggles.
    private static void ApplyNameTagLayout(ShapeshifterPanel panel, NetworkedPlayerInfo playerData)
    {
        var nameText = panel.NameText;
        nameText.text = Utils.GetNameTag(playerData, playerData.DefaultOutfit.PlayerName);

        bool showRoles = CheatToggles.seeRoles;
        bool showPlayerInfo = CheatToggles.seePlayerInfo;

        Vector3 position;
        Vector3 scale = new(0.9f, 1f, 1f);

        if (showRoles && showPlayerInfo)
        {
            position = new Vector3(0.33f, 0.08f, 0f);
            scale = new Vector3(0.75f, 0.75f, 0.75f);
        }
        else if (showRoles || showPlayerInfo)
        {
            position = new Vector3(0.3384f, 0.1125f, -0.1f);
        }
        else
        {
            position = new Vector3(0.3384f, 0.0311f, -0.1f);
        }

        nameText.transform.localPosition = position;
        nameText.transform.localScale = scale;
    }
}

[HarmonyPatch(typeof(ShapeshifterPanel), nameof(ShapeshifterPanel.SetPlayer))]
public static class ShapeshifterPanel_SetPlayer
{
    // Prefix patch of ShapeshifterPanel.SetPlayer to allow usage of PlayerPickMenu in lobbies
    public static bool Prefix(ShapeshifterPanel __instance, int index, NetworkedPlayerInfo playerInfo, Il2CppSystem.Action onShift)
    {
        if (!PlayerPickMenu.isActive) return true; // Open normal shapeshifter menu if not active

        __instance.shapeshift = onShift;

        __instance.PlayerIcon.SetFlipX(false);
        __instance.PlayerIcon.ToggleName(false);

        int maskLayer = index + 2;
        foreach (var spriteRenderer in __instance.GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
        }

        __instance.PlayerIcon.SetMaskLayer(maskLayer);
        __instance.PlayerIcon.UpdateFromEitherPlayerDataOrCache(
            playerInfo, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);

        __instance.LevelNumberText.text = ProgressionManager.FormatVisualLevel(playerInfo.PlayerLevel);

        // Skips using custom nameplates because they break the PlayerPickMenu in lobbies
        __instance.NameText.text = playerInfo.PlayerName;

        DataManager.Settings.Accessibility.OnColorBlindModeChanged += (Il2CppSystem.Action)__instance.SetColorblindText;
        __instance.SetColorblindText();

        return false; // Skips original method when active
    }
}
