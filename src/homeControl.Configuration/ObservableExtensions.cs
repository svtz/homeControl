using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace homeControl.Configuration
{
    public static class ObservableExtensions
    {
        public static Task ForEachAsyncAsync<TSource>(this IObservable<TSource> source, 
            Action<TSource> onNext,
            CancellationToken ct)
        {
            return source.ForEachAsync(s => Task.Run(() => onNext(s), ct), ct);
        }
    }
}