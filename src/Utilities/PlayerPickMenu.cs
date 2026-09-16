using Il2CppSystem.Collections.Generic;
using Sentry.Internal.Extensions;
using UnityEngine;

namespace MalumMenu;

public static class PlayerPickMenu
{
    public static ShapeshifterMinigame Instance { get; private set; }
    public static NetworkedPlayerInfo TargetPlayerData { get; set; }

    public static bool IsActive;

    public static List<NetworkedPlayerInfo> PlayerList { get; private set; }

    public static Il2CppSystem.Action SelectionAction { get; private set; }

    // Get ShapeshifterMenu prefab to instantiate it
    // Found here: https://github.com/AlchlcDvl/TownOfUsReworked/blob/9f3cede9d30bab2c11eb7c960007ab3979f09156/TownOfUsReworked/Custom/Menu.cs
    public static ShapeshifterMinigame GetShapeshifterMenu()
    {
        var rolePrefab = Utils.GetBehaviourByRoleType(AmongUs.GameOptions.RoleTypes.Shapeshifter);
        return Object
            .Instantiate(rolePrefab?.Cast<ShapeshifterRole>(), GameData.Instance.transform)
            .ShapeshifterMenu;
    }

    // Open a PlayerPickMenu to pick a specific Player to target
    public static void OpenPlayerPickMenu(
        List<NetworkedPlayerInfo> players,
        Il2CppSystem.Action onSelected
    )
    {
        if (players == null || players.Count == 0)
            return;

        if (onSelected == null)
            return;

        if (Camera.main == null)
            return;

        if (GameData.Instance == null)
            return;

        IsActive = true;
        PlayerList = players;
        SelectionAction = onSelected;

        // The menu is based off the shapeshifting menu
        Instance = Object.Instantiate(GetShapeshifterMenu(), Camera.main.transform, false);

        Instance.transform.localPosition = new Vector3(0f, 0f, -50f);
        Instance.Begin(null);
    }

    // Returns a custom NetworkedPlayerInfo that can be used as a PPM choice
    public static NetworkedPlayerInfo CustomPPMChoice(
        string name,
        NetworkedPlayerInfo.PlayerOutfit outfit,
        RoleBehaviour role = null
    )
    {
        NetworkedPlayerInfo customChoice = Object.Instantiate<NetworkedPlayerInfo>(
            GameData.Instance.PlayerInfoPrefab
        );

        outfit.PlayerName = name;

        customChoice.Outfits[PlayerOutfitType.Default] = outfit;

        if (!role.IsNull())
            customChoice.Role = role;

        return customChoice;
    }
}
