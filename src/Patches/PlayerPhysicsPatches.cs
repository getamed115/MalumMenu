using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
public static class PlayerPhysics_LateUpdate
{
    public static void Postfix(PlayerPhysics __instance)
    {
        MalumESP.PlayerNametags(__instance);
        MalumESP.SeeGhostsCheat(__instance);

        MalumPPMCheats.SpectatePPM();

        TracersHandler.DrawPlayerTracer(__instance);

        foreach (var bodyObject in GameObject.FindGameObjectsWithTag("DeadBody"))
            if (bodyObject.TryGetComponent<DeadBody>(out var deadBody))
                TracersHandler.DrawBodyTracer(deadBody);
    }
}
