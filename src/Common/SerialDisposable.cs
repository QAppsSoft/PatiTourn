using ReactiveUI;

namespace System.Reactive.Disposables
{
    using System;

    // generic variant of Rx's SerialDisposable class
    // https://kent-boogaart.com/blog/serialdisposablet
    public sealed class SerialDisposable<T> : ReactiveObject, ICancelable, IDisposable
        where T : IDisposable
    {
        private readonly SerialDisposable _disposable;

        public SerialDisposable()
        {
            _disposable = new SerialDisposable();
        }

        public bool IsDisposed => _disposable.IsDisposed;

        public T? Disposable
        {
            get
            {
                return (T?)_disposable.Disposable;
            }
            set
            {
                _disposable.Disposable = value;
                this.RaisePropertyChanged(nameof(Disposable));
            }
        }

        public void Dispose()
            => _disposable.Dispose();
    }
}
