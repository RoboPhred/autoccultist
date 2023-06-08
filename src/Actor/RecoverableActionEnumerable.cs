namespace AutoccultistNS.Actor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class RecoverableActionEnumerable : IEnumerable<IAutoccultistAction>
    {
        private readonly IEnumerable<IAutoccultistAction> enumerable;
        private readonly Func<Exception, ActionErrorSource, IEnumerable<IAutoccultistAction>> onError;

        public RecoverableActionEnumerable(IEnumerable<IAutoccultistAction> enumerable, Func<Exception, ActionErrorSource, IEnumerable<IAutoccultistAction>> onError)
        {
            this.enumerable = enumerable;
            this.onError = onError;
        }

        /// <summary>
        /// The source of the error.
        /// </summary>
        public enum ActionErrorSource
        {
            /// <summary>
            /// The error was thrown by the action itself.
            /// </summary>
            Action,

            /// <summary>
            /// The error was thrown by the source that supplied the action.
            /// </summary>
            Coroutine,
        }

        public IEnumerator<IAutoccultistAction> GetEnumerator()
        {
            return new TryCatchEnumerator(this.enumerable, this.onError);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class TryCatchAction : IAutoccultistAction
        {
            private readonly Action<Exception> onError;
            private readonly IAutoccultistAction action;

            public TryCatchAction(IAutoccultistAction action, Action<Exception> onError)
            {
                this.action = action;
                this.onError = onError;
            }

            public void Execute()
            {
                try
                {
                    this.action.Execute();
                }
                catch (Exception ex)
                {
                    this.onError(ex);
                }
            }

            public override string ToString()
            {
                return this.action.ToString();
            }
        }

        /// <summary>
        /// An enumerator that catches errors both from actions and from the source that supplied them.
        /// </summary>
        private class TryCatchEnumerator : IEnumerator<IAutoccultistAction>
        {
            private readonly Func<Exception, ActionErrorSource, IEnumerable<IAutoccultistAction>> onError;

            private bool inErrorHandler = false;

            private IEnumerator<IAutoccultistAction> currentEnumerator;

            private IAutoccultistAction current;

            public TryCatchEnumerator(IEnumerable<IAutoccultistAction> enumerable, Func<Exception, ActionErrorSource, IEnumerable<IAutoccultistAction>> onError)
            {
                this.onError = onError;
                this.currentEnumerator = enumerable.GetEnumerator();
            }

            public IAutoccultistAction Current
            {
                get
                {
                    this.EnsureNotDisposed();
                    return this.current;
                }
            }

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.EnsureNotDisposed();
                this.currentEnumerator.Dispose();
                this.currentEnumerator = null;
            }

            public bool MoveNext()
            {
                this.EnsureNotDisposed();

                try
                {
                    if (!this.currentEnumerator.MoveNext())
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    if (this.inErrorHandler)
                    {
                        // We're already in an error handler, so we can't handle this error.
                        throw;
                    }

                    this.HandleError(ex, ActionErrorSource.Coroutine);

                    // MoveNext is in the process of being called, meaning the caller is done with the current value of Current and wants a new one.
                    // We need to call MoveNext for the error handler's enumerable so that this.current is updated to the error handler's action.
                    return this.MoveNext();
                }

                this.current = new TryCatchAction(this.currentEnumerator.Current, ex => this.HandleError(ex, ActionErrorSource.Action));
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void HandleError(Exception ex, ActionErrorSource source)
            {
                if (this.inErrorHandler)
                {
                    throw ex;
                }

                this.currentEnumerator.Dispose();

                this.inErrorHandler = true;
                var onError = this.onError(ex, source);
                this.currentEnumerator = onError.GetEnumerator();
            }

            private void EnsureNotDisposed()
            {
                if (this.currentEnumerator == null)
                {
                    throw new ObjectDisposedException(nameof(TryCatchEnumerator));
                }
            }
        }
    }
}
