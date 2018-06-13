﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;
using GitHub.Primitives;
using System.Collections.Generic;

public class GitClientTests
{
    public class TheIsModifiedMethod
    {
        [TestCase(FileStatus.Unaltered, false)]
        [TestCase(FileStatus.ModifiedInIndex, true)]
        [TestCase(FileStatus.ModifiedInWorkdir, true)]
        public async Task RetrieveStatus(FileStatus fileStatus, bool expect)
        {
            var path = "path";
            var repo = Substitute.For<IRepository>();
            repo.RetrieveStatus(path).Returns(fileStatus);
            repo.Head.Returns(Substitute.For<Branch>());
            var treeEntry = null as TreeEntry;
            repo.Head[path].Returns(treeEntry);
            var gitClient = CreateGitClient();

            var modified = await gitClient.IsModified(repo, path, null);

            Assert.That(expect, Is.EqualTo(modified));
        }

        [Test]
        public async Task TreeEntry_Null_False()
        {
            var path = "path";
            var repo = Substitute.For<IRepository>();
            repo.RetrieveStatus(path).Returns(FileStatus.Unaltered);
            repo.Head.Returns(Substitute.For<Branch>());
            var treeEntry = null as TreeEntry;
            repo.Head[path].Returns(treeEntry);
            var gitClient = CreateGitClient();

            var modified = await gitClient.IsModified(repo, path, null);

            Assert.False(modified);
        }

        [Test]
        public async Task TreeEntryTarget_GitLink_False()
        {
            var path = "path";
            var repo = Substitute.For<IRepository>();
            repo.RetrieveStatus(path).Returns(FileStatus.Unaltered);
            repo.Head.Returns(Substitute.For<Branch>());
            var treeEntry = Substitute.For<TreeEntry>();
            treeEntry.TargetType.Returns(TreeEntryTargetType.GitLink);
            treeEntry.Target.Returns(Substitute.For<GitLink>());
            repo.Head[path].Returns(treeEntry);
            var gitClient = CreateGitClient();

            var modified = await gitClient.IsModified(repo, path, null);

            Assert.False(modified);
        }

        [TestCase(0, 0, false)]
        [TestCase(1, 0, true)]
        [TestCase(0, 1, true)]
        [TestCase(1, 1, true)]
        public async Task ContentChanges(int linesAdded, int linesDeleted, bool expected)
        {
            var path = "path";
            var repo = Substitute.For<IRepository>();
            repo.RetrieveStatus(path).Returns(FileStatus.Unaltered);
            repo.Head.Returns(Substitute.For<Branch>());
            var treeEntry = Substitute.For<TreeEntry>();
            treeEntry.TargetType.Returns(TreeEntryTargetType.Blob);
            treeEntry.Target.Returns(Substitute.For<Blob>());
            repo.Head[path].Returns(treeEntry);
            var changes = Substitute.For<ContentChanges>();
            changes.LinesAdded.Returns(linesAdded);
            changes.LinesDeleted.Returns(linesDeleted);
            repo.Diff.Compare(null, null).ReturnsForAnyArgs(changes);
            var gitClient = CreateGitClient();

            var modified = await gitClient.IsModified(repo, path, null);

            Assert.That(expected, Is.EqualTo(modified));
        }
    }

    public class TheIsHeadPushedMethod : TestBaseClass
    {
        [TestCase(0, true)]
        [TestCase(2, false)]
        [TestCase(null, false)]
        public async Task IsHeadPushed(int? aheadBy, bool expected)
        {
            var gitClient = CreateGitClient();
            var repository = MockTrackedBranchRepository(aheadBy);

            var isHeadPushed = await gitClient.IsHeadPushed(repository);

            Assert.That(expected, Is.EqualTo(isHeadPushed));
        }

        static IRepository MockTrackedBranchRepository(int? aheadBy)
        {
            var headBranch = Substitute.For<Branch>();
            var trackingDetails = Substitute.For<BranchTrackingDetails>();
            trackingDetails.AheadBy.Returns(aheadBy);
            headBranch.TrackingDetails.Returns(trackingDetails);
            var repository = Substitute.For<IRepository>();
            repository.Head.Returns(headBranch);
            return repository;
        }
    }

    public class ThePushMethod : TestBaseClass
    {
        [Test]
        public async Task PushesToDefaultOrigin()
        {
            var origin = Substitute.For<Remote>();
            var head = Substitute.For<Branch>();
            head.Commits.Returns(new FakeCommitLog { Substitute.For<Commit>() });
            var repository = Substitute.For<IRepository>();
            repository.Head.Returns(head);
            repository.Network.Remotes["origin"].Returns(origin);
            var gitClient = CreateGitClient();

            await gitClient.Push(repository, "master", "origin");

            repository.Network.Received().Push(origin, "HEAD", @"refs/heads/master", Arg.Any<PushOptions>());
        }

        [Test]
        public async Task DoesNotPushEmptyRepository()
        {
            var repository = Substitute.For<IRepository>();
            var gitClient = CreateGitClient();

            await gitClient.Push(repository, "master", "origin");

            repository.Network.DidNotReceive()
                .Push(Args.LibgGit2Remote, Args.String, Args.String);
        }
    }

    public class TheSetRemoteMethod : TestBaseClass
    {
        [Test]
        public async Task SetsTheConfigToTheRemoteBranch()
        {
            var config = Substitute.For<Configuration>();
            var repository = Substitute.For<IRepository>();
            repository.Config.Returns(config);
            var gitClient = CreateGitClient();

            await gitClient.SetRemote(repository, "origin", new Uri("https://github.com/foo/bar"));

            config.Received().Set<string>("remote.origin.url", "https://github.com/foo/bar");
            config.Received().Set<string>("remote.origin.fetch", "+refs/heads/*:refs/remotes/origin/*");
        }
    }

    public class TheSetTrackingMethod : TestBaseClass
    {
        [Test]
        public async Task SetsTheRemoteTrackingBranch()
        {
            var config = Substitute.For<Configuration>();
            var origin = Substitute.For<Remote>();
            var branches = Substitute.For<BranchCollection>();
            var repository = Substitute.For<IRepository>();
            repository.Config.Returns(config);
            repository.Branches.Returns(branches);
            repository.Network.Remotes["origin"].Returns(origin);
            var localBranch = Substitute.For<Branch>();
            var remoteBranch = Substitute.For<Branch>(); ;
            branches["refs/heads/master"].Returns(localBranch);
            branches["refs/remotes/origin/master"].Returns(remoteBranch);

            var gitClient = CreateGitClient();

            await gitClient.SetTrackingBranch(repository, "master", "origin");

            branches.Received().Update(localBranch, Arg.Any<Action<BranchUpdater>>());
        }
    }

    public class TheFetchMethod : TestBaseClass
    {
        [TestCase("https://github.com/owner/repo", "https://github.com/owner/repo")]
        [TestCase("git@github.com:github/VisualStudioBuildScripts", "https://github.com/github/VisualStudioBuildScripts")]
        public async Task FetchUsingHttps(string repoUrl, string expectFetchUrl)
        {
            var repo = Substitute.For<IRepository>();
            var uri = new UriString(repoUrl);
            var refSpec = "refSpec";
            var gitClient = CreateGitClient();
            var expectUrl = UriString.ToUriString(uri.ToRepositoryUrl());

            await gitClient.Fetch(repo, uri, refSpec);

            repo.Network.Remotes.Received(1).Add(Arg.Any<string>(), expectFetchUrl);
        }

        [TestCase("https://github.com/owner/repo", "https://github.com/owner/repo", null)]
        [TestCase("https://github.com/fetch/repo", "https://github.com/origin/repo", "https://github.com/fetch/repo")]
        [TestCase("git@github.com:owner/repo", "git@github.com:owner/repo", "https://github.com/owner/repo")]
        public async Task UseOriginWhenPossible(string fetchUrl, string originUrl, string addUrl = null)
        {
            var remote = Substitute.For<Remote>();
            remote.Url.Returns(originUrl);
            var repo = Substitute.For<IRepository>();
            repo.Network.Remotes["origin"].Returns(remote);
            var fetchUri = new UriString(fetchUrl);
            var refSpec = "refSpec";
            var gitClient = CreateGitClient();

            await gitClient.Fetch(repo, fetchUri, refSpec);

            if (addUrl != null)
            {
                repo.Network.Remotes.Received().Add(Arg.Any<string>(), addUrl);
            }
            else
            {
                repo.Network.Remotes.DidNotReceiveWithAnyArgs().Add(null, null);
            }
        }
    }

    public class TheGetPullRequestMergeBaseMethod : TestBaseClass
    {
        [Test]
        public async Task LocalBaseHeadAndMergeBase_DontFetch()
        {
            var targetCloneUrl = new UriString("https://github.com/owner/repo");
            var baseSha = "baseSha";
            var headSha = "headSha";
            var expectMergeBaseSha = "mergeBaseSha";
            var baseRef = "master";
            var pullNumber = 0;
            var repo = MockRepo(baseSha, headSha, expectMergeBaseSha);
            var gitClient = CreateGitClient();

            var mergeBaseSha = await gitClient.GetPullRequestMergeBase(repo, targetCloneUrl, baseSha, headSha, baseRef, pullNumber);

#pragma warning disable 618 // Type or member is obsolete
            repo.Network.DidNotReceiveWithAnyArgs().Fetch(null as Remote, null, null as FetchOptions);
#pragma warning restore 618 // Type or member is obsolete
            Assert.That(expectMergeBaseSha, Is.EqualTo(mergeBaseSha));
        }

        [TestCase("baseSha", "headSha", "mergeBaseSha", 0)]
        [TestCase(null, "headSha", "mergeBaseSha", 1)]
        [TestCase("baseSha", null, "mergeBaseSha", 1)]
        [TestCase("baseSha", "headSha", null, 0)]
        public async Task WhenToFetch(string baseSha, string headSha, string mergeBaseSha, int receivedFetch)
        {
            var targetCloneUri = new UriString("https://github.com/owner/repo");
            var baseRef = "master";
            var pullNumber = 0;
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var remote = Substitute.For<Remote>();
            repo.Network.Remotes.Add(null, null).ReturnsForAnyArgs(remote);
            var gitClient = CreateGitClient();

            try
            {
                await gitClient.GetPullRequestMergeBase(repo, targetCloneUri, baseSha, headSha, baseRef, pullNumber);
            }
            catch (NotFoundException) { /* We're interested in calls to Fetch even if it throws */ }

#pragma warning disable 618 // Type or member is obsolete
            repo.Network.Received(receivedFetch).Fetch(Arg.Any<Remote>(), Arg.Any<string[]>(), Arg.Any<FetchOptions>());
#pragma warning restore 618 // Type or member is obsolete
        }

        [TestCase("baseSha", null, "mergeBaseSha", "baseRef", 777, "refs/pull/777/head")]
        [TestCase(null, "headSha", "mergeBaseSha", "baseRef", 777, "baseRef")]

        // PR base might not exist, so we must fetch `refs/pull/<PR>/head` first.
        [TestCase(null, null, "mergeBaseSha", "baseRef", 777, "refs/pull/777/head")]
        public async Task WhatToFetch(string baseSha, string headSha, string mergeBaseSha, string baseRef, int pullNumber,
            string expectRefSpec)
        {
            var repo = MockRepo(baseSha, headSha, mergeBaseSha);
            var targetCloneUri = new UriString("https://github.com/owner/repo");
            var gitClient = CreateGitClient();

            try
            {
                await gitClient.GetPullRequestMergeBase(repo, targetCloneUri, baseSha, headSha, baseRef, pullNumber);
            }
            catch (NotFoundException) { /* We're interested in calls to Fetch even if it throws */ }

#pragma warning disable 618 // Type or member is obsolete
            repo.Network.Received(1).Fetch(Arg.Any<Remote>(), Arg.Is<IEnumerable<string>>(x => x.Contains(expectRefSpec)), Arg.Any<FetchOptions>());
#pragma warning restore 618 // Type or member is obsolete
        }

        static IRepository MockRepo(string baseSha, string headSha, string mergeBaseSha)
        {
            var repo = Substitute.For<IRepository>();
            var baseCommit = Substitute.For<Commit>();
            var headCommit = Substitute.For<Commit>();
            var mergeBaseCommit = Substitute.For<Commit>();
            mergeBaseCommit.Sha.Returns(mergeBaseSha);

            if (baseSha != null)
            {
                repo.Lookup<Commit>(baseSha).Returns(baseSha != null ? baseCommit : null);
            }

            if (headSha != null)
            {
                repo.Lookup<Commit>(headSha).Returns(headSha != null ? headCommit : null);
            }

            repo.ObjectDatabase.FindMergeBase(baseCommit, headCommit).Returns(mergeBaseCommit);
            return repo;
        }
    }

    static GitClient CreateGitClient()
    {
        var gitService = Substitute.For<IGitService>();
        gitService.GetOriginRemoteName(null).ReturnsForAnyArgs("origin");
        return new GitClient(
            Substitute.For<IGitHubCredentialProvider>(),
            gitService);
    }
}
