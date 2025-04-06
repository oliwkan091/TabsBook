using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HoldMyTabs
{
    internal static class SavedTabsManagment
    {

        private const string saveFileName = "SolutionsSettings.txt";

        internal class Tab(string fullName, bool isPinned)
        {
            public string FullName { get; set; } = fullName;
            public bool IsPinned { get; set; } = isPinned;
        }

        internal class Solution(string fullName, string name)
        {
            public string FullName { get; set; } = fullName;
            public string Name { get; set; } = name;
            public List<Tab> Tabs { get; set; } = [];
        }

        internal class SavedTabsFile
        {
            public SavedTabsFile()
            {
            }

            public SavedTabsFile(List<Solution> solutions)
            {
                Solutions = solutions;
            }

            public List<Solution> Solutions { get; set; } = [];
        }


        private static string ExtensionFolder =>
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static string SolutionsSettingsFilePath =>
            Path.Combine(ExtensionFolder, saveFileName);

        private static void SaveSolutionsSettingsFile(SavedTabsFile solutionsSettings)
        {
            string serializedSaveFile = JsonConvert.SerializeObject(solutionsSettings, Formatting.Indented);
            File.WriteAllText(SolutionsSettingsFilePath, serializedSaveFile);
        }

        private static SavedTabsFile GetSavedSolutionsSettings()
        {
            if (File.Exists(SolutionsSettingsFilePath))
            {
                string loadedSolutionsSettings = File.ReadAllText(SolutionsSettingsFilePath);
                var deserializedJson  = JsonConvert.DeserializeObject<SavedTabsFile>(loadedSolutionsSettings);
                if(deserializedJson is null)
                {
                    return CreateNewSolutionSettings();
                }

                return deserializedJson;
            }
            else
            {
                SavedTabsFile newSettings = new();
                SaveSolutionsSettingsFile(newSettings);
                return newSettings;
            }
        }

        private static SavedTabsFile CreateNewSolutionSettings()
        {
            SavedTabsFile newSettings = new();
            SaveSolutionsSettingsFile(newSettings);
            return newSettings;
        }

        public static SavedTabsFile GetSavedSolution(string solutionName)
        {
            if (string.IsNullOrWhiteSpace(solutionName))
                return null;

            SavedTabsFile currentSettings = GetSavedSolutionsSettings();

            return new SavedTabsFile(
                currentSettings.Solutions.Where(s =>
                string.Equals(s.FullName, solutionName, StringComparison.OrdinalIgnoreCase)).ToList()
            );
        }

        public static SavedTabsFile GetAllSolutions()
        {
            return GetSavedSolutionsSettings();
        }

        public static void SaveSolution(SavedTabsFile solutionSettings)
        {
            if (solutionSettings is null)
                return;

            SaveSolutionsSettingsFile(solutionSettings);
        }
    }
}
