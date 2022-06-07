using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Common.Extensions
{
    public static class ReactiveEx
    {
        public static IObservable<Unit> ToUnit<T>(this IObservable<T> observable)
        {
            ArgumentNullException.ThrowIfNull(observable);

            return observable.Select(_ => Unit.Default);
        }
    }
}
