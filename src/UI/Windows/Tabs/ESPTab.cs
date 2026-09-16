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

        CheatState.seePlayerInfo = DrawToggle(
            CheatState.seePlayerInfo,
            "See Player Info"
        );

        CheatState.seeRoles = DrawToggle(
            CheatState.seeRoles,
            "See Roles"
        );

        CheatState.seeGhosts = DrawToggle(
            CheatState.seeGhosts,
            "See Ghosts"
        );

        CheatState.noShadows = DrawToggle(
            CheatState.noShadows,
            "No Shadows"
        );
        CheatState.revealVotes = DrawToggle(
            CheatState.revealVotes,
            "Reveal Votes"
        );

        CheatState.seeLobbyInfo = DrawToggle(
            CheatState.seeLobbyInfo,
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

            CheatState.zoomOut = DrawIndentedToggle(
                CheatState.zoomOut,
                "Zoom Out"
            );

            CheatState.spectate = DrawIndentedToggle(
                CheatState.spectate,
                "Spectate"
            );

            CheatState.freecam = DrawIndentedToggle(
                CheatState.freecam,
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

        CheatState.tracersCrew = DrawIndentedToggle(
            CheatState.tracersCrew,
            "Crewmates"
        );

        CheatState.tracersImps = DrawIndentedToggle(
            CheatState.tracersImps,
            "Impostors"
        );

        CheatState.tracersGhosts = DrawIndentedToggle(
            CheatState.tracersGhosts,
            "Ghosts"
        );

        CheatState.tracersBodies = DrawIndentedToggle(
            CheatState.tracersBodies,
            "Dead Bodies"
        );

        CheatState.colorBasedTracers = DrawIndentedToggle(
            CheatState.colorBasedTracers,
            "Color-based"
        );

        CheatState.distanceBasedTracers = DrawIndentedToggle(
            CheatState.distanceBasedTracers,
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

        CheatState.mapCrew = DrawIndentedToggle(
            CheatState.mapCrew,
            "Crewmates"
        );

        CheatState.mapImps = DrawIndentedToggle(
            CheatState.mapImps,
            "Impostors"
        );

        CheatState.mapGhosts = DrawIndentedToggle(
            CheatState.mapGhosts,
            "Ghosts"
        );

        CheatState.colorBasedMap = DrawIndentedToggle(
            CheatState.colorBasedMap,
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
