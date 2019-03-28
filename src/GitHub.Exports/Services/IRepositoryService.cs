﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IRepositoryService
    {
        /// <summary>
        /// Finds the parent repository of a fork, if any.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <returns>
        /// A tuple of the parent repository's owner and name if the repository is a fork,
        /// otherwise null.
        /// </returns>
        Task<(string owner, string name)?> FindParent(HostAddress address, string owner, string name);

        /// <summary>
        /// Gets the list of protected branches.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <returns></returns>
        Task<IList<ProtectedBranch>> GetProtectedBranches(HostAddress address, string owner, string name);

        /// <summary>
        /// Gets the protected branch configuration of a particular branch, if any.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="branchName"></param>
        /// <returns></returns>
        Task<ProtectedBranch> GetProtectedBranch(HostAddress address, string owner, string name, string branchName);
    }
}
