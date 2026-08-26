/*using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

public static class MinimapHandler
{
    public static bool minimapActive;
    public static List<HerePoint> herePoints = new List<HerePoint>();
    public static List<HerePoint> herePointsToRemove = new List<HerePoint>();

    public static bool IsCheatEnabled()
    {
        return CheatToggles.mapCrew || CheatToggles.mapGhosts || CheatToggles.mapImps;
    }

    public static void HandleHerePoint(HerePoint herePoint)
    {
        Color herePointColor = new Color();

        try // try-catch to fix issues caused by Player disconnection
        {
            herePoint.Sprite.gameObject.SetActive(false); // Initally make Player icon invisible

            // Crewmate, alive
            if (CheatToggles.mapCrew && !herePoint.Player.Data.Role.IsImpostor)
            {
                if (!herePoint.Player.Data.IsDead)
                {
                    herePoint.Sprite.gameObject.SetActive(true);
                    if (CheatToggles.colorBasedMap)
                    {
                        herePointColor = herePoint.Player.Data.Color; // Color-Based Icon
                    }
                    else
                    {
                        herePointColor = herePoint.Player.Data.Role.TeamColor; // Role-Based Icon
                    }
                }
            }
            // Impostor, alive
            else if (CheatToggles.mapImps && herePoint.Player.Data.Role.IsImpostor)
            {
                if (!herePoint.Player.Data.IsDead)
                {
                    herePoint.Sprite.gameObject.SetActive(true);
                    if (CheatToggles.colorBasedMap)
                    {
                        herePointColor = herePoint.Player.Data.Color; // Color-Based Icon
                    }
                    else
                    {
                        herePointColor = herePoint.Player.Data.Role.TeamColor; // Role-Based Icon
                    }
                }
            }
            // Any Role, dead
            if (CheatToggles.mapGhosts && herePoint.Player.Data.IsDead)
            {
                herePoint.Sprite.gameObject.SetActive(true);
                if (CheatToggles.colorBasedMap)
                {
                    herePointColor = herePoint.Player.Data.Color; // Color-Based Icon
                }
                else
                {
                    herePointColor = Palette.White;
                }
            }

            if (herePoint.Sprite.gameObject.active)
            {
                // Set the right colors for active herePoint icons
                herePoint.Sprite.material.SetColor(PlayerMaterial.BackColor, herePointColor);
                herePoint.Sprite.material.SetColor(PlayerMaterial.BodyColor, herePointColor);
                herePoint.Sprite.material.SetColor(PlayerMaterial.VisorColor, Palette.VisorColor);

                // Sync the position of active herePoint icons with their players
                var vector = herePoint.Player.transform.position;
                vector /= ShipStatus.Instance.MapScale;
                vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
                vector.z = -1f;
                herePoint.Sprite.transform.localPosition = vector;
            }
        }
        catch
        {
            // Remove icons that are causing problems
            Object.Destroy(herePoint.Sprite.gameObject);
            herePointsToRemove.Add(herePoint);
        }
    }
}
*/

using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu
{
    public static class MinimapHandler
    {
        public static bool minimapActive;
        public static List<HerePoint> herePoints = new List<HerePoint>();
        public static List<HerePoint> herePointsToRemove = new List<HerePoint>();

        public static bool IsCheatEnabled() =>
            CheatToggles.mapCrew || CheatToggles.mapGhosts || CheatToggles.mapImps;

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
            Color iconColor;
            if (CheatToggles.colorBasedMap)
            {
                iconColor = playerData.Color;
            }
            else
            {
                // If dead, use white. Otherwise, use their team color.
                iconColor = isDead ? Palette.White : playerData.Role.TeamColor;
            }

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
}