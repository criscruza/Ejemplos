﻿using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using GitHub.VisualStudio.Base;

namespace GitHub.VisualStudio.TeamFoundation
{
    public class TeamExplorerServiceHolderFactory
    {
        [ImportingConstructor]
        public TeamExplorerServiceHolderFactory([TeamFoundationResolver] IResolver resolver)
        {
            TeamExplorerServiceHolder = new TeamExplorerServiceHolder();
        }

        [Export(typeof(ITeamExplorerServiceHolder))]
        public ITeamExplorerServiceHolder TeamExplorerServiceHolder { get; }
    }
}
