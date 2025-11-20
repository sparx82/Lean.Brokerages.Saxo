using QuantConnect.Brokerages.Saxo.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo;

public static class SaxoExtensions
{
    public static SecurityType ConvertAssetTypeToSecurityType(this SaxoAssetType assetType) => assetType switch
    {
        SaxoAssetType.Stock => SecurityType.Equity,
        SaxoAssetType.StockOption => SecurityType.Option,
        SaxoAssetType.ContractFutures => SecurityType.Future,
        SaxoAssetType.FuturesOption => SecurityType.FutureOption,
        SaxoAssetType.FxForwards => SecurityType.Forex,
        SaxoAssetType.StockIndex => SecurityType.Index,
        SaxoAssetType.StockIndexOption => SecurityType.IndexOption,
        _ => throw new NotSupportedException($"{nameof(SaxoBrokerage)}.{nameof(ConvertAssetTypeToSecurityType)}: " +
            $"The AssetType '{assetType}' is not supported.")
    };


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
