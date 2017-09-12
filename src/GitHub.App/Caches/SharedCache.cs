using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Logging;
using Serilog;

namespace GitHub.Caches
{
    /// <summary>
    /// A cache for data that's not host specific
    /// </summary>
    [Export(typeof(ISharedCache))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedCache : ISharedCache
    {
        const string enterpriseHostApiBaseUriCacheKey = "enterprise-host-api-base-uri";
        static readonly ILogger log = LogManager.ForContext<SharedCache>();

        static SharedCache()
        {
            try
            {
                BlobCache.ApplicationName = Info.ApplicationInfo.ApplicationName;
            }
            catch (Exception e)
            {
                log.Error(e, "Error while running the static inializer for SharedCache");
            }
        }

        public SharedCache() : this(null, null, null)
        {
        }

        protected SharedCache(IBlobCache userAccountCache, IBlobCache localMachineCache, ISecureBlobCache secureCache)
        {
            if (secureCache == null)
            {
                try
                {
                    BlobCache.Secure = new CredentialCache();
                    secureCache = BlobCache.Secure;
                }
                catch (Exception e)
                {
                    log.Error(e, "Failed to set up secure cache.");
                    secureCache = new InMemoryBlobCache();
                }
            }
            UserAccount = userAccountCache ?? GetBlobCacheWithFallback(() => BlobCache.UserAccount, "UserAccount");
            LocalMachine = localMachineCache ?? GetBlobCacheWithFallback(() => BlobCache.LocalMachine, "LocalMachine");
            Secure = secureCache;
        }

        public IBlobCache UserAccount { get; private set; }
        public IBlobCache LocalMachine { get; private set; }
        public ISecureBlobCache Secure { get; private set; }

        public IObservable<Uri> GetEnterpriseHostUri()
        {
            return UserAccount.GetObject<Uri>(enterpriseHostApiBaseUriCacheKey);
        }

        public IObservable<Unit> InsertEnterpriseHostUri(Uri enterpriseHostUri)
        {
            return UserAccount.InsertObject(enterpriseHostApiBaseUriCacheKey, enterpriseHostUri);
        }

        public IObservable<Unit> InvalidateEnterpriseHostUri()
        {
            return UserAccount.InvalidateObject<Uri>(enterpriseHostApiBaseUriCacheKey);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                UserAccount.Dispose();
                UserAccount.Shutdown.Wait();
                LocalMachine.Dispose();
                LocalMachine.Shutdown.Wait();
                Secure.Dispose();
                Secure.Shutdown.Wait();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        static IBlobCache GetBlobCacheWithFallback(Func<IBlobCache> blobCacheFunc, string cacheName)
        {
            try
            {
                return blobCacheFunc();
            }
            catch (Exception e)
            {
                log.Error(e, "Failed to set the {cacheName} cache.", cacheName);
                return new InMemoryBlobCache();
            }
        }
    }
}
