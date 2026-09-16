using System;
using UnityEngine;

namespace MalumMenu;

public static class GUIStylePreset
{
    private static GUISkin _sourceSkin;

    private static GUIStyle _separator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;
    private static GUIStyle _indentedToggle;

    private static GUIStyle _tabButton;
    private static GUIStyle _tabTitle;
    private static GUIStyle _tabSubtitle;

    private static GUIStyle _accordionHeader;
    private static GUIStyle _accordionHeaderExpanded;
    private static GUIStyle _accordionContent;

    private static GUIStyle _menuHeader;
    private static GUIStyle _closeButton;

    public static GUIStyle Separator
    {
        get
        {
            EnsureInitialized();
            return _separator;
        }
    }

    public static GUIStyle NormalButton
    {
        get
        {
            EnsureInitialized();
            return _normalButton;
        }
    }

    public static GUIStyle NormalToggle
    {
        get
        {
            EnsureInitialized();
            return _normalToggle;
        }
    }

    public static GUIStyle IndentedToggle
    {
        get
        {
            EnsureInitialized();
            return _indentedToggle;
        }
    }

    public static GUIStyle TabButton
    {
        get
        {
            EnsureInitialized();
            return _tabButton;
        }
    }

    public static GUIStyle TabTitle
    {
        get
        {
            EnsureInitialized();
            return _tabTitle;
        }
    }

    public static GUIStyle TabSubtitle
    {
        get
        {
            EnsureInitialized();
            return _tabSubtitle;
        }
    }

    public static GUIStyle AccordionHeader
    {
        get
        {
            EnsureInitialized();
            return _accordionHeader;
        }
    }

    public static GUIStyle AccordionHeaderExpanded
    {
        get
        {
            EnsureInitialized();
            return _accordionHeaderExpanded;
        }
    }

    public static GUIStyle AccordionContent
    {
        get
        {
            EnsureInitialized();
            return _accordionContent;
        }
    }

    public static GUIStyle MenuHeader
    {
        get
        {
            EnsureInitialized();
            return _menuHeader;
        }
    }

    public static GUIStyle CloseButton
    {
        get
        {
            EnsureInitialized();
            return _closeButton;
        }
    }

    public static void EnsureInitialized()
    {
        GUISkin currentSkin = GUI.skin;

        if (currentSkin == null)
        {
            throw new InvalidOperationException(
                "GUIStylePreset must be initialized from OnGUI()."
            );
        }

        if (_sourceSkin == currentSkin &&
            _normalButton != null)
        {
            return;
        }

        Reset();

        _sourceSkin = currentSkin;

        CreateBasicStyles(currentSkin);
        CreateLegacyTabStyles(currentSkin);
        CreateMenuStyles(currentSkin);
    }

    private static void CreateBasicStyles(GUISkin skin)
    {
        _separator = new GUIStyle(skin.box)
        {
            fixedHeight = 1f,

            margin = CreateRectOffset(
                left: 0,
                right: 0,
                top: 4,
                bottom: 4
            ),

            padding = CreateRectOffset(
                left: 0,
                right: 0,
                top: 0,
                bottom: 0
            ),

            border = CreateRectOffset(
                left: 0,
                right: 0,
                top: 0,
                bottom: 0
            )
        };

        _separator.normal.background =
            Texture2D.whiteTexture;

        _normalButton = new GUIStyle(skin.button)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 34f,

            padding = CreateRectOffset(
                left: 10,
                right: 10,
                top: 4,
                bottom: 4
            )
        };

        _normalToggle = new GUIStyle(skin.toggle)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 30f,

            padding = CreateRectOffset(
                left: 22,
                right: 8,
                top: 3,
                bottom: 3
            )
        };

        _indentedToggle = new GUIStyle(_normalToggle)
        {
            padding = CreateRectOffset(
                left: 34,
                right: 8,
                top: 3,
                bottom: 3
            )
        };
    }

    private static void CreateLegacyTabStyles(GUISkin skin)
    {
        _tabButton = new GUIStyle(skin.button)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };

        _tabTitle = new GUIStyle(skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 32f
        };

        _tabSubtitle = new GUIStyle(skin.label)
        {
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 30f,

            margin = CreateRectOffset(
                left: 0,
                right: 0,
                top: 8,
                bottom: 2
            )
        };
    }

    private static void CreateMenuStyles(GUISkin skin)
    {
        _menuHeader = new GUIStyle(skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,

            padding = CreateRectOffset(
                left: 8,
                right: 8,
                top: 0,
                bottom: 0
            )
        };

        _accordionHeader = new GUIStyle(skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 42f,
            wordWrap = false,

            padding = CreateRectOffset(
                left: 14,
                right: 12,
                top: 0,
                bottom: 0
            ),

            margin = CreateRectOffset(
                left: 0,
                right: 0,
                top: 2,
                bottom: 2
            )
        };

        _accordionHeaderExpanded =
            new GUIStyle(_accordionHeader);

        SetTextColorForAllStates(
            _accordionHeaderExpanded,
            Color.white
        );

        _accordionContent = new GUIStyle(skin.box)
        {
            padding = CreateRectOffset(
                left: 12,
                right: 12,
                top: 12,
                bottom: 12
            ),

            margin = CreateRectOffset(
                left: 0,
                right: 0,
                top: 0,
                bottom: 4
            )
        };

        _closeButton = new GUIStyle(skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,

            fixedWidth = 28f,
            fixedHeight = 24f,

            padding = CreateRectOffset(
                left: 0,
                right: 0,
                top: 0,
                bottom: 0
            )
        };
    }

    private static RectOffset CreateRectOffset(
        int left,
        int right,
        int top,
        int bottom)
    {
        return new RectOffset
        {
            left = left,
            right = right,
            top = top,
            bottom = bottom
        };
    }

    private static void SetTextColorForAllStates(
        GUIStyle style,
        Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;

        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }

    public static void Reset()
    {
        _sourceSkin = null;

        _separator = null;
        _normalButton = null;
        _normalToggle = null;
        _indentedToggle = null;

        _tabButton = null;
        _tabTitle = null;
        _tabSubtitle = null;

        _accordionHeader = null;
        _accordionHeaderExpanded = null;
        _accordionContent = null;

        _menuHeader = null;
        _closeButton = null;
    }
}