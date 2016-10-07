﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.Windows.Controls;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.ViewModels;
using System.Diagnostics;
using Microsoft.VisualStudio;
using GitHub.App.Factories;
using NullGuard;

namespace GitHub.VisualStudio.UI
{

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(GitHubPaneGuid)]
    public class GitHubPane : ToolWindowPane
    {
        const string GitHubPaneGuid = "6b0fdc0a-f28e-47a0-8eed-cc296beff6d2";

        IView View
        {
            get { return Content as IView; }
            set { Content = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GitHubPane() : base(null)
        {
            Caption = "GitHub";

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            ToolBar = new CommandID(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.idGitHubToolbar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;

            var factory = this.GetExportedValue<IUIFactory>();
            var d = factory.CreateViewAndViewModel(Exports.UIViewType.GitHubPane);
            // placeholder logic to load the view until the UIController is able to do it for us
            View = d.View;
            View.DataContext = d.ViewModel;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var vm = View.ViewModel as IServiceProviderAware;
            Debug.Assert(vm != null);
            vm?.Initialize(this);
        }

        public void ShowView(ViewWithData data)
        {
            View.ViewModel?.Initialize(data);
        }

        [return: AllowNull]
        public static GitHubPane Activate()
        {
            var windowGuid = new Guid(GitHubPaneGuid);
            IVsWindowFrame frame;
            if (ErrorHandler.Failed(Services.UIShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref windowGuid, out frame)))
            {
                VsOutputLogger.WriteLine("Unable to find or create GitHubPane '" + GitHubPaneGuid + "'");
                return null;
            }
            if (ErrorHandler.Failed(frame.Show()))
            {
                VsOutputLogger.WriteLine("Unable to show GitHubPane '" + GitHubPaneGuid + "'");
                return null;
            }

            object docView = null;
            if (ErrorHandler.Failed(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView)))
            {
                VsOutputLogger.WriteLine("Unable to grab instance of GitHubPane '" + GitHubPaneGuid + "'");
                return null;
            }
            return docView as GitHubPane;
        }
    }
}
