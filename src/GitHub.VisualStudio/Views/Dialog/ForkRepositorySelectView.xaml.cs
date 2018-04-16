﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.Models;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IForkRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ForkRepositorySelectView : UserControl
    {
        public ForkRepositorySelectView()
        {
            InitializeComponent();
        }

        private void accountsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var account = e.AddedItems.OfType<IAccount>().FirstOrDefault();

            if (account != null)
            {
                ((IForkRepositorySelectViewModel)DataContext).Selected.Execute(account);
            }
        }
    }
}
