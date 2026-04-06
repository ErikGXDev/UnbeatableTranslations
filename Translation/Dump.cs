using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Localization.Tables;
using UnityEngine;
using Yarn.Unity;

namespace UnbeatableTranslations.Translation
{
    public class Dump
    {

        public static string GetOutPath()
        {
            return Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/translations_dumped/";
        }
        
        public static void DumpTranslations()
        {
            DumpYarnTranslations();
            DumpUnityTranslations();
        }

        public static void DumpYarnTranslations()
        {

            Plugin.Logger.LogInfo("Dumping translations...");

            string outPath = GetOutPath();
            

            // Check if the directory exists
            if (!Directory.Exists(outPath))
            {
                // Create the directory
                Directory.CreateDirectory(outPath);
            }


            YarnProject[] projects = (YarnProject[])Resources.FindObjectsOfTypeAll(typeof(YarnProject));


            Plugin.Logger.LogInfo(projects.Length + " projects found.");

            var lineRec = new Dictionary<string, string>();

            bool isCustomDisabled = ProgramLoader.disableCustomTranslation;
            if (!isCustomDisabled)
            {
                ProgramLoader.disableCustomTranslation = true;

            }

            foreach (YarnProject project in projects)
            {

                string program = project.GetProgram().ToString();


                string fileName = project.name + ".yarnproject.json";


                string filePath = outPath + fileName;



                File.WriteAllText(filePath, program);

                Yarn.Unity.Localization baseLoc = project.baseLocalization;

                

                foreach (string id in baseLoc.GetLineIDs())
                {
                    if (!lineRec.TryAdd(id, baseLoc.GetLocalizedString(id)))
                    {
                        Plugin.Logger.LogInfo("Duplicate line found: " + id + " in " + fileName);
                    }
                    
                }

               

            }

            if (!isCustomDisabled)
            {
                ProgramLoader.disableCustomTranslation = false;
            }

            var outLines = JsonConvert.SerializeObject(lineRec, Formatting.Indented);

            var outPathLines = outPath + "lines.json";


            File.WriteAllText(outPathLines, outLines);


            Plugin.Logger.LogInfo("Dumped translations to: " + outPath);
        }

        public static void DumpUnityTranslations()
        {
            Plugin.Logger.LogInfo("Dumping Unity localization strings...");
            
            string outPath = GetOutPath();
            
            // Ensure directory exists
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            
            // Find all StringTable objects in the scene/resources
            StringTable[] stringTables = (StringTable[])Resources.FindObjectsOfTypeAll(typeof(StringTable));
            
            
            // TODO: Make better
            // Filter for only the english tables
            stringTables = stringTables.Where(t => t.LocaleIdentifier.Code == "en").ToArray();
            
            
            Plugin.Logger.LogInfo($"Found {stringTables.Length} string table(s)");
            
            var allTranslations = new UnityTable();
            
            foreach (var table in stringTables)
            {
                if (table == null || string.IsNullOrEmpty(table.LocaleIdentifier.Code))
                    continue;
                    
                string localeCode = table.LocaleIdentifier.Code;
                string tableName = table.TableCollectionName;
                
                Plugin.Logger.LogInfo($"Processing table: {tableName} for locale: {localeCode} ({table.Count} entries)");
                
                // Iterate through all entries in the table
                foreach (var entry in table)
                {
                    if (entry.Value == null || string.IsNullOrEmpty(entry.Value.LocalizedValue))
                        continue;

                    string keyName = table.SharedData.GetEntry(entry.Key)?.Key;
                    if (!string.IsNullOrEmpty(keyName))
                        allTranslations.AddEntry(localeCode, tableName, keyName, entry.Value.LocalizedValue);
                }
            }
            
            // Save to JSON
            string jsonOutput = allTranslations.ToJson();
            string outputPath = outPath + "unity_translations.json";
            File.WriteAllText(outputPath, jsonOutput);
            
            Plugin.Logger.LogInfo($"Dumped Unity translations to: {outputPath}");
        }

    }
}
