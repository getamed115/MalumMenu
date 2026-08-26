using HarmonyLib;

namespace MalumMenu;

[HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetAnonymousVotes))]
public static class LogicOptions_GetAnonymousVotes
{
    // Disable anonymous votes when revealVotes is enabled;
    // otherwise preserve the original result.
    public static void Postfix(ref bool __result)
    {
        __result &= !CheatToggles.revealVotes;
    }
}

[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
public static class LogicOptionsNormal_GetAnonymousVotes
{
    // Disable anonymous votes when revealVotes is enabled;
    // otherwise preserve the original result.
    public static void Postfix(ref bool __result)
    {
        __result &= !CheatToggles.revealVotes;
    }
}
