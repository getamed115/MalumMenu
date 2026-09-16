using UnityEngine;

namespace MalumMenu;

public class ConfigTab : ITab
{
    public string Name => "Config";

    public void Draw()
    {
        GUILayout.BeginVertical(
            GUILayout.ExpandWidth(true)
        );

        CheatState.openConfig = DrawToggle(
            CheatState.openConfig,
            "Open Config"
        );

        CheatState.reloadConfig = DrawToggle(
            CheatState.reloadConfig,
            "Reload Config"
        );

        CheatState.saveProfile = DrawToggle(
            CheatState.saveProfile,
            "Save to Profile"
        );

        CheatState.loadProfile = DrawToggle(
            CheatState.loadProfile,
            "Load from Profile"
        );

        GUILayout.EndVertical();
    }

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
}
