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

        CheatState.stealthMode = GUILayout.Toggle(
            CheatState.stealthMode,
            "Stealth Mode",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        CheatState.panicMode = GUILayout.Toggle(
            CheatState.panicMode,
            "Panic Mode",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        CheatState.fakePanicMode = GUILayout.Toggle(
            CheatState.fakePanicMode,
            "Fake Panic Mode",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        GUILayout.EndVertical();
    }
}
