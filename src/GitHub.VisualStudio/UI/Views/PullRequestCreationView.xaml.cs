﻿using System;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestCreationView : SimpleViewUserControl<IPullRequestCreationViewModel, GenericPullRequestCreationView>
    { }

    [ExportView(ViewType = UIViewType.PRCreation)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestCreationView : GenericPullRequestCreationView
    {
        public PullRequestCreationView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(ViewModel.CancelCommand.Subscribe(_ => NotifyCancel()));
                d(ViewModel.CreatePullRequest.Subscribe(_ => NotifyDone()));
            });
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            titleText.Text = string.Empty;
            descriptionText.Text = string.Empty;
        }
    }
}
