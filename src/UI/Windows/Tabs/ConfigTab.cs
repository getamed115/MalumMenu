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

        CheatToggles.openConfig = DrawToggle(
            CheatToggles.openConfig,
            "Open Config"
        );

        CheatToggles.reloadConfig = DrawToggle(
            CheatToggles.reloadConfig,
            "Reload Config"
        );

        CheatToggles.saveProfile = DrawToggle(
            CheatToggles.saveProfile,
            "Save to Profile"
        );

        CheatToggles.loadProfile = DrawToggle(
            CheatToggles.loadProfile,
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
