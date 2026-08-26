using UnityEngine;

namespace MalumMenu;

public class ESPTab : ITab
{
    public string Name => "ESP";

    public void Draw()
    {
        /*
         * Do not specify a partial window width here.
         *
         * The previous:
         *
         * GUILayout.Width(MenuUI.WindowWidth * 0.425f)
         *
         * restricted the tab to less than half of the available width.
         */

        GUILayout.BeginVertical(
            GUILayout.ExpandWidth(true)
        );

        try
        {
            DrawGeneral();
            DrawSectionSeparator();

            bool cameraOptionsAvailable =
                !Utils.isLobby &&
                Utils.isInGame &&
                !Utils.isMeeting;

            DrawCamera(cameraOptionsAvailable);
            DrawSectionSeparator();

            DrawTracers();
            DrawSectionSeparator();

            DrawMinimap();
        }
        finally
        {
            GUILayout.EndVertical();
        }
    }

    private static void DrawGeneral()
    {
        GUILayout.Label(
            "General",
            GUIStylePreset.TabSubtitle,
            GUILayout.ExpandWidth(true)
        );

        CheatToggles.seePlayerInfo = DrawToggle(
            CheatToggles.seePlayerInfo,
            "See Player Info"
        );

        CheatToggles.seeRoles = DrawToggle(
            CheatToggles.seeRoles,
            "See Roles"
        );

        CheatToggles.seeGhosts = DrawToggle(
            CheatToggles.seeGhosts,
            "See Ghosts"
        );

        CheatToggles.noShadows = DrawToggle(
            CheatToggles.noShadows,
            "No Shadows"
        );
        CheatToggles.revealVotes = DrawToggle(
            CheatToggles.revealVotes,
            "Reveal Votes"
        );

        CheatToggles.seeLobbyInfo = DrawToggle(
            CheatToggles.seeLobbyInfo,
            "See Lobby Info"
        );
    }

    private static void DrawCamera(bool cameraOptionsAvailable)
    {
        GUILayout.Label(
            "Camera",
            GUIStylePreset.TabSubtitle,
            GUILayout.ExpandWidth(true)
        );

        bool previousEnabledState = GUI.enabled;

        try
        {
            GUI.enabled =
                previousEnabledState &&
                cameraOptionsAvailable;

            CheatToggles.zoomOut = DrawIndentedToggle(
                CheatToggles.zoomOut,
                "Zoom Out"
            );

            CheatToggles.spectate = DrawIndentedToggle(
                CheatToggles.spectate,
                "Spectate"
            );

            CheatToggles.freecam = DrawIndentedToggle(
                CheatToggles.freecam,
                "Freecam"
            );
        }
        finally
        {
            GUI.enabled = previousEnabledState;
        }
    }

    private static void DrawTracers()
    {
        GUILayout.Label(
            "Tracers",
            GUIStylePreset.TabSubtitle,
            GUILayout.ExpandWidth(true)
        );

        CheatToggles.tracersCrew = DrawIndentedToggle(
            CheatToggles.tracersCrew,
            "Crewmates"
        );

        CheatToggles.tracersImps = DrawIndentedToggle(
            CheatToggles.tracersImps,
            "Impostors"
        );

        CheatToggles.tracersGhosts = DrawIndentedToggle(
            CheatToggles.tracersGhosts,
            "Ghosts"
        );

        CheatToggles.tracersBodies = DrawIndentedToggle(
            CheatToggles.tracersBodies,
            "Dead Bodies"
        );

        CheatToggles.colorBasedTracers = DrawIndentedToggle(
            CheatToggles.colorBasedTracers,
            "Color-based"
        );

        CheatToggles.distanceBasedTracers = DrawIndentedToggle(
            CheatToggles.distanceBasedTracers,
            "Distance-based"
        );
    }

    private static void DrawMinimap()
    {
        GUILayout.Label(
            "Minimap",
            GUIStylePreset.TabSubtitle,
            GUILayout.ExpandWidth(true)
        );

        CheatToggles.mapCrew = DrawIndentedToggle(
            CheatToggles.mapCrew,
            "Crewmates"
        );

        CheatToggles.mapImps = DrawIndentedToggle(
            CheatToggles.mapImps,
            "Impostors"
        );

        CheatToggles.mapGhosts = DrawIndentedToggle(
            CheatToggles.mapGhosts,
            "Ghosts"
        );

        CheatToggles.colorBasedMap = DrawIndentedToggle(
            CheatToggles.colorBasedMap,
            "Color-based"
        );
    }

    /// <summary>
    /// Draws a primary option using the larger normal toggle style.
    /// </summary>
    private static bool DrawToggle(
        bool currentValue,
        string label
    )
    {
        return GUILayout.Toggle(
            currentValue,
            label,
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );
    }

    /// <summary>
    /// Draws an option belonging to a named subsection.
    /// </summary>
    private static bool DrawIndentedToggle(
        bool currentValue,
        string label
    )
    {
        return GUILayout.Toggle(
            currentValue,
            label,
            GUIStylePreset.IndentedToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );
    }

    /// <summary>
    /// Draws an unavailable option without allowing its value to change.
    /// </summary>
    private static void DrawDisabledToggle(string label)
    {
        bool previousState = GUI.enabled;

        GUI.enabled = false;

        GUILayout.Toggle(
            false,
            label,
            GUIStylePreset.IndentedToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        GUI.enabled = previousState;
    }

    /// <summary>
    /// Adds vertical separation between ESP subsections.
    /// </summary>
    private static void DrawSectionSeparator()
    {
        GUILayout.Space(6f);

        GUILayout.Box(
            GUIContent.none,
            GUIStylePreset.Separator,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1f)
        );

        GUILayout.Space(6f);
    }
}
