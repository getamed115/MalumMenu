using UnityEngine;

namespace MalumMenu;

public class ChatTab : ITab
{
    public string Name => "Chat";

    public void Draw()
    {
        GUILayout.BeginVertical(
            GUILayout.ExpandWidth(true)
        );

        CheatState.enableChat = GUILayout.Toggle(
            CheatState.enableChat,
            "Enable Chat",
            GUIStylePreset.NormalToggle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(32f)
        );

        GUILayout.EndVertical();
    }
}
