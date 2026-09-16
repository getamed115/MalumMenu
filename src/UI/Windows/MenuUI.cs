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
    private const float AccordionTabSpacing = 6f;
    private const float AccordionHeaderSlotHeight = AccordionHeaderHeight + AccordionTabSpacing;

    private const float MenuHeaderHeight = 30f;

    // Spazio tra il titolo del menu e la prima tab ESP
    private const float HeaderBottomSpacing = 9f;

    // Spazio dopo l’ultima tab Config
    private const float AccordionBottomSpacing = 7f;

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
            bool alwaysShowVerticalScrollbar = tab.Name == "ESP";

            _scrollPositions[index] = GUILayout.BeginScrollView(
                _scrollPositions[index],
                false,
                alwaysShowVerticalScrollbar
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

        bool isExpanded = _expandedTab == index;
        _expandedTab = isExpanded ? -1 : index;

        if (!isExpanded)
            _scrollPositions[index] = Vector2.zero;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    private float CalculateContentHeight()
    {
        if (_expandedTab < 0 || _expandedTab >= _tabs.Length)
        {
            return 0f;
        }

        float preferredHeight = _tabs[_expandedTab].Name switch
        {
            "Chat" => 34f,
            "Modes" => 102f,
            "Config" => 131f,
            "Roles" => 360f,
            "ESP" => 420f,
            _ => 200f,
        };

        float reservedHeight =
            MenuHeaderHeight
            + HeaderBottomSpacing
            + (_tabs.Length * AccordionHeaderSlotHeight)
            + AccordionBottomSpacing
            + ExpandedContentChromeHeight
            + ScreenMargin * 2f;

        float availableHeight = Mathf.Max(50f, Screen.height - reservedHeight);

        return Mathf.Min(preferredHeight, availableHeight);
    }

    private float CalculateWindowHeight()
    {
        float height =
            MenuHeaderHeight
            + HeaderBottomSpacing
            + (_tabs.Length * AccordionHeaderSlotHeight)
            + AccordionBottomSpacing;

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
        if ((uint)index >= (uint)_tabs.Length || (uint)index >= (uint)_scrollPositions.Length)
        {
            return;
        }

        _expandedTab = index;
        _scrollPositions[index] = Vector2.zero;

        UpdateWindowHeight();
        KeepWindowOnScreen();
    }

    public void OpenTab<T>()
        where T : class, ITab
    {
        if (Array.FindIndex(_tabs, tab => tab is T) is >= 0 and var index)
            OpenTab(index);
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
