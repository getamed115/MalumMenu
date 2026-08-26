using UnityEngine;

namespace MalumMenu;

public class PassiveTab : ITab
{
    public string Name => "Passive";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.WindowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();
    }

    private void DrawGeneral()
    {
        CheatToggles.freeCosmetics = GUILayout.Toggle(CheatToggles.freeCosmetics, " Free Cosmetics");

        CheatToggles.avoidPenalties = GUILayout.Toggle(CheatToggles.avoidPenalties, " Avoid Penalties");

        CheatToggles.unlockFeatures = GUILayout.Toggle(CheatToggles.unlockFeatures, " Unlock Extra Features");
    }
}
