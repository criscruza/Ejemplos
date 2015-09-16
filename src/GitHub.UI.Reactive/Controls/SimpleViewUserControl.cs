﻿using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using ReactiveUI;

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
        readonly Subject<object> error = new Subject<object>();
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

        public IObservable<object> Done { get { return close; } }

        public IObservable<object> Cancel { get { return cancel; } }

        public IObservable<object> Error { get { return error; } }

        public IObservable<bool> IsBusy{ get { return isBusy; } }

        protected void NotifyDone()
        {
            close.OnNext(null);
            close.OnCompleted();
        }

        protected void NotifyCancel()
        {
            cancel.OnNext(null);
            cancel.OnCompleted();
        }

        protected void NotifyError(string message)
        {
            error.OnNext(message);
            error.OnCompleted();
        }

        protected void NotifyIsBusy(bool busy)
        {
            isBusy.OnNext(busy);
        }

        bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;

                close.Dispose();
                error.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
