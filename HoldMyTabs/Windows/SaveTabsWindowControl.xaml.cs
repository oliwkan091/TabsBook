using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static HoldMyTabs.SavedTabsManagment;

namespace HoldMyTabs
{
    public partial class SaveTabsWindowControl : UserControl
    {
        private ObservableCollection<string> names;
        private SavedTabsFile solutionSettings;

        public SaveTabsWindowControl()
        {
            InitializeComponent();
            LoadData();
            InitializeFields();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string newEntry = FileNameTextBox.Text;
            names.Add(newEntry);
            comboSavedInfo.SelectedItem = newEntry;
            SaveData();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var solution = this.solutionSettings.Solutions.FirstOrDefault(s =>
                string.Equals(s.Name, comboSavedInfo.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

            var tabsThatWereNotRestored = TabUtils.RestoreTabs(solution, dte);

            //TODO dodać informacje tabsThatWereNotRestored
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (comboSavedInfo.SelectedItem != null)
            {
                names.Remove(comboSavedInfo.SelectedItem.ToString());
                SaveData();
                if (names.Any())
                {
                    comboSavedInfo.SelectedItem = names[0];
                }
            }
            else
            {
                MessageBox.Show("Select an item to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void LoadData()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

            this.solutionSettings = SavedTabsManagment.GetSavedSolution(dte.Solution.FullName);

            names = new ObservableCollection<string>();
            if (this.solutionSettings is null)
                return;

            foreach (var solution in this.solutionSettings.Solutions)
            {
                names.Add(solution.Name);
            }

            comboSavedInfo.ItemsSource = names;
            if (names.Any())
            { 
                comboSavedInfo.SelectedItem = names[0];
            }
        }

        private void InitializeFields()
        {
            FileNameTextBox.Text = $"Entry {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        private void SaveData()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            SavedTabsManagment.Solution solution = new(dte.Solution.FullName, FileNameTextBox.Text);
            solution.Tabs.AddRange(TabUtils.ExtractAllOpenTabs(dte.Documents));
            this.solutionSettings.Solutions.Add(solution);
            SavedTabsManagment.SaveNewSolution(solution);
        }
    }
}
