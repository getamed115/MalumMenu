using HarmonyLib;
using Object = UnityEngine.Object;

namespace MalumMenu;

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
public static class MapBehaviour_ShowNormalMap
{
    // Postfix patch of MapBehaviour.ShowNormalMap to spawn herePoint icons for each Player
    public static void Postfix(MapBehaviour __instance)
    {
        MinimapHandler.minimapActive = MinimapHandler.IsCheatEnabled();

        // Early exit if the cheat is disabled
        if (!MinimapHandler.minimapActive) return;

        __instance.ColorControl.SetColor(Palette.Purple); // Custom map color
        __instance.DisableTrackerOverlays();

        // 1. Safely destroy old Player icons without using a slow try-catch block
        foreach (var point in MinimapHandler.herePoints)
        {
            // Rely on Unity's implicit boolean check to ensure the Sprite hasn't already been destroyed
            if (point != null && point.Sprite)
            {
                Object.Destroy(point.Sprite.gameObject);
            }
        }

        // 2. Clear the list directly instead of making a new one
        MinimapHandler.herePoints.Clear();

        // 3. Create new icons for each remote Player
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            // Skip if the Player object is missing/destroyed, or if it's the local Player
            if (!player || player.AmOwner) continue;

            var herePointSprite = Object.Instantiate(__instance.HerePoint, __instance.HerePoint.transform.parent);

            // Add directly to the main list, bypassing the need for a "temp" list
            MinimapHandler.herePoints.Add(new HerePoint(player, herePointSprite));
        }
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
public static class MapBehaviour_FixedUpdate
{
    // Postfix patch of MapBehaviour.FixedUpdate to update each herePoint icon's color and position on the map based on their respective Player
    public static void Postfix(MapBehaviour __instance)
    {
        // Reset map if miniMap cheat is disabled
        if (MinimapHandler.IsCheatEnabled() != MinimapHandler.minimapActive)
        {
            if (!__instance.infectedOverlay.gameObject.active) // Do not affect sabotage map
            {
                __instance.Close();
                __instance.ShowNormalMap();
            }
        }

        // Properly handles each herePoint icon on the map
        var temp = MinimapHandler.herePoints;
        foreach (var herePoint in temp)
        {
            MinimapHandler.HandleHerePoint(herePoint);
        }

        foreach (var herePoint in MinimapHandler.herePointsToRemove)
        {
            MinimapHandler.herePoints.Remove(herePoint);
        }

    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
public static class MapBehaviourClosePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        var herePoints = MinimapHandler.herePoints;

        if (herePoints == null) return;

        // Iterate through and safely destroy objects without the CPU overhead of exceptions
        foreach (var herePoint in herePoints)
        {
            // Rely on Unity's implicit boolean check to ensure the object hasn't been destroyed
            if (herePoint != null && herePoint.Sprite)
            {
                Object.Destroy(herePoint.Sprite.gameObject);
            }
        }

        // Clear the list safely
        herePoints.Clear();
    }
}
