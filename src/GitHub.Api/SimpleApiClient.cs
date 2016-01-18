﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;

namespace GitHub.Api
{
    public class SimpleApiClient : ISimpleApiClient
    {
        public HostAddress HostAddress { get; private set; }
        public Uri OriginalUrl { get; private set; }

        readonly GitHubClient client;
        readonly Lazy<IEnterpriseProbeTask> enterpriseProbe;
        readonly Lazy<IWikiProbe> wikiProbe;
        static readonly SemaphoreSlim sem = new SemaphoreSlim(1);

        Repository repositoryCache = new Repository();
        string owner;
        bool? isEnterprise;
        bool? hasWiki;

        internal SimpleApiClient(HostAddress hostAddress, Uri repoUrl, GitHubClient githubClient,
            Lazy<IEnterpriseProbeTask> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            HostAddress = hostAddress;
            OriginalUrl = repoUrl;
            client = githubClient;
            this.enterpriseProbe = enterpriseProbe;
            this.wikiProbe = wikiProbe;
        }

        public async Task<Repository> GetRepository()
        {
            // fast path to avoid locking when the cache has already been set
            // once it's been set, it's never going to be touched again, so it's safe
            // to read. This way, lock queues will only form once on first load
            if (owner != null)
                return repositoryCache;
            return await GetRepositoryInternal();
        }

        async Task<Repository> GetRepositoryInternal()
        {
            await sem.WaitAsync();
            try
            {
                if (owner == null && OriginalUrl != null)
                {
                    var own = OriginalUrl.GetUser();
                    var name = OriginalUrl.GetRepo();

                    if (own != null && name != null)
                    {
                        var repo = await client.Repository.Get(own, name);
                        if (repo != null)
                        {
                            hasWiki = await HasWikiInternal(repo);
                            isEnterprise = await IsEnterpriseInternal();
                            repositoryCache = repo;
                        }
                        owner = own;
                    }
                }
            }
            // it'll throw if it's private
            catch {}
            finally
            {
                sem.Release();
            }

            return repositoryCache;
        }

        public bool HasWiki()
        {
            if (hasWiki.HasValue)
                return hasWiki.Value;
            return false;
        }

        public bool IsEnterprise()
        {
            if (isEnterprise.HasValue)
                return isEnterprise.Value;
            return false;
        }

        async Task<bool> HasWikiInternal(Repository repo)
        {
            if (repo == null)
                return false;

            if (!repo.HasWiki)
            {
                hasWiki = false;
                return false;
            }

            var probe = wikiProbe.Value;
            Debug.Assert(probe != null, "Lazy<Wiki> probe is not set, something is wrong.");
#if !DEBUG
            if (probe == null)
                return false;
#endif
            var ret = await probe.ProbeAsync(repo);
            if (ret == WikiProbeResult.Failed)
                return false;
            return (ret == WikiProbeResult.Ok);
        }

        async Task<bool> IsEnterpriseInternal()
        {
            var probe = enterpriseProbe.Value;
            Debug.Assert(probe != null, "Lazy<Enterprise> probe is not set, something is wrong.");
#if !DEBUG
            if (probe == null)
                return false;
#endif
            var ret = await probe.ProbeAsync(HostAddress.WebUri);
            if (ret == EnterpriseProbeResult.Failed)
                return false;
            return (ret == EnterpriseProbeResult.Ok);
        }
    }
}
