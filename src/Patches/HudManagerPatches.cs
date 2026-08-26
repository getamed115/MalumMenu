using HarmonyLib;
using MalumMenu.Utilities;
using System;

namespace MalumMenu;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class HudManager_Start
{
    // Postfix patch of HudManager.Start to give minimap access to impostors too
    public static void Postfix(HudManager __instance)
    {
        __instance.MapButton.OnClick.RemoveAllListeners(); // Remove previous OnClick action

        // Always open normal map when map button is clicked
        // To access sabotage map, sabotage button can be used
        __instance.MapButton.OnClick.AddListener((Action)(() =>
        {
            __instance.ToggleMapVisible(new MapOptions
            {
                Mode = MapOptions.Modes.Normal
            });

        }));
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudManager_Update
{
    public static void Postfix(HudManager __instance)
    {
        __instance.ShadowQuad.gameObject.SetActive(!MalumESP.IsFullbrightActive()); // Fullbright

        ChatController chat = __instance.Chat;
        ChatUtils.HandleChat(chat);

        MalumESP.ZoomOut(__instance);
        MalumESP.FreecamCheat();

        // Close PlayerPickMenu if there is no PPM cheat enabled
        if (PlayerPickMenu.playerpickMenu != null && CheatToggles.ShouldPPMClose())
        {
            PlayerPickMenu.playerpickMenu.Close();
        }
    }
}
