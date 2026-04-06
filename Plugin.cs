using System;
using BepInEx;
using BepInEx.Logging;
using UnbeatableTranslations.Translation;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

namespace UnbeatableTranslations;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Type[] patchClasses =
        {
            typeof(ProgramLoaderPatch)
        };
        
        var harmony = new HarmonyLib.Harmony(MyPluginInfo.PLUGIN_GUID);
        foreach (var patchClass in patchClasses)
        {
            harmony.PatchAll(patchClass);
        }
        
        ProgramLoader.LoadLocalTranslations();
    }

    public void Update()
    {
        // CTRL + Shift + 3 to dump translations
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            Logger.LogInfo("Dumping translations...");
            Dump.DumpTranslations();
        }
        
        // CTRL + Shift + 1 to enable/disable translations
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            ProgramLoader.disableCustomTranslation = !ProgramLoader.disableCustomTranslation;
            
            if (ProgramLoader.disableCustomTranslation)
            {
                Logger.LogInfo("Translations disabled!");
            }
            else
            {
                Logger.LogInfo("Translations enabled!");
            }
        }
        
        // CTRL + Shift + 2 to reload translations
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            ProgramLoader.LoadLocalTranslations();
            
            Logger.LogInfo("Translations reloaded!");
        }
        
        // CTRL + Shift + 4 reload scene
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            Logger.LogInfo("Reloading scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            ); }
        
    }
}