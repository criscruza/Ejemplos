using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Validation;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : PanePageViewModelBase, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            Branches = new List<IBranch>
            {
                new BranchModel("master", new LocalRepositoryModel("http://github.com/user/repo", new GitServiceDesigner())),
                new BranchModel("don/stub-ui", new LocalRepositoryModel("http://github.com/user/repo", new GitServiceDesigner())),
                new BranchModel("feature/pr/views", new LocalRepositoryModel("http://github.com/user/repo", new GitServiceDesigner())),
                new BranchModel("release-1.0.17.0", new LocalRepositoryModel("http://github.com/user/repo", new GitServiceDesigner())),
            }.AsReadOnly();

            TargetBranch = new BranchModel("master", new LocalRepositoryModel("http://github.com/user/repo", new GitServiceDesigner()));
            SourceBranch = Branches[2];

            SelectedAssignee = "Haacked (Phil Haack)";
            Users = new List<string>()
            {
                "Haacked (Phil Haack)",
                "shana (Andreia Gaita)"
            };
        }

        public IBranch SourceBranch { get; set; }
        public IBranch TargetBranch { get; set; }
        public IReadOnlyList<IBranch> Branches { get; set; }

        public string SelectedAssignee { get; set; }
        public List<string> Users { get; set; }

        public IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }
        public IReactiveCommand<object> Cancel { get; }

        public string PRTitle { get; set; }

        public ReactivePropertyValidator TitleValidator { get; }

        public ReactivePropertyValidator BranchValidator { get; }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}