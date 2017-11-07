﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static IObservable<bool> IsLoggedIn(this IConnectionManager cm, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));

            return Observable.FromAsync(async () =>
            {
                var connections = await cm.GetLoadedConnections();
                return connections.Any(x => x.ConnectionError == null);
            });
        }

        public static IObservable<bool> IsLoggedIn(this IConnectionManager cm, IRepositoryHosts hosts, HostAddress address)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));
            Guard.ArgumentNotNull(address, nameof(address));

            return Observable.FromAsync(async () =>
            {
                var connections = await cm.GetLoadedConnections();
                return connections.Any(x => x.HostAddress == address && x.ConnectionError == null);
            });
        }

        public static IObservable<bool> IsLoggedIn(this IConnection connection, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));

            return Observable.Return(connection?.IsLoggedIn ?? false);
        }

        public static IObservable<IConnection> GetLoggedInConnections(this IConnectionManager cm, IRepositoryHosts hosts)
        {
            Guard.ArgumentNotNull(hosts, nameof(hosts));

            return cm.GetLoadedConnections()
                .ToObservable()
                .Select(x => x.FirstOrDefault(y => y.IsLoggedIn));
        }

        public static IObservable<IConnection> LookupConnection(this IConnectionManager cm, ILocalRepositoryModel repository)
        {
            return Observable.Return(repository?.CloneUrl != null
                ? cm.Connections.FirstOrDefault(c => c.HostAddress.Equals(HostAddress.Create(repository.CloneUrl)))
                : null);
        }
    }
}
