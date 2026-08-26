using UnityEngine;

namespace MalumMenu;

public static class TracersHandler
{
    private const float MinTracerDistance = 2f;  // Red at this distance or closer
    private const float MaxTracerDistance = 20f; // Green at this distance or farther

    // Draws a tracer from LocalPlayer to another Player.
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        try
        {
            var player = playerPhysics.myPlayer;
            var isDead = player.Data.IsDead;
            var isImpostor = player.Data.Role.IsImpostor;

            var shouldDraw = isDead
                ? CheatToggles.tracersGhosts
                : (CheatToggles.tracersCrew && !isImpostor) || (CheatToggles.tracersImps && isImpostor);

            var color = shouldDraw
                ? GetTracerColor(player.transform.position, player.Data.Color, defaultColor: isDead ? Palette.White : player.Data.Role.TeamColor)
                : Color.clear;

            Utils.DrawTracer(player.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        }
        catch { }
    }

    // Draws a tracer from LocalPlayer to a dead body. Only draws tracers for unreported dead bodies.
    public static void DrawBodyTracer(DeadBody deadBody)
    {
        var color = CheatToggles.tracersBodies
            ? GetTracerColor(deadBody.transform.position, GameData.Instance.GetPlayerById(deadBody.ParentId).Color, defaultColor: Color.yellow)
            : Color.clear;

        Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
    }

    // Resolves the tracer color based on the active toggle mode (distance, Player color, or default).
    private static Color GetTracerColor(Vector3 targetPosition, Color playerColor, Color defaultColor)
    {
        if (CheatToggles.distanceBasedTracers)
            return GetDistanceBasedColor(targetPosition);

        if (CheatToggles.colorBasedTracers)
            return playerColor;

        return defaultColor;
    }

    // Gets a color based on the distance between the LocalPlayer and a target position.
    // Closer distances are red, medium distances are yellow, and farther distances are green.
    private static Color GetDistanceBasedColor(Vector3 targetPosition)
    {
        var distance = Vector3.Distance(targetPosition, PlayerControl.LocalPlayer.transform.position);
        var normalized = Mathf.InverseLerp(MinTracerDistance, MaxTracerDistance, distance);

        return normalized < 0.5f
            ? Color.Lerp(Color.red, Color.yellow, normalized * 2f)
            : Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f);
    }
}
