using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace HoldMyTabs
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(HoldMyTabsPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SaveTabsWindow))]
    public sealed class HoldMyTabsPackage : AsyncPackage
    {
        public const string PackageGuidString = "392072a6-8ff8-4dfb-acf8-080c960082db";

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await SaveTabsWindowCommand.InitializeAsync(this);
        }

        #endregion
    }
}
