﻿using System;
using System.ComponentModel.Composition;
using GitHub.Services;

namespace GitHub.VisualStudio.TeamFoundation
{
    public class TeamExplorerServicesFactory
    {
        [ImportingConstructor]
        public TeamExplorerServicesFactory(
            [TeamFoundationResolver] IResolver resolver,
            IGitHubServiceProvider serviceProvider)
        {
            TeamExplorerServices = new TeamExplorerServices(serviceProvider);
        }

        [Export(typeof(ITeamExplorerServices))]
        public ITeamExplorerServices TeamExplorerServices { get; }
    }
}
