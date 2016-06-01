﻿using System;
using System.Collections.Generic;
using System.Reactive;
using GitHub.Models;
using GitHub.Caches;
using GitHub.Collections;
using System.Collections.ObjectModel;

namespace GitHub.Services
{
    /// <summary>
    /// Class used to retrieve GitHub API data and turn them into models suitable for binding to in the UI.
    /// This handles the API retrieval and caching.
    /// </summary>
    public interface IModelService : IDisposable
    {
        IObservable<AccountCacheItem> GetUserFromCache();
        IObservable<Unit> InsertUser(AccountCacheItem user);
        IObservable<IReadOnlyList<IAccount>> GetAccounts();
        IObservable<IReadOnlyList<IRepositoryModel>> GetRepositories();
        IObservable<IReadOnlyList<LicenseItem>> GetLicenses();
        IObservable<IReadOnlyList<GitIgnoreItem>> GetGitIgnoreTemplates();
        ITrackingCollection<IPullRequestModel> GetPullRequests(ISimpleRepositoryModel repo,
            ITrackingCollection<IPullRequestModel> collection = null);
        IObservable<Unit> InvalidateAll();
        IObservable<IReadOnlyList<IBranchModel>> GetBranches(ISimpleRepositoryModel repo);
        IObservable<IReadOnlyList<IAccount>> GetAvailableAssignees(ISimpleRepositoryModel repo);
    }
}
