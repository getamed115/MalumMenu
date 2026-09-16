using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine.Analytics;

namespace MalumMenu;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class MalumMenu : BasePlugin
{
    public static MalumMenu Plugin { get; private set; }
    public static new ManualLogSource Log { get; private set; }
    public static MenuUI menuUI { get; private set; }

    public Harmony Harmony { get; } = new(Id);

    public const string malumVersion = "3.3.0";
    public static bool isPanicked = false;
    public static bool inStealthMode = true;

    public static ConfigEntry<string> menuKeybind;
    public static ConfigEntry<string> menuHtmlColor;
    public static ConfigEntry<bool> menuOpenOnMouse;
    public static ConfigEntry<bool> menuKeepSubwindowsOpen;
    public static ConfigEntry<bool> menuAllowClickThrough;
    public static ConfigEntry<bool> noTelemetry;

    public override void Load()
    {
        Plugin = this;
        Log = base.Log;

        SetupConfig();
        InitializeDefaultCheats();
        DisableTelemetry();

        Harmony.PatchAll();
        menuUI = AddComponent<MenuUI>();

        void SetupConfig()
        {
            menuKeybind = Config.Bind(
                "MalumMenu.GUI",
                "Keybind",
                "Delete",
                "The keyboard key used to toggle the GUI..."
            );
            menuHtmlColor = Config.Bind(
                "MalumMenu.GUI",
                "Color",
                "",
                "A custom color for your MalumMenu GUI..."
            );
            menuOpenOnMouse = Config.Bind(
                "MalumMenu.GUI",
                "OpenOnMouse",
                false,
                "Always open GUI at mouse position"
            );
            menuKeepSubwindowsOpen = Config.Bind(
                "MalumMenu.GUI",
                "KeepSubwindowsOpen",
                false,
                "Keep subwindows open"
            );
            menuAllowClickThrough = Config.Bind(
                "MalumMenu.GUI",
                "AllowClicksThrough",
                true,
                "Clicks pass through GUI"
            );
        }

        void InitializeDefaultCheats()
        {
            CheatState.unlockFeatures = true;
            CheatState.freeCosmetics = true;
            CheatState.avoidPenalties = true;
        }

        void DisableTelemetry()
        {
            Analytics.enabled = false;
            Analytics.deviceStatsEnabled = false;
            PerformanceReporting.enabled = false;
        }
    }
}
