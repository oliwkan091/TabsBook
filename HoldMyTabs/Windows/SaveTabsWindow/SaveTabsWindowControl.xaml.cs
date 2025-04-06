using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static HoldMyTabs.SavedTabsManagment;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HoldMyTabs
{
    public partial class SaveTabsWindowControl : UserControl
    {
        private ObservableCollection<string> names;
        private SavedTabsFile solutionSettings;
        private readonly ToolWindowPane _toolWindowPane;

        public SaveTabsWindowControl(ToolWindowPane toolWindowPane)
        {
            _toolWindowPane = toolWindowPane ?? throw new NullReferenceException("ToolWindowPane was not passed");
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
            if(comboSavedInfo.SelectedItem is null)
            {
                return;
            }
            var solution = this.solutionSettings.Solutions.FirstOrDefault(s =>
                string.Equals(s.Name, comboSavedInfo.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

            DTE2 dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

            var tabsThatWereNotRestored = TabUtils.RestoreTabs(solution, dte);

            if (tabsThatWereNotRestored.Any())
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Some bookmarks were not opened: \n");
                foreach (var element in tabsThatWereNotRestored)
                {
                    sb.Append(tabsThatWereNotRestored[0] + '\n');
                }

                VsShellUtilities.ShowMessageBox(
                    serviceProvider: _toolWindowPane,
                    message: sb.ToString(),
                    title: "Information",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST
                );
            }
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
                IVsWindowFrame windowFrame = (IVsWindowFrame)_toolWindowPane.Frame;
                windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                return;
            }      

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
