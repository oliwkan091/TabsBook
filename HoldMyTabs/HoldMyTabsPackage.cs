using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace HoldMyTabs
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(HoldMyTabsPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SaveTabsWindow), Transient = true)]
    public sealed class HoldMyTabsPackage : AsyncPackage
    {
        public const string PackageGuidString = "392072a6-8ff8-4dfb-acf8-080c960082db";

        #region Package Members

        private DTE2 _dte;
        private DTEEvents _dteEvents;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await SaveTabsWindowCommand.InitializeAsync(this);

             _dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            _dteEvents = _dte.Events.DTEEvents;
            _dteEvents.OnBeginShutdown += OnVisualStudioClose;
        }

        private void OnVisualStudioClose()
        {
            CloseTabManagementWindow();
        }

        private void CloseTabManagementWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ToolWindowPane window = this.FindToolWindow(typeof(SaveTabsWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                //TODO
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
        }

        #endregion
    }
}
