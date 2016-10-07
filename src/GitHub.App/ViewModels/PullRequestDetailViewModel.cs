﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Caches;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// A view model which displays the details of a pull request.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel repository;
        readonly IAvatarProvider avatarProvider;
        PullRequestState state;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        int commitCount;
        IAccount author;
        DateTimeOffset createdAt;
        string body;
        int number;
        int changeCount;
        ChangedFilesView changedFilesView;
        OpenChangedFileAction openChangedFileAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="connectionRepositoryHostMap">The connection repository host map.</param>
        /// <param name="teservice">The team explorer service.</param>
        /// <param name="avatarProvider">The avatar provider.</param>
        [ImportingConstructor]
        PullRequestDetailViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice,
            IAvatarProvider avatarProvider)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo, avatarProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestDetailViewModel"/> class.
        /// </summary>
        /// <param name="repositoryHost">The repository host.</param>
        /// <param name="teservice">The team explorer service.</param>
        /// <param name="avatarProvider">The avatar provider.</param>
        public PullRequestDetailViewModel(
            IRepositoryHost repositoryHost,
            ILocalRepositoryModel repository,
            IAvatarProvider avatarProvider)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;
            this.avatarProvider = avatarProvider;

            OpenOnGitHub = ReactiveCommand.Create();

            ToggleChangedFilesView = ReactiveCommand.Create();
            ToggleChangedFilesView.Subscribe(_ =>
            {
                ChangedFilesView = ChangedFilesView == ChangedFilesView.TreeView ?
                    ChangedFilesView.ListView : ChangedFilesView.TreeView;
            });

            ToggleOpenChangedFileAction = ReactiveCommand.Create();
            ToggleOpenChangedFileAction.Subscribe(_ =>
            {
                OpenChangedFileAction = OpenChangedFileAction == OpenChangedFileAction.Diff ?
                    OpenChangedFileAction.Open : OpenChangedFileAction.Diff;
            });
        }

        /// <summary>
        /// Gets the state of the pull request, e.g. Open, Closed, Merged.
        /// </summary>
        public PullRequestState State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        /// <summary>
        /// Gets a string describing how to display the pull request's source branch.
        /// </summary>
        public string SourceBranchDisplayName
        {
            get { return sourceBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value); }
        }

        /// <summary>
        /// Gets a string describing how to display the pull request's target branch.
        /// </summary>
        public string TargetBranchDisplayName
        {
            get { return targetBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value); }
        }

        /// <summary>
        /// Gets the number of commits in the pull request.
        /// </summary>
        public int CommitCount
        {
            get { return commitCount; }
            private set { this.RaiseAndSetIfChanged(ref commitCount, value); }
        }

        /// <summary>
        /// Gets the pull request number.
        /// </summary>
        public int Number
        {
            get { return number; }
            private set { this.RaiseAndSetIfChanged(ref number, value); }
        }

        /// <summary>
        /// Gets the account that submitted the pull request.
        /// </summary>
        public IAccount Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

        /// <summary>
        /// Gets the date and time at which the pull request was created.
        /// </summary>
        public DateTimeOffset CreatedAt
        {
            get { return createdAt; }
            private set { this.RaiseAndSetIfChanged(ref createdAt, value); }
        }

        /// <summary>
        /// Gets the pull request body.
        /// </summary>
        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <summary>
        /// Gets the number of files that have been changed in the pull request.
        /// </summary>
        public int ChangedFilesCount
        {
            get { return changeCount; }
            private set { this.RaiseAndSetIfChanged(ref changeCount, value); }
        }

        /// <summary>
        /// Gets or sets a value describing how changed files are displayed in a view.
        /// </summary>
        public ChangedFilesView ChangedFilesView
        {
            get { return changedFilesView; }
            set { this.RaiseAndSetIfChanged(ref changedFilesView, value); }
        }

        /// <summary>
        /// Gets or sets a value describing how files are opened when double clicked.
        /// </summary>
        public OpenChangedFileAction OpenChangedFileAction
        {
            get { return openChangedFileAction; }
            set { this.RaiseAndSetIfChanged(ref openChangedFileAction, value); }
        }

        /// <summary>
        /// Gets the changed files as a tree.
        /// </summary>
        public IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; } = new ReactiveList<IPullRequestChangeNode>();

        /// <summary>
        /// Gets the changed files as a flat list.
        /// </summary>
        public IReactiveList<IPullRequestFileViewModel> ChangedFilesList { get; } = new ReactiveList<IPullRequestFileViewModel>();

        /// <summary>
        /// Gets a command that opens the pull request on GitHub.
        /// </summary>
        public ReactiveCommand<object> OpenOnGitHub { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="ChangedFilesView"/> property.
        /// </summary>
        public ReactiveCommand<object> ToggleChangedFilesView { get; }

        /// <summary>
        /// Gets a command that toggles the <see cref="OpenChangedFileAction"/> property.
        /// </summary>
        public ReactiveCommand<object> ToggleOpenChangedFileAction { get; }

        /// <summary>
        /// Initializes the view model with new data.
        /// </summary>
        /// <param name="data"></param>
        public override void Initialize([AllowNull] ViewWithData data)
        {
            var prNumber = (int)data.Data;

            IsBusy = true;

            // TODO: Catch errors.
            Observable.CombineLatest(
                    repositoryHost.ApiClient.GetPullRequest(repository.Owner, repository.CloneUrl.RepositoryName, prNumber),
                    repositoryHost.ApiClient.GetPullRequestFiles(repository.Owner, repository.CloneUrl.RepositoryName, prNumber).ToList(),
                    (pr, files) => new { PullRequest = pr, Files = files })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Finally(() => IsBusy = false)
                .Subscribe(x => Load(x.PullRequest, x.Files));
        }

        /// <summary>
        /// Loads the view model from octokit models.
        /// </summary>
        /// <param name="pullRequest">The pull request model.</param>
        /// <param name="files">The pull request's changed files.</param>
        public void Load(PullRequest pullRequest, IList<PullRequestFile> files)
        {
            State = CreatePullRequestState(pullRequest);
            SourceBranchDisplayName = GetBranchDisplayName(pullRequest.Head.Label);
            TargetBranchDisplayName = GetBranchDisplayName(pullRequest.Base.Label);
            CommitCount = pullRequest.Commits;
            Title = pullRequest.Title;
            Number = pullRequest.Number;
            Author = new Models.Account(pullRequest.User, GetAvatar(pullRequest.User));
            CreatedAt = pullRequest.CreatedAt;
            Body = pullRequest.Body;
            ChangedFilesCount = files.Count;

            ChangedFilesTree.Clear();
            ChangedFilesList.Clear();

            // WPF doesn't support AddRange here so iterate through the changes.
            foreach (var change in CreateChangedFilesList(files))
            {
                ChangedFilesList.Add(change);
            }

            foreach (var change in CreateChangedFilesTree(ChangedFilesList).Children)
            {
                ChangedFilesTree.Add(change);
            }
        }

        static PullRequestState CreatePullRequestState(PullRequest pullRequest)
        {
            if (pullRequest.State == ItemState.Open)
            {
                return new PullRequestState(true, "Open");
            }
            else if (pullRequest.Merged)
            {
                return new PullRequestState(false, "Merged");
            }
            else
            {
                return new PullRequestState(false, "Closed");
            }
        }

        static IEnumerable<IPullRequestFileViewModel> CreateChangedFilesList(IList<PullRequestFile> files)
        {
            return files.Select(x => new PullRequestFileViewModel(x.FileName, x.Status == "added", x.Status == "deleted"));
        }

        static IPullRequestDirectoryViewModel CreateChangedFilesTree(IEnumerable<IPullRequestFileViewModel> files)
        {
            var dirs = new Dictionary<string, PullRequestDirectoryViewModel>
            {
                { string.Empty, new PullRequestDirectoryViewModel(string.Empty) }
            };

            foreach (var file in files)
            {
                var dir = GetDirectory(file.Path, dirs);
                dir.Files.Add(file);
            }

            return dirs[string.Empty];
        }

        static PullRequestDirectoryViewModel GetDirectory(string path, Dictionary<string, PullRequestDirectoryViewModel> dirs)
        {
            PullRequestDirectoryViewModel dir;

            if (!dirs.TryGetValue(path, out dir))
            {
                var parentPath = Path.GetDirectoryName(path);
                var parentDir = GetDirectory(parentPath, dirs);

                dir = new PullRequestDirectoryViewModel(path);

                if (!parentDir.Directories.Any(x => x.DirectoryName == dir.DirectoryName))
                {
                    parentDir.Directories.Add(dir);
                    dirs.Add(path, dir);
                }
            }

            return dir;
        }

        string GetBranchDisplayName(string targetBranchLabel)
        {
            var parts = targetBranchLabel.Split(':');
            var owner = parts[0];
            return owner == repository.CloneUrl.Owner ? parts[1] : targetBranchLabel;
        }

        IObservable<BitmapSource> GetAvatar(User user)
        {
            return avatarProvider.GetAvatar(new AccountCacheItem(user))
                .Do(_ => { });
        }
    }
}
