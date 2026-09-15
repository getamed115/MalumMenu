namespace MalumMenu;

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the <see cref="MinimapHandler" />
/// </summary>
public static class MinimapHandler
{
    /// <summary>
    /// Defines the minimapActive
    /// </summary>
    public static bool minimapActive;

    /// <summary>
    /// Defines the herePoints
    /// </summary>
    public static List<HerePoint> herePoints = new List<HerePoint>();

    /// <summary>
    /// Defines the herePointsToRemove
    /// </summary>
    public static List<HerePoint> herePointsToRemove = new List<HerePoint>();

    /// <summary>
    /// The IsCheatEnabled
    /// </summary>
    /// <returns>The <see cref="bool"/></returns>
    public static bool IsCheatEnabled() =>
        CheatToggles.mapCrew || CheatToggles.mapGhosts || CheatToggles.mapImps;

    /// <summary>
    /// The HandleHerePoint
    /// </summary>
    /// <param name="herePoint">The herePoint<see cref="HerePoint"/></param>
    public static void HandleHerePoint(HerePoint herePoint)
    {
        // Proactive null checks replace the slow try-catch block
        if (herePoint?.Sprite == null || herePoint.Player?.Data == null)
        {
            if (herePoint?.Sprite != null)
            {
                Object.Destroy(herePoint.Sprite.gameObject);
            }
            herePointsToRemove.Add(herePoint);
            return;
        }

        var playerData = herePoint.Player.Data;
        bool isDead = playerData.IsDead;
        bool isImpostor = playerData.Role.IsImpostor;

        // 1. Determine Visibility
        bool showAlive = !isDead && ((isImpostor && CheatToggles.mapImps) || (!isImpostor && CheatToggles.mapCrew));
        bool showDead = isDead && CheatToggles.mapGhosts;
        bool isVisible = showAlive || showDead;

        herePoint.Sprite.gameObject.SetActive(isVisible);

        // Early exit if we don't need to update graphics/position
        if (!isVisible) return;

        // 2. Determine Color
        Color iconColor = CheatToggles.colorBasedMap ? playerData.Color
            : (isDead ? Palette.White : playerData.Role.TeamColor);

        // 3. Apply Visuals
        herePoint.Sprite.material.SetColor(PlayerMaterial.BackColor, iconColor);
        herePoint.Sprite.material.SetColor(PlayerMaterial.BodyColor, iconColor);
        herePoint.Sprite.material.SetColor(PlayerMaterial.VisorColor, Palette.VisorColor);

        // 4. Sync Position
        Vector3 position = herePoint.Player.transform.position;
        position /= ShipStatus.Instance.MapScale;
        position.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
        position.z = -1f;

        herePoint.Sprite.transform.localPosition = position;
    }
}
