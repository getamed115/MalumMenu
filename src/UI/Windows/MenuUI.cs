/*using MalumMenu.Utilities;
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

        if (CheatState.panicMode) PanicUtils.Panic();
        if (CheatState.fakePanicMode) PanicUtils.FakePanic();
    }

    /*private void OnGUI()
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
    }*/

/*private bool _firstRender = true;
private bool _warmupLayoutCompleted;

private void OnGUI()
{
    if (MalumMenu.isPanicked)
        return;

    // Render the menu invisibly once during startup.
    if (_firstRender)
    {
        WarmUpGUI();
        return;
    }

    // Normal behavior after the warm-up.
    if (!isGUIActive)
        return;

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

private void WarmUpGUI()
{
    UIHelpers.ApplyUIColor();
    UpdateWindowHeight();

    // Position used only for the invisible warm-up.
    Rect warmupRect = new Rect(
        -10000f,
        -10000f,
        _windowRect.width,
        _windowRect.height
    );

    bool previousGUIEnabled = GUI.enabled;
    Color previousGUIColor = GUI.color;

    try
    {
        // Disable all interactive controls during warm-up.
        GUI.enabled = false;

        // Make the window transparent as an additional precaution.
        GUI.color = new Color(
            previousGUIColor.r,
            previousGUIColor.g,
            previousGUIColor.b,
            0f
        );

        GUI.Window(
            (int)WindowId.MenuUI,
            warmupRect,
            (GUI.WindowFunction)DrawWindow,
            string.Empty
        );

        // Unity normally performs Layout before Repaint.
        if (Event.current.type == EventType.Layout)
        {
            _warmupLayoutCompleted = true;
        }
        else if (
            Event.current.type == EventType.Repaint &&
            _warmupLayoutCompleted)
        {
            _firstRender = false;

            MalumMenu.Log.LogInfo(
                "Menu GUI warm-up completed."
            );
        }
    }
    finally
    {
        GUI.enabled = previousGUIEnabled;
        GUI.color = previousGUIColor;
    }
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
*/

#pragma warning disable IDE0051

using System;
using System.Collections.Generic;
using MalumMenu.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public const int WindowWidth = 390;

    private const float AccordionHeaderHeight = 42f;
    private const float AccordionHeaderSlotHeight = 46f;

    private const float MenuHeaderHeight = 30f;
    private const float HeaderBottomSpacing = 6f;

    private const float ExpandedContentChromeHeight = 30f;
    private const float ScreenMargin = 12f;

    private const float HorizontalPadding = 8f;
    private const float ContentPadding = 8f;

    // Tabs
    private ITab[] _tabs = Array.Empty<ITab>();
    private Vector2[] _scrollPositions = Array.Empty<Vector2>();

    private int _expandedTab = -1;

    // Window
    private Rect _windowRect;
    private bool _isDragging;
    private Vector2 _dragOffset;

    public static bool isGUIActive;
    public static float hue;

    // Resolution tracking
    private int _lastScreenWidth;
    private int _lastScreenHeight;

    // Cached menu key
    private string _lastMenuKeyString;
    private KeyCode _cachedMenuKey = KeyCode.None;

    // Safe GUI chrome warm-up
    private bool _warmupComplete;
    private bool _warmupLayoutCompleted;

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
            new ModesTab(),
            new ConfigTab(),
            //new LobbySettingsTab()
        };

        _tabs = tabList.ToArray();
        _scrollPositions = new Vector2[_tabs.Length];
    }

    private void Update()
    {
        HandleMenuKey();
        HandleResolutionChange();
        HandlePanicModes();
    }

    private void HandleMenuKey()
    {
        if (MalumMenu.isPanicked)
            return;

        KeyCode menuKey = GetMenuKey();

        if (menuKey == KeyCode.None)
            return;

        if (!Input.GetKeyDown(menuKey))
            return;

        isGUIActive = !isGUIActive;

        // Stop dragging if the menu is closed while dragging.
        if (!isGUIActive)
        {
            _isDragging = false;
            return;
        }

        if (MalumMenu.menuOpenOnMouse.Value)
        {
            MoveToMousePosition();
        }
    }

    private void HandleResolutionChange()
    {
        if (_lastScreenWidth == Screen.width && _lastScreenHeight == Screen.height)
        {
            return;
        }

        _lastScreenWidth = Screen.width;
        _lastScreenHeight = Screen.height;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    private static void HandlePanicModes()
    {
        if (CheatState.panicMode)
            PanicUtils.Panic();
        else if (CheatState.fakePanicMode)
            PanicUtils.FakePanic();
    }

    private void OnGUI()
    {
        if (MalumMenu.isPanicked)
            return;

        GUIStylePreset.EnsureInitialized();

        /*
         * Warm only the menu chrome and GUI styles.
         *
         * The expanded tab content is not rendered during warm-up,
         * so tab.Draw() cannot execute game-related behavior at startup.
         */
        if (!_warmupComplete)
        {
            WarmUpGUI();
            return;
        }

        if (!isGUIActive)
            return;

        UIHelpers.ApplyUIColor();

        UpdateWindowHeight();
        KeepWindowOnScreen();

        DrawMenu(renderExpandedContent: true, allowInteraction: true);
    }

    private void WarmUpGUI()
    {
        GUIStylePreset.EnsureInitialized();

        if (_tabs.Length == 0)
            return;

        Rect savedWindowRect = _windowRect;
        Color savedGUIColor = GUI.color;
        Color savedBackgroundColor = GUI.backgroundColor;
        Color savedContentColor = GUI.contentColor;
        bool savedGUIEnabled = GUI.enabled;

        try
        {
            UpdateWindowHeight();

            // Place the temporary warm-up window far outside the screen.
            _windowRect = new Rect(-10000f, -10000f, WindowWidth, CalculateWindowHeight());

            // Disable interaction and make the warm-up fully transparent.
            GUI.enabled = false;

            GUI.color = new Color(savedGUIColor.r, savedGUIColor.g, savedGUIColor.b, 0f);

            GUI.backgroundColor = new Color(
                savedBackgroundColor.r,
                savedBackgroundColor.g,
                savedBackgroundColor.b,
                0f
            );

            GUI.contentColor = new Color(
                savedContentColor.r,
                savedContentColor.g,
                savedContentColor.b,
                0f
            );

            UIHelpers.ApplyUIColor();

            /*
             * Draw the background, header and accordion headers,
             * but do not execute the expanded tab's Draw() method.
             */
            DrawMenu(renderExpandedContent: false, allowInteraction: false);


            switch (Event.current.type)
            {
                case EventType.Layout:
                    _warmupLayoutCompleted = true;
                    break;
                case EventType.Repaint when _warmupLayoutCompleted:
                    _warmupComplete = true;
                    break;
            }
        }
        catch (Exception)
        {
            /*
             * Do not permanently block the real menu if the warm-up
             * fails for an unexpected reason.
             */
            _warmupComplete = true;
        }
        finally
        {
            GUI.enabled = savedGUIEnabled;
            GUI.color = savedGUIColor;
            GUI.backgroundColor = savedBackgroundColor;
            GUI.contentColor = savedContentColor;

            _windowRect = savedWindowRect;
        }
    }

    private void DrawMenu(bool renderExpandedContent, bool allowInteraction)
    {
        GUI.Box(_windowRect, GUIContent.none, GUI.skin.window);

        DrawMenuHeader(allowInteraction);

        float currentY = _windowRect.y + MenuHeaderHeight + HeaderBottomSpacing;

        for (int index = 0; index < _tabs.Length; index++)
        {
            DrawAccordionTab(index, ref currentY, renderExpandedContent, allowInteraction);
        }

        if (allowInteraction)
        {
            HandleWindowDragging();
        }
    }

    private void DrawMenuHeader(bool allowInteraction)
    {
        Rect headerRect = new Rect(
            _windowRect.x + HorizontalPadding,
            _windowRect.y + 4f,
            _windowRect.width - HorizontalPadding * 2f,
            MenuHeaderHeight
        );

        Rect closeButtonRect = new Rect(_windowRect.xMax - 36f, _windowRect.y + 5f, 28f, 24f);

        Rect titleRect = new Rect(
            headerRect.x,
            headerRect.y,
            headerRect.width - 36f,
            headerRect.height
        );

        GUI.Label(titleRect, $"MalumMenu v{MalumMenu.malumVersion}", GUIStylePreset.MenuHeader);

        if (!allowInteraction)
        {
            GUI.Button(closeButtonRect, "×", GUIStylePreset.CloseButton);

            return;
        }

        if (GUI.Button(closeButtonRect, "×", GUIStylePreset.CloseButton))
        {
            isGUIActive = false;
            _isDragging = false;
        }
    }

    private void DrawAccordionTab(
        int index,
        ref float currentY,
        bool renderExpandedContent,
        bool allowInteraction
    )
    {
        if (index < 0 || index >= _tabs.Length)
            return;

        ITab tab = _tabs[index];
        bool isExpanded = _expandedTab == index;

        Rect headerRect = new Rect(
            _windowRect.x + HorizontalPadding,
            currentY,
            _windowRect.width - HorizontalPadding * 2f,
            AccordionHeaderHeight
        );

        string indicator = isExpanded ? "▼" : "▶";
        string title = $"{indicator}  {tab.Name}";

        GUIStyle headerStyle = isExpanded
            ? GUIStylePreset.AccordionHeaderExpanded
            : GUIStylePreset.AccordionHeader;

        bool clicked = GUI.Button(headerRect, title, headerStyle);

        if (allowInteraction && clicked)
        {
            ToggleTab(index);
        }

        currentY += AccordionHeaderSlotHeight;

        if (!isExpanded || !renderExpandedContent)
            return;

        DrawExpandedTab(index, tab, ref currentY);
    }

    private void DrawExpandedTab(int index, ITab tab, ref float currentY)
    {
        float contentHeight = CalculateContentHeight();

        Rect contentRect = new Rect(
            _windowRect.x + HorizontalPadding,
            currentY,
            _windowRect.width - HorizontalPadding * 2f,
            contentHeight + ExpandedContentChromeHeight
        );

        GUI.Box(contentRect, GUIContent.none, GUIStylePreset.AccordionContent);

        Rect scrollAreaRect = new Rect(
            contentRect.x + ContentPadding,
            contentRect.y + ContentPadding,
            contentRect.width - ContentPadding * 2f,
            contentRect.height - ContentPadding * 2f
        );

        GUILayout.BeginArea(scrollAreaRect);

        bool scrollViewStarted = false;

        try
        {
            _scrollPositions[index] = GUILayout.BeginScrollView(
                _scrollPositions[index],
                false,
                true
            );

            scrollViewStarted = true;

            tab.Draw();
        }
        catch (Exception exception)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            Debug.LogError(
                $"[MalumMenu] Error drawing tab '{tab.Name}' "
                    + $"in scene '{sceneName}':\n{exception}"
            );

            GUILayout.Space(8f);

            GUILayout.Label(
                $"Unable to draw the {tab.Name} tab " + "in the current game state.",
                GUI.skin.label
            );
        }
        finally
        {
            if (scrollViewStarted)
            {
                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        currentY += contentHeight + ExpandedContentChromeHeight;
    }

    private void HandleWindowDragging()
    {
        Event currentEvent = Event.current;

        if (currentEvent == null)
            return;

        /*
         * Exclude the close-button area from the draggable region.
         */
        Rect dragArea = new Rect(
            _windowRect.x,
            _windowRect.y,
            _windowRect.width - 42f,
            MenuHeaderHeight + 8f
        );

        switch (currentEvent.type)
        {
            case EventType.MouseDown:
            {
                if (currentEvent.button != 0)
                    break;

                if (!dragArea.Contains(currentEvent.mousePosition))
                    break;

                _isDragging = true;

                _dragOffset = currentEvent.mousePosition - _windowRect.position;

                currentEvent.Use();
                break;
            }

            case EventType.MouseDrag:
            {
                if (!_isDragging)
                    break;

                _windowRect.position = currentEvent.mousePosition - _dragOffset;

                KeepWindowOnScreen();

                currentEvent.Use();
                break;
            }

            case EventType.MouseUp:
            {
                if (currentEvent.button != 0)
                    break;

                _isDragging = false;
                break;
            }

            case EventType.Ignore:
            case EventType.MouseLeaveWindow:
            {
                _isDragging = false;
                break;
            }
        }
    }

    private void ToggleTab(int index)
    {
        if (index < 0 || index >= _tabs.Length)
            return;

        if (_expandedTab == index)
        {
            _expandedTab = -1;
        }
        else
        {
            _expandedTab = index;
            _scrollPositions[index] = Vector2.zero;
        }

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    private float CalculateContentHeight()
    {
        if (_expandedTab < 0 || _expandedTab >= _tabs.Length)
        {
            return 0f;
        }

        /*float preferredHeight = _tabs[_expandedTab].Name switch
        {
            "Chat" => 21.5f,
            "Modes" => 100f,
            "Config" => 131.5f,
            "Roles" => 360f,
            "ESP" => 420f,
            "Lobby Settings" => 200f,
            _ => 200f,
        };*/

        float preferredHeight = _tabs[_expandedTab].Name switch
        {
            "Chat" => 50f,
            "Modes" => 82f,
            "Config" => 150f,
            "Roles" => 360f,
            "ESP" => 420f,
            _ => 200f
        };


        float reservedHeight =
            MenuHeaderHeight
            + HeaderBottomSpacing
            + (_tabs.Length * AccordionHeaderSlotHeight)
            + ExpandedContentChromeHeight
            + ScreenMargin * 2f;

        float availableHeight = Mathf.Max(50f, Screen.height - reservedHeight);

        return Mathf.Min(preferredHeight, availableHeight);
    }

    private float CalculateWindowHeight()
    {
        float height =
            MenuHeaderHeight + HeaderBottomSpacing + (_tabs.Length * AccordionHeaderSlotHeight);

        if (_expandedTab >= 0 && _expandedTab < _tabs.Length)
        {
            height += CalculateContentHeight() + ExpandedContentChromeHeight;
        }

        float maximumHeight = Mathf.Max(100f, Screen.height - ScreenMargin * 2f);

        return Mathf.Min(height, maximumHeight);
    }

    private void UpdateWindowHeight()
    {
        float newHeight = CalculateWindowHeight();

        if (!Mathf.Approximately(_windowRect.height, newHeight))
        {
            _windowRect.height = newHeight;
        }

        if (!Mathf.Approximately(_windowRect.width, WindowWidth))
        {
            _windowRect.width = WindowWidth;
        }
    }

    private void KeepWindowOnScreen()
    {
        float maximumX = Mathf.Max(ScreenMargin, Screen.width - _windowRect.width - ScreenMargin);

        float maximumY = Mathf.Max(ScreenMargin, Screen.height - _windowRect.height - ScreenMargin);

        _windowRect.x = Mathf.Clamp(_windowRect.x, ScreenMargin, maximumX);

        _windowRect.y = Mathf.Clamp(_windowRect.y, ScreenMargin, maximumY);
    }

    public void OpenTab(int index)
    {
        if (index < 0 || index >= _tabs.Length)
            return;

        _expandedTab = index;
        _scrollPositions[index] = Vector2.zero;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void OpenTab<T>()
        where T : class, ITab
    {
        for (int index = 0; index < _tabs.Length; index++)
        {
            if (_tabs[index] is not T)
                continue;

            OpenTab(index);
            return;
        }
    }

    public void CollapseCurrentTab()
    {
        _expandedTab = -1;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void OpenMenu()
    {
        if (MalumMenu.isPanicked)
            return;

        isGUIActive = true;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void CloseMenu()
    {
        isGUIActive = false;
        _isDragging = false;
    }

    public void ToggleMenu()
    {
        if (isGUIActive)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void MoveToMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        _windowRect.x = mousePosition.x - _windowRect.width / 2f;

        _windowRect.y = Screen.height - mousePosition.y - MenuHeaderHeight / 2f;

        KeepWindowOnScreen();
    }

    private KeyCode GetMenuKey()
    {
        string currentConfigValue = MalumMenu.menuKeybind.Value;

        if (string.Equals(_lastMenuKeyString, currentConfigValue, StringComparison.Ordinal))
        {
            return _cachedMenuKey;
        }

        _lastMenuKeyString = currentConfigValue;

        try
        {
            _cachedMenuKey = Utils.StringToKeycode(currentConfigValue);
        }
        catch (Exception exception)
        {
            _cachedMenuKey = KeyCode.None;

            MalumMenu.Log.LogWarning(
                $"Invalid menu key '{currentConfigValue}': " + exception.Message
            );
        }

        return _cachedMenuKey;
    }
}
