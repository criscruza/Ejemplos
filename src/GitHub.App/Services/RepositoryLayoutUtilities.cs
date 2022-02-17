﻿using System;
using System.IO;
using GitHub.Primitives;

namespace GitHub.Services
{
    public static class RepositoryLayoutUtilities
    {
        // Exclude directories with a case sensitive name of "GitHub" from being a possible owner.
        // GitHub Desktop uses "GitHub" as its default directory and this isn't a user or organization name.
        const string GitHubDirectoryName = "GitHub";

        public static string GetDefaultRepositoryPath(UriString cloneUrl, string defaultPath, RepositoryLayout repositoryLayout)
        {
            switch (repositoryLayout)
            {
                case RepositoryLayout.Name:
                    return Path.Combine(defaultPath, cloneUrl.RepositoryName);
                case RepositoryLayout.Default:
                case RepositoryLayout.OwnerName:
                    return Path.Combine(defaultPath, cloneUrl.Owner, cloneUrl.RepositoryName);
                default:
                    throw new ArgumentException($"Unknown repository layout: {repositoryLayout}");

            }
        }

        public static RepositoryLayout GetRepositoryLayout(string repositoryLayoutSetting)
        {
            return Enum.TryParse(repositoryLayoutSetting, out RepositoryLayout repositoryLayout) ?
                repositoryLayout : RepositoryLayout.Default;
        }

        public static (string, RepositoryLayout) GetDefaultPathAndLayout(string repositoryPath, UriString cloneUrl)
        {
            var possibleOwnerPath = Path.GetDirectoryName(repositoryPath);
            var possibleOwner = Path.GetFileName(possibleOwnerPath);
            if (string.Equals(possibleOwner, cloneUrl.Owner, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(possibleOwner, GitHubDirectoryName, StringComparison.Ordinal))
            {
                return (Path.GetDirectoryName(possibleOwnerPath), RepositoryLayout.OwnerName);
            }
            else
            {
                return (possibleOwnerPath, RepositoryLayout.Name);
            }
        }
    }
}
