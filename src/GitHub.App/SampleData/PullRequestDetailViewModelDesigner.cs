using System;
using System.Diagnostics.CodeAnalysis;
using GitHub.Models;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestDetailViewModelDesigner : BaseViewModel, IPullRequestDetailViewModel
    {
        public PullRequestDetailViewModelDesigner()
        {
            Title = "Error handling/bubbling from viewmodels to views to viewhosts";
            State = new PullRequestState { Name = "Open", IsOpen = true };
            SourceBranchDisplayName = "shana/error-handling";
            TargetBranchDisplayName = "master";
            CommitCount = 9;
            Author = new AccountDesigner { Login = "shana", IsUser = true };
            CreatedAt = DateTime.Now.Subtract(TimeSpan.FromDays(3));
            Number = 419;
            Body = @"Adds a way to surface errors from the view model to the view so that view hosts can get to them.

ViewModels are responsible for handling the UI on the view they control, but they shouldn't be handling UI for things outside of the view. In this case, we're showing errors in VS outside the view, and that should be handled by the section that is hosting the view.

This requires that errors be propagated from the viewmodel to the view and from there to the host via the IView interface, since hosts don't usually know what they're hosting.

![An image](https://cloud.githubusercontent.com/assets/1174461/18882991/5dd35648-8496-11e6-8735-82c3a182e8b4.png)";

            var gitHubDir = new PullRequestDirectoryViewModel("GitHub");
            var modelsDir = new PullRequestDirectoryViewModel("Models");
            var repositoriesDir = new PullRequestDirectoryViewModel("Repositories");
            var itrackingBranch = new PullRequestFileViewModel(@"GitHub\Models\ITrackingBranch.cs", false, false);
            var oldBranchModel = new PullRequestFileViewModel(@"GitHub\Models\OldBranchModel.cs", false, false);
            var concurrentRepositoryConnection = new PullRequestFileViewModel(@"GitHub\Repositories\ConcurrentRepositoryConnection.cs", false, true);

            repositoriesDir.Files.Add(concurrentRepositoryConnection);
            modelsDir.Directories.Add(repositoriesDir);
            modelsDir.Files.Add(itrackingBranch);
            modelsDir.Files.Add(oldBranchModel);
            gitHubDir.Directories.Add(modelsDir);

            ChangedFilesCount = 3;
            ChangedFilesTree = new ReactiveList<IPullRequestChangeNode>();
            ChangedFilesTree.Add(gitHubDir);

            ChangedFilesList = new ReactiveList<IPullRequestFileViewModel>();
            ChangedFilesList.Add(concurrentRepositoryConnection);
            ChangedFilesList.Add(itrackingBranch);
            ChangedFilesList.Add(oldBranchModel);
        }

        public PullRequestState State { get; }
        public string SourceBranchDisplayName { get; }
        public string TargetBranchDisplayName { get; }
        public int CommitCount { get; }
        public IAccount Author { get; }
        public DateTimeOffset CreatedAt { get; }
        public int Number { get; }
        public string Body { get; }
        public int ChangedFilesCount { get; }
        public ChangedFilesView ChangedFilesView { get; set; }
        public OpenChangedFileAction OpenChangedFileAction { get; set; }
        public IReactiveList<IPullRequestChangeNode> ChangedFilesTree { get; }
        public IReactiveList<IPullRequestFileViewModel> ChangedFilesList { get; }

        public ReactiveCommand<object> OpenOnGitHub { get; }
        public ReactiveCommand<object> ToggleChangedFilesView { get; }
        public ReactiveCommand<object> ToggleOpenChangedFileAction { get; }
    }
}