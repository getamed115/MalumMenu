using Il2CppSystem.Collections.Generic;
using System;
using UnityEngine;

namespace MalumMenu;

public static class MalumPPMCheats
{
    private static bool _spectateActive;

    public static void SpectatePPM()
    {
        if (CheatState.spectate)
        {

            if (!_spectateActive)
            {

                // Close any Player pick menus already open & their cheats
                if (PlayerPickMenu.Instance != null)
                {
                    PlayerPickMenu.Instance.Close();
                    CheatState.DisablePPMCheats("spectate");
                }

                List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

                // All players are saved to playerList apart from LocalPlayer
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.AmOwner)
                    {
                        playerDataList.Add(player.Data);
                    }
                }

                // Player pick menu made for spectating the targeted Player
                PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Action)(() =>
                {
                    Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerPickMenu.TargetPlayerData.Object);
                }));

                _spectateActive = true;

                PlayerControl.LocalPlayer.moveable = false; // Can't move while spectating

                CheatState.freecam = false; // Disable incompatible cheats while spectating
            }

            // Deactivate cheat if menu is closed and no one is getting spectated
            if (PlayerPickMenu.Instance == null && Camera.main.gameObject.GetComponent<FollowerCamera>().Target == PlayerControl.LocalPlayer)
            {
                CheatState.spectate = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
        }
        else
        {
            // Deactivate cheat when it is disabled from the Malum GUI
            if (_spectateActive)
            {
                _spectateActive = false;
                PlayerControl.LocalPlayer.moveable = true;
                Camera.main.gameObject.GetComponent<FollowerCamera>().SetTarget(PlayerControl.LocalPlayer);
            }
        }
    }
}
