using UnityEngine;

namespace MalumMenu;

public class ModesTab : ITab
{
    public string Name => "Modes";

    public void Draw()
    {
        GUILayout.BeginVertical(
            GUILayout.ExpandWidth(true)
        );

        CheatToggles.stealthMode = GUILayout.Toggle(
            CheatToggles.stealthMode,
            "Stealth Mode",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        CheatToggles.panicMode = GUILayout.Toggle(
            CheatToggles.panicMode,
            "Panic Mode",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        GUILayout.EndVertical();
    }
}
