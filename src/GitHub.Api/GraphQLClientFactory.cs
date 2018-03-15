﻿using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;

namespace GitHub.Api
{
    [Export(typeof(IGraphQLClientFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GraphQLClientFactory : IGraphQLClientFactory
    {
        readonly IKeychain keychain;
        readonly IProgram program;

        [ImportingConstructor]
        public GraphQLClientFactory(IKeychain keychain, IProgram program)
        {
            this.keychain = keychain;
            this.program = program;
        }

        public Task<Octokit.GraphQL.IConnection> CreateConnection(HostAddress address)
        {
            var credentials = new GraphQLKeychainCredentialStore(keychain, address);
            var header = new ProductHeaderValue(program.ProductHeader.Name, program.ProductHeader.Version);
            return Task.FromResult<Octokit.GraphQL.IConnection>(new Connection(header, credentials));
        }
    }
}
