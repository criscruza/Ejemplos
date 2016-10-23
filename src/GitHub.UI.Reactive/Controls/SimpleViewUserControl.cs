﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using GitHub.Helpers;
using System.ComponentModel;

namespace GitHub.UI
{
    /// <summary>
    /// Base class for all of our user controls. This one does not import GitHub resource/styles and is used by the 
    /// publish control.
    /// </summary>
    public class SimpleViewUserControl : UserControl, IDisposable, IActivatable
    {
        readonly Subject<object> close = new Subject<object>();
        readonly Subject<object> cancel = new Subject<object>();
        readonly Subject<bool> isBusy = new Subject<bool>();

        public SimpleViewUserControl()
        {
            this.WhenActivated(d =>
            {
                d(this.Events()
                    .KeyUp
                    .Where(x => x.Key == Key.Escape && !x.Handled)
                    .Subscribe(key =>
                    {
                        key.Handled = true;
                        NotifyCancel();
                    }));
            });
        }

        public IObservable<object> Done => close;

        public IObservable<object> Cancel => cancel;

        public IObservable<bool> IsBusy => isBusy;

        protected void NotifyDone()
        {
            if (disposed)
                return;
            
            close.OnNext(null);
            close.OnCompleted();
        }

        protected void NotifyCancel()
        {
            if (disposed)
                return;

            cancel.OnNext(null);
        }

        protected void NotifyIsBusy(bool busy)
        {
            if (disposed)
                return;

            isBusy.OnNext(busy);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                close.Dispose();
                cancel.Dispose();
                isBusy.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class SimpleViewUserControl<TInterface, TImplementor> : SimpleViewUserControl, IViewFor<TInterface>, IView 
        where TInterface : class, IViewModel
        where TImplementor : class
    {
        public SimpleViewUserControl()
        {
            DataContextChanged += (s, e) => ViewModel = (TInterface)e.NewValue;
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TInterface), typeof(TImplementor), new PropertyMetadata(null));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == ViewModelProperty)
            {
                if (e.OldValue is ICanBeBusy)
                {
                    if (e.OldValue is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)e.OldValue).PropertyChanged -= ViewModelBusyChanged;
                    }
                }
                if (e.NewValue is ICanBeBusy)
                {
                    if (e.NewValue is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)e.NewValue).PropertyChanged += ViewModelBusyChanged;
                    }
                }
            }
        }

        void ViewModelBusyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ICanBeBusy.IsBusy))
            {
                if (sender is ICanBeBusy)
                {
                    this.NotifyIsBusy(((ICanBeBusy)sender).IsBusy);
                }
            }
        }

        [AllowNull]
        object IViewFor.ViewModel
        {
            [return:AllowNull]
            get { return ViewModel; }
            set { ViewModel = (TInterface)value; }
        }

        [AllowNull]
        IViewModel IView.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
        }

        [AllowNull]
        public TInterface ViewModel
        {
            [return: AllowNull]
            get { return (TInterface)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        [AllowNull]
        TInterface IViewFor<TInterface>.ViewModel
        {
            [return: AllowNull]
            get { return ViewModel; }
            set { ViewModel = value; }
        }
    }
}
