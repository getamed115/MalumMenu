using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TurnOnProtection))]
public static class PlayerControl_TurnOnProtection
{
    // Make all protections visible when seeGhosts is enabled;
    // otherwise preserve the original value.
    public static void Prefix(ref bool visible)
    {
        visible |= CheatToggles.seeGhosts;
    }
}