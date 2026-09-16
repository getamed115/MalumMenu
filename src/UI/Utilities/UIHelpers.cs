using UnityEngine;

namespace MalumMenu;

public static class UIHelpers
{
    public static void ApplyUIColor()
    {
        var htmlColor = MalumMenu.menuHtmlColor.Value?.Trim();

        if (string.IsNullOrEmpty(htmlColor))
            return;

        if (!htmlColor.StartsWith('#'))
            htmlColor = $"#{htmlColor}";

        if (ColorUtility.TryParseHtmlString(htmlColor, out var uiColor))
            GUI.backgroundColor = uiColor;
    }
}
