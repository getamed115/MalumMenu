using UnityEngine;

namespace MalumMenu.UI.Utilities;

/// <summary>
/// Defines the <see cref="Components" />
/// </summary>
internal static class Components
{
    /// <summary>
    /// Defines the <see cref="DisabledToggle" />
    /// </summary>
    public static class DisabledToggle
    {
        /// <summary>
        /// The Draw
        /// </summary>
        /// <param Name="label">The label<see cref="string"/></param>
        public static void Draw(string label)
        {
            label = " " + label;
            bool previousState = GUI.enabled;

            GUI.enabled = false;
            GUILayout.Toggle(false, label);
            GUI.enabled = previousState;
        }
    }
}
