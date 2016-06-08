﻿using System;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using System.ComponentModel.Composition;
using System.Windows.Controls;
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
                d(this.OneWayBind(ViewModel, vm => vm.BranchValidator, v => v.branchValidationMessage.ReactiveValidator));
                d(this.OneWayBind(ViewModel, vm => vm.TitleValidator, v => v.titleValidationMessage.ReactiveValidator));
                d(ViewModel.CancelCommand.Subscribe(_ => NotifyCancel()));
            });
        }

        private void branchSelectionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            branchPopup.IsOpen = true;
        }

        private void assigneePopupButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            assigneePopup.IsOpen = true;
        }
    }
}
