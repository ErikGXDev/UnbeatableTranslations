using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnbeatableTranslations.Translation;

/// <summary>
/// Stores Unity localization strings in a locale → table → key → value hierarchy.
/// </summary>
public class UnityTable
{
    // locale → table name → key → value
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _locales = new();

    /// <summary>Add or overwrite a single entry.</summary>
    public void AddEntry(string locale, string tableName, string key, string value)
    {
        if (!_locales.TryGetValue(locale, out var tables))
        {
            tables = new Dictionary<string, Dictionary<string, string>>();
            _locales[locale] = tables;
        }

        if (!tables.TryGetValue(tableName, out var entries))
        {
            entries = new Dictionary<string, string>();
            tables[tableName] = entries;
        }

        entries[key] = value;
    }

    /// <summary>Try to look up a translated string.</summary>
    public bool TryGetEntry(string locale, string tableName, string key, out string value)
    {
        value = null;
        return _locales.TryGetValue(locale, out var tables)
               && tables.TryGetValue(tableName, out var entries)
               && entries.TryGetValue(key, out value);
    }

    /// <summary>Serialize the whole table to indented JSON.</summary>
    public string ToJson() => JsonConvert.SerializeObject(_locales, Formatting.Indented);

    /// <summary>Deserialize a UnityTable from JSON text.</summary>
    public static UnityTable FromJson(string json)
    {
        var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
        var table = new UnityTable();
        if (raw == null) return table;

        foreach (var (locale, tables) in raw)
            foreach (var (tableName, entries) in tables)
                foreach (var (key, value) in entries)
                    table.AddEntry(locale, tableName, key, value);

        return table;
    }
}
