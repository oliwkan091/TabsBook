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

        internal class Solution(string fullName, string name, DateTime? creatDate)
        {
            public string FullName { get; set; } = fullName;
            public string Name { get; set; } = name;
            public DateTime? CreatDate { get; set; } = creatDate ?? DateTime.UtcNow;
            public List<Tab> Tabs { get; set; } = [];
        }

        internal class SavedSollutions
        {
            public SavedSollutions()
            {
            }

            public SavedSollutions(List<Solution> solutions)
            {
                Solutions = solutions;
            }

            public List<Solution> Solutions { get; set; } = [];
        }


        private static string ExtensionFolder =>
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static string SolutionsSettingsFilePath =>
            Path.Combine(ExtensionFolder, saveFileName);

        private static void SaveSolutionsSettingsFile(SavedSollutions solutionsSettings)
        {
            string serializedSaveFile = JsonConvert.SerializeObject(solutionsSettings, Formatting.Indented);
            File.WriteAllText(SolutionsSettingsFilePath, serializedSaveFile);
        }

        private static SavedSollutions GetSavedSolutionsSettings()
        {
            if (File.Exists(SolutionsSettingsFilePath))
            {
                string loadedSolutionsSettings = File.ReadAllText(SolutionsSettingsFilePath);
                var deserializedJson  = JsonConvert.DeserializeObject<SavedSollutions>(loadedSolutionsSettings);
                if(deserializedJson is null)
                {
                    return CreateNewSolutionSettings();
                }

                return deserializedJson;
            }
            else
            {
                SavedSollutions newSettings = new();
                SaveSolutionsSettingsFile(newSettings);
                return newSettings;
            }
        }

        private static SavedSollutions CreateNewSolutionSettings()
        {
            SavedSollutions newSettings = new();
            SaveSolutionsSettingsFile(newSettings);
            return newSettings;
        }

        public static SavedSollutions GetSavedSolution(string solutionName)
        {
            if (string.IsNullOrWhiteSpace(solutionName))
                return null;

            SavedSollutions currentSettings = GetSavedSolutionsSettings();

            var savedSollutions = new SavedSollutions(
                currentSettings.Solutions.OrderByDescending(x => x.CreatDate).Where(s =>
                    string.Equals(s.FullName, solutionName, StringComparison.OrdinalIgnoreCase)
                ).ToList()
            );

            return savedSollutions;
        }

        public static SavedSollutions GetAllSolutions()
        {
            return GetSavedSolutionsSettings();
        }

        public static void SaveSolution(SavedSollutions solutionSettings)
        {
            if (solutionSettings is null)
                return;

            SaveSolutionsSettingsFile(solutionSettings);
        }
    }
}
