using UnityEngine;

namespace MalumMenu;

public static class GUIStylePreset
{
    private static GUIStyle _separator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;

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
            if (_separator == null)
            {
                _separator = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        background = Texture2D.whiteTexture
                    },

                    margin = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 4,
                        bottom = 4
                    },

                    padding = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    },

                    border = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    },

                    fixedHeight = 1f
                };
            }

            return _separator;
        }
    }

    public static GUIStyle NormalButton
    {
        get
        {
            if (_normalButton == null)
            {
                _normalButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 17,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 34f,

                    padding = new RectOffset
                    {
                        left = 10,
                        right = 10,
                        top = 4,
                        bottom = 4
                    }
                };
            }

            return _normalButton;
        }
    }

    public static GUIStyle NormalToggle
    {
        get
        {
            if (_normalToggle == null)
            {
                _normalToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 17,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 30f,

                    padding = new RectOffset
                    {
                        left = 22,
                        right = 8,
                        top = 3,
                        bottom = 3
                    }
                };
            }

            return _normalToggle;
        }
    }

    /*
     * Retained for compatibility with any existing code that still uses
     * the original tab styles.
     */
    public static GUIStyle TabButton
    {
        get
        {
            if (_tabButton == null)
            {
                _tabButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold
                };
            }

            return _tabButton;
        }
    }

    public static GUIStyle TabTitle
    {
        get
        {
            if (_tabTitle == null)
            {
                _tabTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 32f
                };
            }

            return _tabTitle;
        }
    }

    private static GUIStyle _indentedToggle;

    public static GUIStyle IndentedToggle
    {
        get
        {
            if (_indentedToggle == null)
            {
                _indentedToggle = new GUIStyle(NormalToggle)
                {
                    fontSize = 17,

                    padding = new RectOffset
                    {
                        left = 34,
                        right = 8,
                        top = 3,
                        bottom = 3
                    }
                };
            }

            return _indentedToggle;
        }
    }

    public static GUIStyle TabSubtitle
    {
        get
        {
            if (_tabSubtitle == null)
            {
                _tabSubtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 19,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 30f,

                    margin = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 8,
                        bottom = 2
                    }
                };
            }

            return _tabSubtitle;
        }
    }

    public static GUIStyle MenuHeader
    {
        get
        {
            if (_menuHeader == null)
            {
                _menuHeader = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,

                    padding = new RectOffset
                    {
                        left = 8,
                        right = 8,
                        top = 0,
                        bottom = 0
                    }
                };
            }

            return _menuHeader;
        }
    }

    public static GUIStyle AccordionHeader
    {
        get
        {
            if (_accordionHeader == null)
            {
                _accordionHeader = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = 42f,

                    padding = new RectOffset
                    {
                        left = 14,
                        right = 12,
                        top = 0,
                        bottom = 0
                    },

                    margin = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 2,
                        bottom = 2
                    },

                    wordWrap = false
                };
            }

            return _accordionHeader;
        }
    }

    public static GUIStyle AccordionHeaderExpanded
    {
        get
        {
            if (_accordionHeaderExpanded == null)
            {
                _accordionHeaderExpanded =
                    new GUIStyle(AccordionHeader)
                    {
                        fontStyle = FontStyle.Bold
                    };

                _accordionHeaderExpanded.normal.textColor = Color.white;
                _accordionHeaderExpanded.hover.textColor = Color.white;
                _accordionHeaderExpanded.active.textColor = Color.white;
                _accordionHeaderExpanded.focused.textColor = Color.white;
            }

            return _accordionHeaderExpanded;
        }
    }

    public static GUIStyle AccordionContent
    {
        get
        {
            if (_accordionContent == null)
            {
                _accordionContent = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset
                    {
                        left = 12,
                        right = 12,
                        top = 12,
                        bottom = 12
                    },

                    margin = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 4
                    }
                };
            }

            return _accordionContent;
        }
    }

    public static GUIStyle CloseButton
    {
        get
        {
            if (_closeButton == null)
            {
                _closeButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = 28f,
                    fixedHeight = 24f,

                    padding = new RectOffset
                    {
                        left = 0,
                        right = 0,
                        top = 0,
                        bottom = 0
                    }
                };
            }

            return _closeButton;
        }
    }

    public static void Reset()
    {
        _separator = null;
        _normalButton = null;
        _normalToggle = null;

        _tabButton = null;
        _tabTitle = null;
        _tabSubtitle = null;

        _accordionHeader = null;
        _accordionHeaderExpanded = null;
        _accordionContent = null;
        _menuHeader = null;
        _closeButton = null;
        _indentedToggle = null;
    }
}
