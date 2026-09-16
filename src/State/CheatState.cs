using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//[Obsolete("This class is obsolete and will deprecated")]
public struct CheatState
{
    public static bool seePlayerInfo { get; set; } = true;
    public static bool seeRoles { get; set; } = true;
    public static bool seeGhosts { get; set; } = true;
    public static bool noShadows;
    public static bool revealVotes;
    public static bool seeLobbyInfo
    {
        get => true;
        set { }
    }

    // Camera: permanently disabled
    public static bool spectate;
    public static bool zoomOut;
    public static bool freecam;

    // Minimap tracking: permanently disabled
    public static bool mapCrew;
    public static bool mapImps;
    public static bool mapGhosts;
    public static bool colorBasedMap
    {
        get => true;
        set { }
    }

    // Tracers: permanently disabled
    public static bool tracersImps { get; set; } = true;
    public static bool tracersCrew;
    public static bool tracersGhosts;
    public static bool tracersBodies
    {
        get => true;
        set { }
    }
    public static bool colorBasedTracers
    {
        get => true;
        set { }
    }
    public static bool distanceBasedTracers
    {
        get => false;
        set { }
    }

    // Chat
    // Basic chat availability remains writable.
    public static bool enableChat;

    // Passive
    // Restriction and penalty bypasses are permanently disabled.
    public static bool unlockFeatures;
    public static bool freeCosmetics;
    public static bool avoidPenalties;

    public static bool stealthMode
    {
        get => true;
        set { }
    }
    public static bool panicMode;
    public static bool fakePanicMode;

    // Config controls remain writable
    public static bool reloadConfig;
    public static bool openConfig;
    public static bool loadProfile;
    public static bool saveProfile;

    public static void DisablePPMCheats(string variableToKeep)
    {
        spectate &= string.Equals(variableToKeep, "spectate", StringComparison.Ordinal);
    }

    public static bool ShouldPPMClose()
    {
        return !spectate;
    }

    private static readonly FieldInfo[] ResettableToggleFields = typeof(CheatState)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(bool) && !field.IsInitOnly && !field.IsLiteral)
        .ToArray();

    private static readonly PropertyInfo[] ResettableToggleProperties = typeof(CheatState)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(property =>
            property.PropertyType == typeof(bool)
            && property.CanWrite
            && property.SetMethod is not null
            && property.SetMethod.IsPublic
        )
        .ToArray();

    public static void DisableAllExcept(params string[] togglesToKeep)
    {
        var namesToKeep = new HashSet<string>(
            togglesToKeep ?? Array.Empty<string>(),
            StringComparer.Ordinal
        );

        foreach (FieldInfo field in ResettableToggleFields)
        {
            if (namesToKeep.Contains(field.Name))
                continue;

            field.SetValue(null, false);
        }

        foreach (PropertyInfo property in ResettableToggleProperties)
        {
            if (namesToKeep.Contains(property.Name))
                continue;

            property.SetValue(null, false);
        }
    }

    public static void EnableAllExcept(params string[] toggleToExclude)
    {
        var namesToExclude = new HashSet<string>(
            toggleToExclude ?? Array.Empty<string>(),
            StringComparer.Ordinal
        );
        foreach (FieldInfo field in ResettableToggleFields)
        {
            if (namesToExclude.Contains(field.Name))
                continue;
            field.SetValue(null, true);
        }
        foreach (PropertyInfo property in ResettableToggleProperties)
        {
            if (namesToExclude.Contains(property.Name))
                continue;
            property.SetValue(null, true);
        }
    }
}
