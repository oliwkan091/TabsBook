using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

            this.Loaded += InitializeOnWindowOpenDelegate;
        }

        private void InitializeOnWindowOpenDelegate(object sender, RoutedEventArgs e)
        {
            InitializeOnWindowOpen();
        }

        public void InitializeOnWindowOpen()
        {
            LoadSavedTabs();
            InitializeFields();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string newEntry = FileNameTextBox.Text;
            names.Add(newEntry);
            comboSavedInfo.SelectedItem = newEntry;
            SaveNewTab();
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
                DeleteTab(comboSavedInfo.SelectedItem.ToString());
                names.Remove(comboSavedInfo.SelectedItem.ToString());
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

        private SavedTabsFile LoadSolutionSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            return SavedTabsManagment.GetSavedSolution(dte.Solution.FullName);
        }

        private void LoadSavedTabs()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.solutionSettings = LoadSolutionSettings();

            if (this.solutionSettings is null)
            {
                //TODO zamykanie okna
                return;
            }      

            //TODO dane SolutionSettings są wczytywane przed każdą operacją ale dane names nie są aktualiowane 
            names = new ObservableCollection<string>();
            foreach (var solution in this.solutionSettings.Solutions)
            {
                names.Add(solution.Name);
            }

            comboSavedInfo.ItemsSource = names;
        }

        private void InitializeFields()
        {
            FileNameTextBox.Text = $"Entry {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        private void DeleteTab(string fileToDeleteName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = this.solutionSettings.Solutions.FirstOrDefault(s =>
                string.Equals(s.Name, fileToDeleteName, StringComparison.OrdinalIgnoreCase));
            this.solutionSettings.Solutions.Remove(solution);
            SavedTabsManagment.SaveSolution(this.solutionSettings);
        }

        private void SaveNewTab()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            SavedTabsManagment.Solution solution = new(dte.Solution.FullName, FileNameTextBox.Text);
            solution.Tabs.AddRange(TabUtils.ExtractAllOpenTabs(dte.Documents));
            this.solutionSettings.Solutions.Add(solution);
            SavedTabsManagment.SaveSolution(this.solutionSettings);
        }
    }
}
