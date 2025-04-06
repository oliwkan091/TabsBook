using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HoldMyTabs
{
    [Guid("251a4762-0953-4fa2-928c-9b21b7aed94a")]
    public class SaveTabsWindow : ToolWindowPane
    {
        public SaveTabsWindow() : base(null)
        {
            this.Caption = "Save Tabs";
            this.Content = new SaveTabsWindowControl(this);
        }

    }
}
