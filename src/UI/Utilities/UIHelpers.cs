using UnityEngine;

namespace MalumMenu;

public static class UIHelpers
{
    public static void ApplyUIColor()
    {
        var configHtmlColor = MalumMenu.menuHtmlColor.Value;

        if (!ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
        {
            if (!configHtmlColor.StartsWith("#"))
            {
                if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                {
                    GUI.backgroundColor = uiColor;
                }
            }
        }
        else
        {
            GUI.backgroundColor = uiColor;
        }
    }
}
