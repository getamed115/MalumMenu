using UnityEngine;

namespace MalumMenu;

public static class TracersHandler
{
    private const float MinTracerDistance = 2f;
    private const float MaxTracerDistance = 20f;

    // Draws a tracer from LocalPlayer to another player.
    public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
    {
        var localPlayer = PlayerControl.LocalPlayer;

        if (playerPhysics == null ||
            playerPhysics.myPlayer == null ||
            localPlayer == null)
        {
            return;
        }

        var player = playerPhysics.myPlayer;
        var playerData = player.Data;

        if (playerData == null || playerData.Role == null)
            return;

        var role = playerData.Role;
        var isDead = playerData.IsDead;
        var isImpostor = role.IsImpostor;

        var shouldDraw = isDead
            ? CheatState.tracersGhosts
            : CheatState.tracersCrew && !isImpostor ||
              CheatState.tracersImps && isImpostor;

        var color = shouldDraw
            ? GetTracerColor(
                targetPosition: player.transform.position,
                playerColor: playerData.Color,
                defaultColor: isDead ? Palette.White : role.TeamColor)
            : Color.clear;

        Utils.DrawTracer(
            player.gameObject,
            localPlayer.gameObject,
            color);
    }

    // Draws a tracer from LocalPlayer to an unreported dead body.
    public static void DrawBodyTracer(DeadBody deadBody)
    {
        var color = CheatState.tracersBodies
            ? GetTracerColor(
                targetPosition: deadBody.transform.position,
                playerColor: GameData.Instance
                    .GetPlayerById(deadBody.ParentId)
                    .Color,
                defaultColor: Color.yellow)
            : Color.clear;

        Utils.DrawTracer(
            deadBody.gameObject,
            PlayerControl.LocalPlayer.gameObject,
            color);
    }

    // Resolves the tracer color based on the active toggle mode.
    private static Color GetTracerColor(
        Vector3 targetPosition,
        Color playerColor,
        Color defaultColor)
    {
        if (CheatState.distanceBasedTracers)
            return GetDistanceBasedColor(targetPosition);

        return CheatState.colorBasedTracers
            ? playerColor
            : defaultColor;
    }

    // Gets a color based on the distance between LocalPlayer and the target.
    private static Color GetDistanceBasedColor(Vector3 targetPosition)
    {
        var localPosition = PlayerControl.LocalPlayer.transform.position;
        var distance = Vector3.Distance(targetPosition, localPosition);
        var normalizedDistance = Mathf.InverseLerp(
            MinTracerDistance,
            MaxTracerDistance,
            distance);

        return normalizedDistance < 0.5f
            ? Color.Lerp(
                Color.red,
                Color.yellow,
                normalizedDistance * 2f)
            : Color.Lerp(
                Color.yellow,
                Color.green,
                (normalizedDistance - 0.5f) * 2f);
    }
}
