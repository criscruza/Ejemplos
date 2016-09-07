using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IGitService
    {
        /// <summary>
        /// Returns the URL of the remote for the specified <see cref="repository"/>. If the repository
        /// is null or no remote exists, this method returns null
        /// </summary>
        /// <param name="repository">The repository to look at for the remote.</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        UriString GetUri(IRepository repository, string remote = "origin");

        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="UriString"/> for the repository's
        /// remote if one is found
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>A <see cref="UriString"/> representing the origin or null if none found.</returns>
        UriString GetUri(string path, string remote = "origin");
        
        /// <summary>
        /// Probes for a git repository and if one is found, returns a <see cref="IRepository"/> instance for the
        /// repository.
        /// </summary>
        /// <remarks>
        /// The lookup checks to see if the specified <paramref name="path"/> is a repository. If it's not, it then
        /// walks up the parent directories until it either finds a repository, or reaches the root disk.
        /// </remarks>
        /// <param name="path">The path to start probing</param>
        /// <returns>An instance of <see cref="IRepository"/> or null</returns>
        IRepository GetRepository(string path);

        /// <summary>
        /// Returns a <see cref="UriString"/> representing the uri of a remote.
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        UriString GetRemoteUri(IRepository repo, string remote = "origin");

        Task<string> GetLatestPushedSha(string path);
    }
}