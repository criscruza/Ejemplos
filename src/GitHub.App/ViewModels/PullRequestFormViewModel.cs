﻿using System;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using NullGuard;
using ReactiveUI;
using Octokit;

namespace GitHub.ViewModels
{
        /// <summary>
        /// Base class for the Pull Request create form. 
        /// </summary>
        public abstract class PullRequestFormViewModel : BaseViewModel
        {
            readonly ObservableAsPropertyHelper<string> safeRepositoryName;

            protected PullRequestFormViewModel()
            {
            }

            string description;
            /// <summary>
            /// Description to set on the repo (optional)
            /// </summary>
            [AllowNull]
            public string Description
            {
                [return: AllowNull]
                get { return description; }
                set { this.RaiseAndSetIfChanged(ref description, value); }
            }

            bool keepPrivate;
            /// <summary>
            /// Make the new repository private
            /// </summary>
            public bool KeepPrivate
            {
                get { return keepPrivate; }
                set { this.RaiseAndSetIfChanged(ref keepPrivate, value); }
            }

            string pullRequestTitle;
            /// <summary>
            /// Name of the repository as typed by user
            /// </summary>
            [AllowNull]
            public string PullRequestTitle
            {
                [return: AllowNull]
                get { return pullRequestTitle; }
                set { this.RaiseAndSetIfChanged(ref pullRequestTitle, value); }
            }

            public ReactivePropertyValidator<string> RepositoryNameValidator { get; protected set; }

            /// <summary>
            /// Name of the repository after fixing it to be safe (dashes instead of spaces, etc)
            /// </summary>
            public string SafeRepositoryName
            {
                [return: AllowNull]
                get { return safeRepositoryName.Value; }
            }

            public ReactivePropertyValidator<string> SafeRepositoryNameWarningValidator { get; protected set; }

            IAccount selectedAccount;
            /// <summary>
            /// Account where the repository is going to be created on
            /// </summary>
            [AllowNull]
            public IAccount SelectedAccount
            {
                [return: AllowNull]
                get { return selectedAccount; }
                set { this.RaiseAndSetIfChanged(ref selectedAccount, value); }
            }

            public bool ShowUpgradePlanWarning { get; private set; }

            public bool ShowUpgradeToMicroPlanWarning { get; private set; }

            public ICommand UpgradeAccountPlan { get; private set; }

            protected IObservable<bool> CanKeepPrivateObservable { get; private set; }

            // These are the characters which are permitted when creating a repository name on GitHub The Website
            static readonly Regex invalidRepositoryCharsRegex = new Regex(@"[^0-9A-Za-z_\.\-]", RegexOptions.ECMAScript);

            /// <summary>
            /// Given a repository name, returns a safe version with invalid characters replaced with dashes.
            /// </summary>
            protected static string GetSafeRepositoryName(string name)
            {
                return invalidRepositoryCharsRegex.Replace(name, "-");
            }

            protected virtual NewPullRequest GatherPullRequestInfo()
            {
                string justtest = PullRequestTitle;
                throw new NotImplementedException();
            }
        }
}
