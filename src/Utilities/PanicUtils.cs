using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalumMenu.Utilities;

public class PanicUtils
{
    public class PanicCleaner : MonoBehaviour
    {
        // Creates a PanicCleaner to unpatch Harmony
        public static void Create()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PanicCleaner>();
            var go = new GameObject("MalumMenu_PanicCleaner");
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<PanicCleaner>();
        }

        // Unpatching Harmony in handled in the next frame after creation
        // This allows some patches to run for a last time and finish properly
        private void LateUpdate()
        {
            try { Harmony.UnpatchID(MalumMenu.Id); } catch { }
            Destroy(gameObject);
        }

    }

    public static void Panic()
    {
        MalumMenu.isPanicked = true;

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = false;


        CheatState.DisableAllExcept(
            nameof(CheatState.stealthMode)
        );

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "MainMenu" || scene.name == "MatchMaking")
        {
            SceneManager.LoadScene(scene.name);
        }

        UnityEngine.Object.Destroy(MalumMenu.menuUI);
        PanicCleaner.Create();
    }


    public static void FakePanic()
    {
        CheatState.DisableAllExcept(
            nameof(CheatState.stealthMode)
        );
        CheatState.stealthMode = true;
    }
}
