﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.Dialog.Clone;
using NSubstitute;
using NUnit.Framework;

public class RepositorySelectViewModelTests
{
    public class TheFilterProperty
    {
        [TestCase("unknown", "owner", "name", "https://github.com/owner/name", 0)]
        [TestCase("", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("owner", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("name", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("owner/name", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("OWNER/NAME", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("https://github.com/owner/name", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("HTTPS://GITHUB.COM/OWNER/NAME", "owner", "name", "https://github.com/owner/name", 1)]
        public async Task Filter(string filter, string owner, string name, string url, int expectCount)
        {
            var contributedToRepositories = new[]
            {
                new RepositoryListItemModel
                {
                    Owner = owner,
                    Name = name,
                    Url = new Uri(url)
                }
            };
            var hostAddress = HostAddress.GitHubDotComHostAddress;
            var connection = CreateConnection(hostAddress);
            var repositoryCloneService = CreateRepositoryCloneService(contributedToRepositories, hostAddress);
            var gitHubContextService = CreateGitHubContextService();
            var target = new RepositorySelectViewModel(repositoryCloneService, gitHubContextService);
            target.Filter = filter;
            target.Initialize(connection);

            await target.Activate();

            var items = target.ItemsView.Groups
                .Cast<CollectionViewGroup>()
                .SelectMany(g => g.Items)
                .Cast<RepositoryItemViewModel>();
            Assert.That(items.Count, Is.EqualTo(expectCount));
        }

        [TestCase("filter", null)]
        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github/VisualStudio", "https://github.com/github/VisualStudio")]
        public void Set_Repository_When_Filter_Is_Url(string url, string expectUrl)
        {
            var expectCloneUrl = expectUrl != null ? new UriString(expectUrl) : null;
            var repositoryCloneService = CreateRepositoryCloneService();
            var gitHubContextService = new GitHubContextService(Substitute.For<IGitHubServiceProvider>(),
                Substitute.For<IGitService>(), Substitute.For<IVSServices>());
            var target = new RepositorySelectViewModel(repositoryCloneService, gitHubContextService);

            target.Filter = url;

            Assert.That(target.Repository?.CloneUrl, Is.EqualTo(expectCloneUrl));
        }
    }

    static IGitHubContextService CreateGitHubContextService()
    {
        return Substitute.For<IGitHubContextService>();
    }

    static IConnection CreateConnection(HostAddress hostAddress)
    {
        var connection = Substitute.For<IConnection>();
        connection.HostAddress.Returns(hostAddress);
        return connection;
    }

    static IRepositoryCloneService CreateRepositoryCloneService(
        IList<RepositoryListItemModel> contributedToRepositories = null,
        HostAddress hostAddress = null)
    {
        contributedToRepositories = contributedToRepositories ?? Array.Empty<RepositoryListItemModel>();
        hostAddress = hostAddress ?? HostAddress.GitHubDotComHostAddress;

        var viewRepositoriesModel = CreateViewerRepositoriesModel(contributedToRepositories: contributedToRepositories);
        var repositoryCloneService = Substitute.For<IRepositoryCloneService>();
        repositoryCloneService.ReadViewerRepositories(hostAddress).Returns(viewRepositoriesModel);
        return repositoryCloneService;
    }

    private static ViewerRepositoriesModel CreateViewerRepositoriesModel(
        string owner = "owner",
        IList<RepositoryListItemModel> repositories = null,
        IList<RepositoryListItemModel> contributedToRepositories = null)
    {
        repositories = repositories ?? Array.Empty<RepositoryListItemModel>();
        contributedToRepositories = contributedToRepositories ?? Array.Empty<RepositoryListItemModel>();

        return new ViewerRepositoriesModel
        {
            Owner = owner,
            Repositories = CreateRepositoriesList(repositories),
            ContributedToRepositories = CreateRepositoriesList(contributedToRepositories),
            Organizations = CreateOrganizationsList()
        };
    }

    static IReadOnlyList<RepositoryListItemModel> CreateRepositoriesList(IList<RepositoryListItemModel> repositories)
    {
        return repositories.ToList().AsReadOnly();
    }

    static IDictionary<string, IReadOnlyList<RepositoryListItemModel>> CreateOrganizationsList()
    {
        return new Dictionary<string, IReadOnlyList<RepositoryListItemModel>>();
    }
}
