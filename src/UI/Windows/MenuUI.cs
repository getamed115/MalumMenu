//using System.Collections.Generic;
//using UnityEngine;

//namespace MalumMenu;

//public class MenuUI : MonoBehaviour
//{
//    public const int WindowWidth = 390;

//    /*
//     * Visible height of each accordion button.
//     */
//    private const float AccordionHeaderHeight = 42f;

//    /*
//     * The accordion style has 2 px above and 2 px below each button.
//     *
//     * Therefore, each collapsed row occupies:
//     *
//     *     42 + 2 + 2 = 46 px
//     */
//    private const float AccordionHeaderSlotHeight = 46f;

//    private const float MenuHeaderHeight = 30f;

//    /*
//     * Includes the GUI.Window border, GUILayout spacing and outer padding.
//     * This prevents the bottom title bar from being clipped.
//     */
//    private const float WindowChromeHeight = 28f;

//    /*
//     * Additional height occupied by the expanded content box:
//     * padding, margin and scroll-view border.
//     */
//    private const float ExpandedContentChromeHeight = 30f;

//    private const float ScreenMargin = 12f;

//    private readonly List<ITab> _tabs = new();
//    private readonly Dictionary<int, Vector2> _scrollPositions = new();

//    private int _expandedTab = -1;
//    private Rect _windowRect;

//    public static bool isGUIActive = false;
//    public static float hue;

//    private int _lastScreenWidth;
//    private int _lastScreenHeight;

//    private void RegisterTabs()
//    {
//        _tabs.Clear();

//        _tabs.Add(new ESPTab());
//        _tabs.Add(new ChatTab());

//#if DEBUG
//        _tabs.Add(new HostOnlyTab());
//#endif
//        _tabs.Add(new ModesTab());
//        _tabs.Add(new ConfigTab());

//        for (int index = 0; index < _tabs.Count; index++)
//        {
//            _scrollPositions[index] = Vector2.zero;
//        }
//    }

//    private void Start()
//    {
//        RegisterTabs();

//        _lastScreenWidth = Screen.width;
//        _lastScreenHeight = Screen.height;

//        float initialHeight = CalculateWindowHeight();

//        _windowRect = new Rect(
//            Screen.width / 2f - WindowWidth / 2f,
//            Screen.height / 2f - initialHeight / 2f,
//            WindowWidth,
//            initialHeight
//        );

//        KeepWindowOnScreen();
//    }

//    private void Update()
//    {
//        /*
//         * Handle the principal menu keybind here.
//         *
//         * KeybindListener handles feature keybinds, not the main menu.
//         */
//        if (!MalumMenu.isPanicked)
//        {
//            KeyCode menuKey = Utils.StringToKeycode(
//                MalumMenu.menuKeybind.Value
//            );

//            if (menuKey != KeyCode.None &&
//                Input.GetKeyDown(menuKey))
//            {
//                isGUIActive = !isGUIActive;

//                if (isGUIActive &&
//                    MalumMenu.menuOpenOnMouse.Value)
//                {
//                    MoveToMousePosition();
//                }
//            }
//        }

//        /*
//         * Recalculate the menu when the game resolution changes.
//         */
//        if (_lastScreenWidth != Screen.width ||
//            _lastScreenHeight != Screen.height)
//        {
//            _lastScreenWidth = Screen.width;
//            _lastScreenHeight = Screen.height;

//            UpdateWindowHeight();
//            KeepWindowOnScreen();
//        }

//        //CheatToggles.noShadows = Utils.isLobby || (Utils.isInGame && PlayerControl.LocalPlayer?.Data != null && (PlayerControl.LocalPlayer.Data.IsDead || Utils.isImpostor));

//        if (CheatToggles.panicMode) Utils.Panic();
//    }

//    private void OnGUI()
//    {
//        if (!isGUIActive || MalumMenu.isPanicked)
//        {
//            return;
//        }

//        UIHelpers.ApplyUIColor();

//        UpdateWindowHeight();

//        _windowRect = GUI.Window(
//            (int)WindowId.MenuUI,
//            _windowRect,
//            (GUI.WindowFunction)DrawWindow,
//            string.Empty
//        );

//        KeepWindowOnScreen();
//    }

//    /// <summary>
//    /// Draws the complete vertical menu.
//    /// </summary>
//    private void DrawWindow(int windowId)
//    {
//        GUILayout.BeginVertical();

//        DrawMenuHeader();

//        for (int index = 0; index < _tabs.Count; index++)
//        {
//            DrawAccordionTab(index);
//        }

//        GUILayout.EndVertical();

//        /*
//         * Only the upper row can drag the menu.
//         *
//         * Without an explicit rectangle, GUI.DragWindow() can interfere with
//         * tab buttons, toggles, sliders and scroll views.
//         */
//        GUI.DragWindow(
//            new Rect(
//                0f,
//                0f,
//                _windowRect.width - 36f,
//                MenuHeaderHeight + 8f
//            )
//        );
//    }

//    /// <summary>
//    /// Draws the fixed header at the top of the accordion.
//    /// </summary>
//    private void DrawMenuHeader()
//    {
//        GUILayout.BeginHorizontal(
//            GUILayout.Height(MenuHeaderHeight)
//        );

//        GUILayout.Label(
//            $"MalumMenu v{MalumMenu.malumVersion}",
//            GUIStylePreset.MenuHeader,
//            GUILayout.Height(MenuHeaderHeight)
//        );

//        GUILayout.FlexibleSpace();

//        if (GUILayout.Button(
//            "×",
//            GUIStylePreset.CloseButton,
//            GUILayout.Width(28f),
//            GUILayout.Height(24f)))
//        {
//            isGUIActive = false;
//        }

//        GUILayout.EndHorizontal();
//    }

//    /// <summary>
//    /// Draws one accordion title and, when selected, the corresponding
//    /// tab content directly underneath.
//    /// </summary>
//    private void DrawAccordionTab(int index)
//    {
//        if (index < 0 || index >= _tabs.Count)
//        {
//            return;
//        }

//        ITab tab = _tabs[index];
//        bool isExpanded = _expandedTab == index;

//        string indicator = isExpanded ? "▼" : "▶";
//        string title = $"{indicator}  {tab.Name}";

//        GUIStyle headerStyle = isExpanded
//            ? GUIStylePreset.AccordionHeaderExpanded
//            : GUIStylePreset.AccordionHeader;

//        if (GUILayout.Button(
//            title,
//            headerStyle,
//            GUILayout.ExpandWidth(true),
//            GUILayout.Height(AccordionHeaderHeight)))
//        {
//            ToggleTab(index);
//        }

//        if (isExpanded)
//        {
//            DrawExpandedTab(index, tab);
//        }
//    }

//    /// <summary>
//    /// Draws the controls belonging to the selected tab.
//    /// </summary>
//    private void DrawExpandedTab(int index, ITab tab)
//    {
//        float contentHeight = CalculateContentHeight();

//        GUILayout.BeginVertical(
//            GUIStylePreset.AccordionContent,
//            GUILayout.ExpandWidth(true)
//        );

//        Vector2 scrollPosition;

//        if (!_scrollPositions.TryGetValue(index, out scrollPosition))
//        {
//            scrollPosition = Vector2.zero;
//        }

//        bool scrollViewStarted = false;

//        try
//        {
//            scrollPosition = GUILayout.BeginScrollView(
//                scrollPosition,
//                false,
//                true,
//                GUILayout.Height(contentHeight),
//                GUILayout.ExpandWidth(true)
//            );

//            scrollViewStarted = true;

//            tab.Draw();
//        }
//        catch (System.Exception exception)
//        {
//            Debug.LogError(
//                $"[MalumMenu] Error while drawing tab " +
//                $"'{tab.Name}' during scene " +
//                $"'{UnityEngine.SceneManagement.SceneManager.GetActiveScene().Name}':\n" +
//                exception
//            );

//            GUILayout.Space(8f);

//            GUILayout.Label(
//                $"Unable to draw the {tab.Name} tab in the current game state.",
//                GUI.skin.label
//            );
//        }
//        finally
//        {
//            if (scrollViewStarted)
//            {
//                GUILayout.EndScrollView();
//            }

//            _scrollPositions[index] = scrollPosition;

//            GUILayout.EndVertical();
//        }
//    }

//    /// <summary>
//    /// Opens the selected tab or collapses it when its title is clicked a
//    /// second time.
//    /// </summary>
//    private void ToggleTab(int index)
//    {
//        if (_expandedTab == index)
//        {
//            _expandedTab = -1;
//        }
//        else
//        {
//            _expandedTab = index;

//            if (!_scrollPositions.ContainsKey(index))
//            {
//                _scrollPositions[index] = Vector2.zero;
//            }
//        }

//        UpdateWindowHeight();
//        KeepWindowOnScreen();
//    }

//    /// <summary>
//    /// Returns the vertical space available for the open tab.
//    /// </summary>
//    private float CalculateContentHeight()
//    {
//        if (_expandedTab < 0 || _expandedTab >= _tabs.Count)
//        {
//            return 0f;
//        }

//        ITab activeTab = _tabs[_expandedTab];

//        /*
//         * These values describe the preferred visible height of each tab.
//         *
//         * Small tabs receive only the space they need.
//         * Larger tabs receive a scrollable viewport.
//         */
//        float preferredHeight;

//        switch (activeTab.Name)
//        {
//            case "Chat":
//                preferredHeight = 50f;
//                break;

//            case "Modes":
//                preferredHeight = 82f;
//                break;

//            case "Config":
//                preferredHeight = 150f;
//                break;

//            case "Console":
//                preferredHeight = 82f;
//                break;

//            case "Overload":
//                preferredHeight = 180f;
//                break;

//            case "Animations":
//                preferredHeight = 220f;
//                break;

//            case "Passive":
//                preferredHeight = 220f;
//                break;

//            case "Host-Only":
//                preferredHeight = 300f;
//                break;

//            case "Movement":
//                preferredHeight = 340f;
//                break;

//            case "Roles":
//                preferredHeight = 360f;
//                break;

//            case "Ship":
//                preferredHeight = 380f;
//                break;

//            case "ESP":
//                preferredHeight = 420f;
//                break;

//            default:
//                preferredHeight = 200f;
//                break;
//        }

//        /*
//         * Calculate how much vertical room remains after drawing the title,
//         * every accordion header and the window borders.
//         */
//        float reservedHeight =
//            MenuHeaderHeight +
//            _tabs.Count * AccordionHeaderSlotHeight +
//            WindowChromeHeight +
//            ExpandedContentChromeHeight +
//            ScreenMargin * 2f;

//        float availableHeight = Screen.height - reservedHeight;

//        /*
//         * Never return a negative or unusably small scroll-view height.
//         *
//         * If the screen is short, larger tabs will scroll.
//         */
//        float maximumHeight = Mathf.Max(50f, availableHeight);

//        return Mathf.Min(preferredHeight, maximumHeight);
//    }

//    /// <summary>
//    /// Calculates the complete outer window height.
//    /// </summary>
//    private float CalculateWindowHeight()
//    {
//        /*
//         * Base height when every tab is collapsed.
//         *
//         * The calculation uses AccordionHeaderSlotHeight rather than only the
//         * visible button height because the GUIStyle has top and bottom margins.
//         */
//        float height =
//            MenuHeaderHeight +
//            _tabs.Count * AccordionHeaderSlotHeight +
//            WindowChromeHeight;

//        if (_expandedTab >= 0 && _expandedTab < _tabs.Count)
//        {
//            height +=
//                CalculateContentHeight() +
//                ExpandedContentChromeHeight;
//        }

//        float maximumHeight = Mathf.Max(
//            200f,
//            Screen.height - ScreenMargin * 2f
//        );

//        return Mathf.Min(height, maximumHeight);
//    }

//    /// <summary>
//    /// Updates the current rectangle without changing its top-left position.
//    /// </summary>
//    private void UpdateWindowHeight()
//    {
//        float oldHeight = _windowRect.height;
//        float newHeight = CalculateWindowHeight();

//        if (Mathf.Approximately(oldHeight, newHeight))
//        {
//            return;
//        }

//        _windowRect.height = newHeight;
//    }

//    /// <summary>
//    /// Prevents the menu from extending beyond the current screen.
//    /// </summary>
//    private void KeepWindowOnScreen()
//    {
//        float maximumX = Mathf.Max(
//            ScreenMargin,
//            Screen.width - _windowRect.width - ScreenMargin
//        );

//        float maximumY = Mathf.Max(
//            ScreenMargin,
//            Screen.height - _windowRect.height - ScreenMargin
//        );

//        _windowRect.x = Mathf.Clamp(
//            _windowRect.x,
//            ScreenMargin,
//            maximumX
//        );

//        _windowRect.y = Mathf.Clamp(
//            _windowRect.y,
//            ScreenMargin,
//            maximumY
//        );
//    }

//    /// <summary>
//    /// Allows another component to open a specific tab by index.
//    /// </summary>
//    public void OpenTab(int index)
//    {
//        if (index < 0 || index >= _tabs.Count)
//        {
//            return;
//        }

//        _expandedTab = index;

//        UpdateWindowHeight();
//        KeepWindowOnScreen();
//    }

//    /// <summary>
//    /// Collapses the currently expanded tab.
//    /// </summary>
//    public void CollapseCurrentTab()
//    {
//        _expandedTab = -1;

//        UpdateWindowHeight();
//        KeepWindowOnScreen();
//    }

//    /// <summary>
//    /// Moves the menu so its top-left area is near the current mouse
//    /// position. This can be called by the existing keybind listener when
//    /// OpenOnMouse is enabled.
//    /// </summary>
//    public void MoveToMousePosition()
//    {
//        Vector3 mousePosition = Input.mousePosition;

//        /*
//         * Unity's input coordinates start at the bottom-left, while GUI
//         * coordinates start at the top-left.
//         */
//        _windowRect.x = mousePosition.x - _windowRect.width / 2f;
//        _windowRect.y =
//            Screen.height -
//            mousePosition.y -
//            MenuHeaderHeight / 2f;

//        KeepWindowOnScreen();
//    }
//}*/


using MalumMenu.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public const int WindowWidth = 390;

    // UI Layout Constants
    private const float AccordionHeaderHeight = 42f;
    private const float AccordionHeaderSlotHeight = 46f;
    private const float MenuHeaderHeight = 30f;
    private const float WindowChromeHeight = 28f;
    private const float ExpandedContentChromeHeight = 30f;
    private const float ScreenMargin = 12f;

    // Collections simplified to arrays for better memory/speed performance
    private ITab[] _tabs;
    private Vector2[] _scrollPositions;

    private int _expandedTab = -1;
    private Rect _windowRect;

    public static bool isGUIActive = false;
    public static float hue;

    private int _lastScreenWidth;
    private int _lastScreenHeight;

    // Cached Keycode to prevent string parsing every frame
    private string _lastMenuKeyString;
    private KeyCode _cachedMenuKey = KeyCode.None;

    private void Start()
    {
        RegisterTabs();

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        float initialHeight = CalculateWindowHeight();

        _windowRect = new Rect(
            Screen.width / 2f - WindowWidth / 2f,
            Screen.height / 2f - initialHeight / 2f,
            WindowWidth,
            initialHeight
        );

        KeepWindowOnScreen();
    }

    private void RegisterTabs()
    {
        var tabList = new List<ITab>
        {
            new ESPTab(),
            new ChatTab(),
#if DEBUG
            new HostOnlyTab(),
#endif
            new ModesTab(),
            new ConfigTab()
        };

        // Convert to array so we don't need a Dictionary for scroll positions
        _tabs = tabList.ToArray();
        _scrollPositions = new Vector2[_tabs.Length];
    }

    private void Update()
    {
        if (!MalumMenu.isPanicked)
        {
            KeyCode menuKey = GetMenuKey();

            if (menuKey != KeyCode.None && Input.GetKeyDown(menuKey))
            {
                isGUIActive = !isGUIActive;

                if (isGUIActive && MalumMenu.menuOpenOnMouse.Value)
                {
                    MoveToMousePosition();
                }
            }
        }

        if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            UpdateWindowHeight();
            KeepWindowOnScreen();
        }

        if (CheatToggles.panicMode) PanicUtils.Panic();
    }

    private void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        UIHelpers.ApplyUIColor();
        UpdateWindowHeight();

        _windowRect = GUI.Window(
            (int)WindowId.MenuUI,
            _windowRect,
            (GUI.WindowFunction)DrawWindow,
            string.Empty
        );

        KeepWindowOnScreen();
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();
        DrawMenuHeader();

        for (int i = 0; i < _tabs.Length; i++)
        {
            DrawAccordionTab(i);
        }

        GUILayout.EndVertical();

        // Dragable header area
        GUI.DragWindow(new Rect(0f, 0f, _windowRect.width - 36f, MenuHeaderHeight + 8f));
    }

    private void DrawMenuHeader()
    {
        GUILayout.BeginHorizontal(GUILayout.Height(MenuHeaderHeight));

        GUILayout.Label($"MalumMenu v{MalumMenu.malumVersion}", GUIStylePreset.MenuHeader, GUILayout.Height(MenuHeaderHeight));
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("×", GUIStylePreset.CloseButton, GUILayout.Width(28f), GUILayout.Height(24f)))
        {
            isGUIActive = false;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawAccordionTab(int index)
    {
        if (index < 0 || index >= _tabs.Length) return;

        ITab tab = _tabs[index];
        bool isExpanded = _expandedTab == index;

        string title = $"{(isExpanded ? "▼" : "▶")}  {tab.Name}";
        GUIStyle headerStyle = isExpanded ? GUIStylePreset.AccordionHeaderExpanded : GUIStylePreset.AccordionHeader;

        if (GUILayout.Button(title, headerStyle, GUILayout.ExpandWidth(true), GUILayout.Height(AccordionHeaderHeight)))
        {
            ToggleTab(index);
        }

        if (isExpanded)
        {
            DrawExpandedTab(index, tab);
        }
    }

    private void DrawExpandedTab(int index, ITab tab)
    {
        float contentHeight = CalculateContentHeight();

        GUILayout.BeginVertical(GUIStylePreset.AccordionContent, GUILayout.ExpandWidth(true));

        bool scrollViewStarted = false;

        try
        {
            _scrollPositions[index] = GUILayout.BeginScrollView(
                _scrollPositions[index],
                false,
                true,
                GUILayout.Height(contentHeight),
                GUILayout.ExpandWidth(true)
            );

            scrollViewStarted = true;
            tab.Draw();
        }
        catch (Exception exception)
        {
            // Using a single formatted string instead of concatenating (+) for better performance and readability
            Debug.LogError($"[MalumMenu] Error drawing tab '{tab.Name}' in scene '{SceneManager.GetActiveScene().name}':\n{exception}");

            GUILayout.Space(8f);
            GUILayout.Label($"Unable to draw the {tab.Name} tab in the current game state.", GUI.skin.label);
        }
        finally
        {
            if (scrollViewStarted)
            {
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
        }
    }

    private void ToggleTab(int index)
    {
        _expandedTab = (_expandedTab == index) ? -1 : index;

        if (_expandedTab != -1)
        {
            _scrollPositions[index] = Vector2.zero;
        }

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    private float CalculateContentHeight()
    {
        if (_expandedTab < 0 || _expandedTab >= _tabs.Length) return 0f;

        // Using a modern C# switch expression condenses 40 lines into 15
        float preferredHeight = _tabs[_expandedTab].Name switch
        {
            "Chat" => 50f,
            "Modes" => 82f,
            "Config" => 150f,
            "Roles" => 360f,
            "ESP" => 420f,
            _ => 200f
        };

        float reservedHeight = MenuHeaderHeight +
                               (_tabs.Length * AccordionHeaderSlotHeight) +
                               WindowChromeHeight +
                               ExpandedContentChromeHeight +
                               (ScreenMargin * 2f);

        return Mathf.Min(preferredHeight, Mathf.Max(50f, Screen.height - reservedHeight));
    }

    private float CalculateWindowHeight()
    {
        float height = MenuHeaderHeight + (_tabs.Length * AccordionHeaderSlotHeight) + WindowChromeHeight;

        if (_expandedTab >= 0 && _expandedTab < _tabs.Length)
        {
            height += CalculateContentHeight() + ExpandedContentChromeHeight;
        }

        return Mathf.Min(height, Mathf.Max(200f, Screen.height - ScreenMargin * 2f));
    }

    private void UpdateWindowHeight()
    {
        float newHeight = CalculateWindowHeight();
        if (!Mathf.Approximately(_windowRect.height, newHeight))
        {
            _windowRect.height = newHeight;
        }
    }

    private void KeepWindowOnScreen()
    {
        _windowRect.x = Mathf.Clamp(_windowRect.x, ScreenMargin, Mathf.Max(ScreenMargin, Screen.width - _windowRect.width - ScreenMargin));
        _windowRect.y = Mathf.Clamp(_windowRect.y, ScreenMargin, Mathf.Max(ScreenMargin, Screen.height - _windowRect.height - ScreenMargin));
    }

    public void OpenTab(int index)
    {
        if (index < 0 || index >= _tabs.Length) return;

        _expandedTab = index;
        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void CollapseCurrentTab()
    {
        _expandedTab = -1;
        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void MoveToMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        _windowRect.x = mousePos.x - _windowRect.width / 2f;
        _windowRect.y = Screen.height - mousePos.y - MenuHeaderHeight / 2f;
        KeepWindowOnScreen();
    }

    // Evaluates string configuration exactly once, caching it until the user changes it in the config file.
    private KeyCode GetMenuKey()
    {
        string currentConfigValue = MalumMenu.menuKeybind.Value;

        if (_lastMenuKeyString != currentConfigValue)
        {
            _lastMenuKeyString = currentConfigValue;
            _cachedMenuKey = Utils.StringToKeycode(currentConfigValue);
        }

        return _cachedMenuKey;
    }
}