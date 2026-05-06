using System;
using BepInEx;
using BepInEx.Logging;
using UnbeatableTranslations.Translation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;

namespace UnbeatableTranslations;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"");
        Logger.LogInfo("--- Translation Mod Loaded ---");
        Logger.LogInfo($"");
        Logger.LogInfo($"Find Source Code + Documentation here:");
        Logger.LogInfo($"https://github.com/ErikGXDev/UnbeatableTranslations");
        Logger.LogInfo($"");
        Logger.LogInfo("--- Shortcuts ---");
        Logger.LogInfo("Ctrl + Shift + 1: Toggle translations on/off");
        Logger.LogInfo("Ctrl + Shift + 2: Reload translations");
        Logger.LogInfo("Ctrl + Shift + 3: Dump translations from the game to the disk");
        Logger.LogInfo("Ctrl + Shift + 4: Reload the current scene");
        Logger.LogInfo("");
        Logger.LogInfo("Check link above for more instructions.");
        Logger.LogInfo("--- --- ---");
        Logger.LogInfo($"");


        var harmony = new HarmonyLib.Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll(typeof(ProgramLoaderPatch));

        ProgramLoader.LoadLocalTranslations();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneTranslationApplier.RequestApply(this);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneTranslationApplier.RequestApply(this);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            Logger.LogInfo("Dumping translations...");
            Dump.DumpTranslations();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            ProgramLoader.disableCustomTranslation = !ProgramLoader.disableCustomTranslation;
            Logger.LogInfo(ProgramLoader.disableCustomTranslation ? "Translations disabled!" : "Translations enabled!");
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            ProgramLoader.LoadLocalTranslations();
            SceneTranslationApplier.RequestApply(this);
            Logger.LogInfo("Translations reloaded!");
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            Logger.LogInfo("Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

    }
}