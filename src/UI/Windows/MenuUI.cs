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
