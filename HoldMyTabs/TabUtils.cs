using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using static HoldMyTabs.SavedTabsManagment;

namespace HoldMyTabs
{
    internal static class TabUtils
    {
        public static IEnumerable<Tab> ExtractAllOpenTabs(EnvDTE.Documents documents, bool pinnedOnly = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Document document in documents)
            {
                bool isPinned = IsDocumentPinned(document);

                //TODO 
                //if (pinnedOnly && !isPinned)
                //    continue;

                yield return new Tab(document.FullName, isPinned);
            }
        }

        public static bool IsDocumentPinned(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (document == null)
                return false;

            foreach (Window window in document.Windows)
            {
                if (window == null)
                    continue;

                PropertyInfo dockViewElementProperty = window
                    .GetType()
                    .GetProperty(
                        "DockViewElement",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

                if (dockViewElementProperty == null)
                    continue;

                object dockViewElementObject = dockViewElementProperty.GetValue(window);
                if (dockViewElementObject == null)
                    continue;

                if ((bool)dockViewElementObject.GetType().GetProperty("IsPinned").GetValue(dockViewElementObject))
                    return true;
            }

            return false;
        }

        public static List<string> RestoreTabs(SavedTabsManagment.Solution solution, DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> tabsThatWereNotRestored = new List<string>();

            var pinnedTabs = solution.Tabs.Where(x => x.IsPinned).ToList();
            var notPinnedTabs = solution.Tabs.Where(x => !x.IsPinned).ToList();
            notPinnedTabs.Reverse();

            var allTabs = new List<Tab>();
            allTabs.AddRange(pinnedTabs);
            allTabs.AddRange(notPinnedTabs);
            foreach (var tab in allTabs)
            {
                try
                {
                    ProjectItem proj = dte.Solution.FindProjectItem(tab.FullName);
                    //TODO
                    //if (proj == null)
                    //    continue;

                    Document document = dte.Documents.Open(tab.FullName);

                    if (tab.IsPinned && !TabUtils.IsDocumentPinned(document))
                        dte.ExecuteCommand("Window.PinTab");
                }
                catch (Exception)
                {
                    tabsThatWereNotRestored.Add(tab.FullName);
                }
            }

            return tabsThatWereNotRestored;
        }
    }
}
