﻿using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.SampleData
{
    class GitServiceDesigner : IGitService
    {
        public Task<string> GetLatestPushedSha(string path) => Task.FromResult<string>(null);
        public UriString GetRemoteUri(IRepository repo, string remote = "origin") => null;
        public IRepository GetRepository(string path) => null;
        public UriString GetUri(string path, string remote = "origin") => null;
        public UriString GetUri(IRepository repository, string remote = "origin") => null;
    }
}
