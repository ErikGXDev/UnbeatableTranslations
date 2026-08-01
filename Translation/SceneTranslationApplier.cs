using System;
using System.Collections;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.UI;

namespace UnbeatableTranslations.Translation;

public static class SceneTranslationApplier
{
    private const int MaxAttempts = 5;

    public static void RequestApply(MonoBehaviour host)
    {
        if (host != null)
        {
            host.StartCoroutine(ApplyWhenReady());
        }
    }

    private static IEnumerator ApplyWhenReady()
    {
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            yield return null;

            if (ApplyToScene() > 0)
            {
                yield break;
            }
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

                    if (TryApplyTranslation(localizer.gameObject, translatedValue))
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

    private static bool TryApplyTranslation(GameObject gameObject, string translatedText)
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
                return true; // No need to set the same text again
            }
            
            textMeshProUGUI.SetText(translatedText);
            return true;
        }

        var textMeshPro = gameObject.GetComponent<TextMeshPro>();
        if (textMeshPro != null)
        {
            if (textMeshPro.text == translatedText)
            {
                return true; // No need to set the same text again
            }
            textMeshPro.SetText(translatedText);
            return true;
        }

        var unityText = gameObject.GetComponent<Text>();
        if (unityText != null)
        {
            if (unityText.text == translatedText)
            {
                return true; // No need to set the same text again
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
                return true; // No need to set the same text again
            }
            
            textMeshProUGUI.SetText(translatedText);
            return true;
        }

        var textMeshPro = gameObject.GetComponentInChildren<TextMeshPro>();
        if (textMeshPro != null)
        {
            if (textMeshPro.text == translatedText)
            {
                return true; // No need to set the same text again
            }
            
            textMeshPro.SetText(translatedText);
            return true;
        }

        var unityText = gameObject.GetComponentInChildren<Text>();
        if (unityText != null)
        {
            if (unityText.text == translatedText)
            {
                return true; // No need to set the same text again
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

