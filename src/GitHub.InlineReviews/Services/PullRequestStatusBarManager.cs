﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel.Composition;
using GitHub.Commands;
using GitHub.InlineReviews.Views;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using GitHub.Models;
using GitHub.Logging;
using GitHub.Extensions;
using Serilog;
using ReactiveUI;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Manage the UI that shows the PR for the current branch.
    /// </summary>
    [Export(typeof(PullRequestStatusBarManager))]
    public class PullRequestStatusBarManager
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestStatusBarManager>();
        const string StatusBarPartName = "PART_SccStatusBarHost";

        readonly IUsageTracker usageTracker;
        readonly IShowCurrentPullRequestCommand showCurrentPullRequestCommand;

        // More the moment this must be constructed on the main thread.
        // TeamExplorerContext needs to retrieve DTE using GetService.
        readonly Lazy<IPullRequestSessionManager> pullRequestSessionManager;

        [ImportingConstructor]
        public PullRequestStatusBarManager(
            IUsageTracker usageTracker,
            IShowCurrentPullRequestCommand showCurrentPullRequestCommand,
            Lazy<IPullRequestSessionManager> pullRequestSessionManager)
        {
            this.usageTracker = usageTracker;
            this.showCurrentPullRequestCommand = showCurrentPullRequestCommand;
            this.pullRequestSessionManager = pullRequestSessionManager;
        }

        /// <summary>
        /// Start showing the PR for the active branch on the status bar.
        /// </summary>
        /// <remarks>
        /// This must be called from the Main thread.
        /// </remarks>
        public void StartShowingStatus()
        {
            try
            {
                pullRequestSessionManager.Value.WhenAnyValue(x => x.CurrentSession)
                    .Subscribe(x => RefreshCurrentSession());
            }
            catch (Exception e)
            {
                log.Error(e, "Error initializing");
            }
        }

        void RefreshCurrentSession()
        {
            var pullRequest = pullRequestSessionManager.Value.CurrentSession?.PullRequest;
            var viewModel = pullRequest != null ? CreatePullRequestStatusViewModel(pullRequest) : null;
            ShowStatus(viewModel);
        }

        PullRequestStatusViewModel CreatePullRequestStatusViewModel(IPullRequestModel pullRequest)
        {
            var trackingCommand = new UsageTrackingCommand(showCurrentPullRequestCommand, usageTracker);
            var pullRequestStatusViewModel = new PullRequestStatusViewModel(trackingCommand);
            pullRequestStatusViewModel.Number = pullRequest.Number;
            pullRequestStatusViewModel.Title = pullRequest.Title;
            return pullRequestStatusViewModel;
        }

        void IncrementNumberOfShowCurrentPullRequest()
        {
        }

        void ShowStatus(PullRequestStatusViewModel pullRequestStatusViewModel = null)
        {
            var statusBar = FindSccStatusBar(Application.Current.MainWindow);
            if (statusBar != null)
            {
                var githubStatusBar = Find<PullRequestStatusView>(statusBar);
                if (githubStatusBar != null)
                {
                    // Replace to ensure status shows up.
                    statusBar.Items.Remove(githubStatusBar);
                }

                if (pullRequestStatusViewModel != null)
                {
                    githubStatusBar = new PullRequestStatusView { DataContext = pullRequestStatusViewModel };
                    statusBar.Items.Insert(0, githubStatusBar);
                }
            }
        }

        static T Find<T>(StatusBar statusBar)
        {
            foreach (var item in statusBar.Items)
            {
                if (item is T)
                {
                    return (T)item;
                }
            }

            return default(T);
        }

        StatusBar FindSccStatusBar(Window mainWindow)
        {
            var contentControl = mainWindow?.Template?.FindName(StatusBarPartName, mainWindow) as ContentControl;
            return contentControl?.Content as StatusBar;
        }

        class UsageTrackingCommand : ICommand
        {
            readonly ICommand command;
            readonly IUsageTracker usageTracker;

            internal UsageTrackingCommand(ICommand command, IUsageTracker usageTracker)
            {
                this.command = command;
                this.usageTracker = usageTracker;
            }

            public event EventHandler CanExecuteChanged
            {
                add
                {
                    command.CanExecuteChanged += value;
                }

                remove
                {
                    command.CanExecuteChanged -= value;
                }
            }

            public bool CanExecute(object parameter)
            {
                return command.CanExecute(parameter);
            }

            public void Execute(object parameter)
            {
                command.Execute(parameter);
                usageTracker.IncrementCounter(x => x.NumberOfShowCurrentPullRequest).Forget();
            }
        }
    }
}
