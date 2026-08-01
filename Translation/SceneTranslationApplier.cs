using System;
using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.UI;

namespace UnbeatableTranslations.Translation;

public static class SceneTranslationApplier
{
    public static void RequestApplyScene(MonoBehaviour host, int tries = 5)
    {
        if (host)
        {
            host.StartCoroutine(ApplySceneWhenReady(tries));
        }
    }
    
    public static void RequestApplyLocalizer(GameObjectLocalizer localizer, int tries = 5)
    {
        if (localizer)
        {
            localizer.StartCoroutine(ApplyLocalizerWhenReady(localizer, tries));
        }
    }

    private static IEnumerator ApplySceneWhenReady(int tries)
    {
        for (int attempt = 0; attempt < tries; attempt++)
        {
            yield return null;

            ApplyToScene();
            
            yield return new WaitForSeconds(0.2f);

        }

    }
    
    private static IEnumerator ApplyLocalizerWhenReady(GameObjectLocalizer localizer, int tries)
    {
        for (int attempt = 0; attempt < tries; attempt++)
        {
            yield return null;

            if (!localizer || !localizer.gameObject)
            {
                yield break;
            }

            ApplyToLocalizer(localizer);
            
            yield return new WaitForSeconds(0.2f);

        }
    }

    private static int ApplyToScene()
    {
        if (ProgramLoader.disableCustomTranslation || ProgramIndex.unityTable == null)
        {
            return 0;
        }

        var appliedCount = 0;

        try
        {
            var localizers = Resources.FindObjectsOfTypeAll<GameObjectLocalizer>();
            foreach (var localizer in localizers)
            {
                if (localizer == null || localizer.gameObject == null)
                {
                    continue;
                }

                appliedCount += ApplyToLocalizer(localizer);
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"Failed to apply translations to scene: {ex.Message}");
        }

        return appliedCount;
    }

    private static int ApplyToLocalizer(GameObjectLocalizer localizer)
    {
        var appliedCount = 0;

        try
        {
            var trackedObjects = localizer.TrackedObjects;
            if (trackedObjects == null)
            {
                return 0;
            }

            foreach (var trackedObject in trackedObjects)
            {
                if (trackedObject == null)
                {
                    continue;
                }

                var trackedProperties = trackedObject.TrackedProperties;
                if (trackedProperties == null)
                {
                    continue;
                }

                foreach (var trackedProperty in trackedProperties)
                {
                    if (trackedProperty is not LocalizedStringProperty localizedStringProperty)
                    {
                        continue;
                    }

                    if (!TryExtractLocalizationEntry(localizedStringProperty.LocalizedString, localizedStringProperty, out var tableName, out var entryKey))
                    {
                        continue;
                    }

                    if (!ProgramIndex.unityTable.TryGetEntry("en", tableName, entryKey, out var translatedValue))
                    {
                        continue;
                    }

                    if (TryApplyTranslation(trackedObject, translatedValue))
                    {
                        appliedCount++;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"Failed to apply translations for {localizer.name}: {ex.Message}");
        }

        return appliedCount;
    }

    private static bool TryApplyTranslation(TrackedObject trackedObject, string translatedText)
    {
        if (trackedObject == null || trackedObject.Target == null)
        {
            return false;
        }

        if (trackedObject.Target is TMP_Text tmpText)
        {
            if (tmpText.text == translatedText)
            {
                return true;
            }

            tmpText.SetText(translatedText);
            return true;
        }
        
        if (trackedObject.Target is Text unityText)
        {
            if (unityText.text == translatedText)
            {
                return true;
            }

            unityText.text = translatedText;
            return true;
        }
        
        if (trackedObject.Target is GameObject gameObject)
        {
            return TryApplyTranslationO(gameObject, translatedText);
        }
        
        return false;
    }

    private static bool TryApplyTranslationO(GameObject gameObject, string translatedText)
    {
        if (gameObject == null)
        {
            return false;
        }

        if (TrySetText(gameObject, translatedText))
        {
            return true;
        }

        return TrySetTextInChildren(gameObject, translatedText);
    }

    private static bool TrySetText(GameObject gameObject, string translatedText)
    {
        var textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        if (textMeshProUGUI != null)
        {
            if (textMeshProUGUI.text == translatedText)
            {
                return true;
            }
            
            textMeshProUGUI.SetText(translatedText);
            return true;
        }

        var textMeshPro = gameObject.GetComponent<TextMeshPro>();
        if (textMeshPro != null)
        {
            if (textMeshPro.text == translatedText)
            {
                return true;
            }
            textMeshPro.SetText(translatedText);
            return true;
        }

        var unityText = gameObject.GetComponent<Text>();
        if (unityText != null)
        {
            if (unityText.text == translatedText)
            {
                return true;
            }
            
            unityText.text = translatedText;
            return true;
        }

        return false;
    }

    private static bool TrySetTextInChildren(GameObject gameObject, string translatedText)
    {
        var textMeshProUGUI = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshProUGUI != null)
        {
            if (textMeshProUGUI.text == translatedText)
            {
                return true;
            }
            
            textMeshProUGUI.SetText(translatedText);
            return true;
        }

        var textMeshPro = gameObject.GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null)
        {
            if (textMeshPro.text == translatedText)
            {
                return true;
            }
            
            textMeshPro.SetText(translatedText);
            return true;
        }

        var unityText = gameObject.GetComponentInChildren<Text>();
        if (unityText != null)
        {
            if (unityText.text == translatedText)
            {
                return true;
            }
            unityText.text = translatedText;
            return true;
        }

        return false;
    }

    private static bool TryExtractLocalizationEntry(LocalizedString localizedString, LocalizedStringProperty localizedStringProperty,
        out string tableName, out string entryKey)
    {
        tableName = null;
        entryKey = null;

        if (localizedString == null)
        {
            return false;
        }

        tableName = localizedString.TableReference.TableCollectionName;
        entryKey = localizedString.TableEntryReference.Key;

        if (string.IsNullOrEmpty(entryKey) && TryParseEntryKeyFromLocalizedString(localizedString.TableEntryReference.ToString(), out var parsedEntryKey))
        {
            entryKey = parsedEntryKey;
        }

        if (string.IsNullOrEmpty(entryKey) && localizedStringProperty != null)
        {
            try
            {
                var field = localizedStringProperty.GetType().GetField("m_Localized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var value = field?.GetValue(localizedStringProperty);
                if (value != null && TryParseEntryKeyFromLocalizedString(value.ToString(), out var parsedLocalizedEntryKey))
                {
                    entryKey = parsedLocalizedEntryKey;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"Failed to extract key from LocalizedStringProperty.m_Localized: {ex.Message}");
            }
        }

        return !string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(entryKey);
    }

    private static bool TryParseEntryKeyFromLocalizedString(string text, out string entryKey)
    {
        entryKey = null;

        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        const string separator = " - ";
        var separatorIndex = text.LastIndexOf(separator, StringComparison.Ordinal);
        var closeIndex = text.LastIndexOf(')');

        if (separatorIndex < 0 || closeIndex < 0 || closeIndex <= separatorIndex)
        {
            return false;
        }

        entryKey = text.Substring(separatorIndex + separator.Length, closeIndex - separatorIndex - separator.Length).Trim();
        return !string.IsNullOrEmpty(entryKey);
    }
}

