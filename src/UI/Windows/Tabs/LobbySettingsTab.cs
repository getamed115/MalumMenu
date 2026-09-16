using UnityEngine;

namespace MalumMenu;

public class LobbySettingsTab : ITab
{
    public string Name => "Lobby Settings";

    public void Draw()
    {
        GUILayout.BeginVertical(
            GUILayout.ExpandWidth(true)
        );

        GUILayout.Label("<color=#00ff00>✔ Complete</color>");
        GUILayout.Label("Anonymouse votes: ");

        GUILayout.EndVertical();
    }
}
