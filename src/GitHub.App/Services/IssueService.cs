using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;
using IssueState = GitHub.Models.IssueState;

namespace GitHub.Services
{
    [Export(typeof(IIssueService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IssueService : IIssueService
    {
        static ICompiledQuery<Page<ActorModel>> readAssignableUsers;
        static ICompiledQuery<Page<IssueListItemModel>> readIssues;
        static ICompiledQuery<IssueDetailModel> readIssue;

        readonly IGraphQLClientFactory graphqlFactory;

        [ImportingConstructor]
        public IssueService(IGraphQLClientFactory graphqlFactory)
        {
            this.graphqlFactory = graphqlFactory;
        }

        public async Task<Page<IssueListItemModel>> ReadIssues(
            HostAddress address,
            string owner,
            string name,
            string after,
            IssueState[] states)
        {
            if (readIssues == null)
            {
                readIssues = new Query()
                    .Repository(owner: Var(nameof(owner)), name: Var(nameof(name)))
                    .Issues(
                        first: 100,
                        after: Var(nameof(after)),
                        orderBy: new IssueOrder { Direction = OrderDirection.Desc, Field = IssueOrderField.CreatedAt },
                        states: Var(nameof(states)))
                    .Select(page => new Page<IssueListItemModel>
                    {
                        EndCursor = page.PageInfo.EndCursor,
                        HasNextPage = page.PageInfo.HasNextPage,
                        TotalCount = page.TotalCount,
                        Items = page.Nodes.Select(issue => new IssueListItemModel
                        {
                            Id = issue.Id.Value,
                            Author = new ActorModel
                            {
                                Login = issue.Author.Login,
                                AvatarUrl = issue.Author.AvatarUrl(null),
                            },
                            CommentCount = issue.Comments(0, null, null, null).TotalCount,
                            Number = issue.Number,
                            State = issue.State.FromGraphQl(),
                            Title = issue.Title,
                            UpdatedAt = issue.UpdatedAt,
                        }).ToList(),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(after), after },
                { nameof(states), states.Select(x => x.ToGraphQL()).ToList() },
            };

            return await graphql.Run(readIssues, vars).ConfigureAwait(false);
        }

        public async Task<IssueDetailModel> ReadIssue(HostAddress address, string owner, string name, int number)
        {
            if (readIssue == null)
            {
                readIssue = new Query()
                    .Repository(owner: Var(nameof(owner)), name: Var(nameof(name)))
                    .Issue(Var(nameof(number)))
                    .Select(issue => new IssueDetailModel
                    {
                        Id = issue.Id.Value,
                        Number = issue.Number,
                        Author = new ActorModel
                        {
                            Login = issue.Author.Login,
                            AvatarUrl = issue.Author.AvatarUrl(null),
                        },
                        Title = issue.Title,
                        Body = issue.Body,
                        UpdatedAt = issue.UpdatedAt,
                        Comments = issue.Comments(null, null, null, null).AllPages().Select(comment => new CommentModel
                        {
                            Id = comment.Id.Value,
                            DatabaseId = comment.DatabaseId.Value,
                            Author = new ActorModel
                            {
                                Login = comment.Author.Login,
                                AvatarUrl = comment.Author.AvatarUrl(null),
                            },
                            Body = comment.Body,
                            CreatedAt = comment.CreatedAt,                            
                            Url = comment.Url,
                        }).ToList(),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(number), number },
            };

            return await graphql.Run(readIssue, vars).ConfigureAwait(false);
        }

        public async Task<Page<ActorModel>> ReadAssignableUsers(
            HostAddress address,
            string owner,
            string name,
            string after)
        {
            if (readAssignableUsers == null)
            {
                readAssignableUsers = new Query()
                    .Repository(owner: Var(nameof(owner)), name: Var(nameof(name)))
                    .AssignableUsers(first: 100, after: Var(nameof(after)))
                    .Select(connection => new Page<ActorModel>
                    {
                        EndCursor = connection.PageInfo.EndCursor,
                        HasNextPage = connection.PageInfo.HasNextPage,
                        TotalCount = connection.TotalCount,
                        Items = connection.Nodes.Select(user => new ActorModel
                        {
                            AvatarUrl = user.AvatarUrl(30),
                            Login = user.Login,
                        }).ToList(),
                    }).Compile();
            }

            var graphql = await graphqlFactory.CreateConnection(address).ConfigureAwait(false);
            var vars = new Dictionary<string, object>
            {
                { nameof(owner), owner },
                { nameof(name), name },
                { nameof(after), after },
            };

            return await graphql.Run(readAssignableUsers, vars).ConfigureAwait(false);
        }
    }
}
