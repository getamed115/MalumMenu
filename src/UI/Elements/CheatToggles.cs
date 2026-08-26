using System;

namespace MalumMenu;

public struct CheatToggles
{
    public static bool seePlayerInfo { get => true; set { } }
    public static bool seeRoles { get => true; set { } }
    public static bool seeGhosts { get; set; } = true;
    public static bool noShadows;
    public static bool revealVotes;
    public static bool seeLobbyInfo { get => true; set { } }

    // Camera: permanently disabled
    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    // Minimap tracking: permanently disabled
    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap { get => true; set { } }

    // Tracers: permanently disabled
    public static bool tracersImps { get; set; } = true;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies { get => true; set { } }
    public static bool colorBasedTracers { get => true; set { } }
    public static bool distanceBasedTracers { get => false; set { } }

    // Chat
    // Basic chat availability remains writable.
    public static bool enableChat;

    // Passive
    // Restriction and penalty bypasses are permanently disabled.
    public static bool unlockFeatures;
    public static bool freeCosmetics;
    public static bool avoidPenalties;

    public static bool stealthMode { get => true; set { } }
    public static bool panicMode;

    // Config controls remain writable
    public static bool reloadConfig;
    public static bool openConfig;
    public static bool loadProfile;
    public static bool saveProfile;

    public static void DisablePPMCheats(string variableToKeep)
    {
        spectate &= string.Equals(
            variableToKeep,
            "spectate",
            StringComparison.Ordinal
        );
    }

    public static bool ShouldPPMClose()
    {
        return !spectate;
    }
}
