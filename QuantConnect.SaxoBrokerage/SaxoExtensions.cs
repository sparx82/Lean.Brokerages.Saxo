using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo;

public static class SaxoExtensions
{
    public static IEnumerable<T> ToEnumerable<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        IAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (true)
            {
                ValueTask<bool> moveNext = e.MoveNextAsync();
                if (moveNext.IsCompletedSuccessfully ? moveNext.Result : moveNext.AsTask().GetAwaiter().GetResult())
                {
                    yield return e.Current;
                }
                else break;
            }
        }
        finally
        {
            e.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }
}
