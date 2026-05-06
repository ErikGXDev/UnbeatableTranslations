using System;
using HarmonyLib;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using static UnbeatableTranslations.Translation.ProgramLoader;

namespace UnbeatableTranslations.Translation
{
    public static class ProgramLoaderPatch
    {
        [HarmonyPatch(typeof(Yarn.Unity.YarnProject), "GetProgram")]
        [HarmonyPrefix]
        public static bool GetCustomProgram(Yarn.Unity.YarnProject __instance, ref Yarn.Program __result)
        {
            if (disableCustomTranslation)
            {
                return true;
            }

            if (ProgramIndex.programs.ContainsKey(__instance.name))
            {
                __result = ProgramIndex.programs[__instance.name];
                return false;
            }

            return true;
        }


        [HarmonyPatch(typeof(Yarn.Unity.Localization), "GetLocalizedString")]
        [HarmonyPrefix]
        public static bool GetCustomLocalizedString(Yarn.Unity.Localization __instance, string key, ref string __result)
        {

            if (disableCustomTranslation)
            {
                return true;
            }

            //Plugin.Logger.LogInfo("Getting localized string for key: " + key);

            if (ProgramIndex.lines.ContainsKey(key))
            {
                __result = ProgramIndex.lines[key];
                return false;
            }


            return true;

        }
        
        // TODO: Somehow make this work, maybe await translations update for the game?

        /*// Patch 1: StringTableEntry.GetLocalizedString()
        [HarmonyPatch(typeof(StringTableEntry), "GetLocalizedString", new Type[] { })]
        [HarmonyPrefix]
        public static bool StringTableEntryPatch(StringTableEntry __instance, ref string __result)
        {
            //Plugin.Logger.LogInfo("[Patch] StringTableEntry.GetLocalizedString called");
            //Plugin.Logger.LogInfo(__instance.Key + " - " + __instance.Value);
            //Plugin.Logger.LogInfo(__instance.Table.TableCollectionName);
            if (__instance.Table != null)
            {
                string tableName = __instance.Table.TableCollectionName;
                string key = __instance.Key;
                string localeCode = "en";
                
                //Plugin.Logger.LogInfo($"[StringTableEntry] Table: {tableName}, Key: {key}");
                
                if (ProgramIndex.unityTable.TryGetEntry(localeCode, tableName, key, out string value))
                {
                    __result = value;
                    return false;
                }
            }

            return true;
        }

        // Patch 2: LocalizedString.GetLocalizedString()
        [HarmonyPatch(typeof(LocalizedString), "GetLocalizedString", new Type[] { })]
        [HarmonyPrefix]
        public static bool LocalizedStringPatch(LocalizedString __instance, ref string __result)
        {

            if (disableCustomTranslation)
            {
                return true;
            }
            
            try
            {
                var tableRef = __instance.TableReference;
                var entryRef = __instance.TableEntryReference;

                if (!string.IsNullOrEmpty(tableRef.TableCollectionName) && !string.IsNullOrEmpty(entryRef.Key))
                {
                    string tableName = tableRef.TableCollectionName;
                    string key = entryRef.Key;
                    //Plugin.Logger.LogInfo($"[LocalizedString] Table: {tableName}, Key: {key}");
                    
                    string localeCode = "en";
            
                    if (ProgramIndex.unityTable.TryGetEntry(localeCode, tableName, key, out string value))
                    {
                        __result = value;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in LocalizedString patch: {ex.Message}");
            }

            return true;
        }

        // Patch 3: LocalizedDatabase.GetLocalizedString() - with nullable Locale parameter
        [HarmonyPatch(typeof(LocalizedStringDatabase), "GetLocalizedString", new[] { typeof(TableReference), typeof(TableEntryReference), typeof(Locale), typeof(FallbackBehavior), typeof(object[]) })]
        [HarmonyPrefix]
        public static bool LocalizedDatabasePatch(
            LocalizedStringDatabase __instance,
            TableReference tableReference,
            TableEntryReference tableEntryReference,
            Locale locale,
            FallbackBehavior fallbackBehavior,
            object[] arguments,
            ref string __result)
        {
            
            if (disableCustomTranslation)
            {
                return true;
            }

            var localeCode = "en";
            var tableName = tableReference.TableCollectionName;
            var key = tableEntryReference.Key;
            
            try
            {
                string dLocaleCode = "en";
                string dTableName = tableReference.TableCollectionName;
                string dEntryKey = tableEntryReference.Key;
                //Plugin.Logger.LogInfo($"[LocalizedDatabase] Table: {dTableName}, Entry: {dEntryKey}, Locale: {dLocaleCode}, Fallback: {fallbackBehavior}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Error in LocalizedDatabase patch: {ex.Message}");
            }
            
            if (ProgramIndex.unityTable.TryGetEntry(localeCode, tableName, key, out string value))
            {
                __result = value;
                return false;
            }

            
            

            return true;
        }
        */


    }
}
