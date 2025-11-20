/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Brokerages.Saxo.Models.Enums;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantConnect.Brokerages.Saxo;

/// <summary>
/// Represents the Saxo Brokerage's HistoryProvider implementation.
/// </summary>
public partial class SaxoBrokerage
{
    /// <summary>
    /// Indicates whether the warning for invalid <see cref="SecurityType"/> has been fired.
    /// </summary>
    private volatile bool _unsupportedSecurityTypeWarningFired;

    /// <summary>
    /// Indicates whether the warning for invalid <see cref="Resolution"/> has been fired.
    /// </summary>
    private volatile bool _unsupportedResolutionTypeWarningFired;

    /// <summary>
    /// Indicates whether the warning for invalid <see cref="TickType"/> has been fired.
    /// </summary>
    private volatile bool _unsupportedTickTypeTypeWarningFired;

    /// <summary>
    /// Gets the history for the requested security
    /// </summary>
    /// <param name="request">The historical data request</param>
    /// <returns>An enumerable of bars covering the span specified in the request</returns>
    public override IEnumerable<BaseData> GetHistory(HistoryRequest request)
    {
        if (!CanSubscribe(request.Symbol))
        {
            if (!_unsupportedSecurityTypeWarningFired)
            {
                _unsupportedSecurityTypeWarningFired = true;
                var error = new StringBuilder($"{nameof(SaxoBrokerage)}.{nameof(GetHistory)}: ");
                if (request.Symbol.IsCanonical())
                {
                    Log.Trace(error.Append($"The symbol '{request.Symbol}' is in canonical form, which is not supported for historical data retrieval.").ToString());
                }
                else
                {
                    Log.Trace(error.Append($"Unsupported SecurityType '{request.Symbol.SecurityType}' for symbol '{request.Symbol}'").ToString());
                }
            }

            return null;
        }

        if(request.Symbol.SecurityType == SecurityType.Option)
        {
            Log.Error($"{nameof(SaxoBrokerage)}.{nameof(GetHistory)}: Saxo does not support historical data of '{request.Symbol.SecurityType}'");
            return null;
        }

        if (request.Resolution < Resolution.Second)
        {
            if (!_unsupportedResolutionTypeWarningFired)
            {
                _unsupportedResolutionTypeWarningFired = true;
                Log.Trace($"{nameof(SaxoBrokerage)}.{nameof(GetHistory)}: Unsupported Resolution '{request.Resolution}'");
            }

            return null;
        }


        //ToDo: Check whether Saxo distinguishes between different TickTypes
        /*if (request.TickType != TickType.Trade)
        {
            if (!_unsupportedTickTypeTypeWarningFired)
            {
                _unsupportedTickTypeTypeWarningFired = true;
                Log.Trace($"{nameof(SaxoBrokerage)}.{nameof(GetHistory)}: Unsupported TickType '{request.TickType}'");
            }

            return null;
        }*/

        return GetSaxoHistory(request);
    }

    private IEnumerable<BaseData> GetSaxoHistory(HistoryRequest request)
    {
        var brokerageSymbol = _symbolMapper.GetBrokerageSymbol(request.Symbol);
        var assetType = SaxoSymbolMapper.ConvertSecurityTypeToSaxoAssetType(request.Symbol.SecurityType);

        var brokerageUnitTime = request.Resolution switch
        {
            Resolution.Minute => SaxoUnitTimeIntervalType.Minute,
            Resolution.Hour => SaxoUnitTimeIntervalType.Hour,
            Resolution.Daily => SaxoUnitTimeIntervalType.Daily,
            _ => throw new NotSupportedException($"{nameof(SaxoBrokerage)}.{nameof(GetHistory)}: Unsupported time Resolution type '{request.Resolution}'")
        };

        var period = request.Resolution.ToTimeSpan();

        foreach (var bar in _saxoAPIClient.GetBars(assetType, brokerageSymbol.ToInt32(), brokerageUnitTime, request.StartTimeUtc, request.EndTimeUtc).ToEnumerable())
        {
            TradeBar tradeBar;

            if (assetType.Any(n => n == SaxoAssetType.FxSpot))
            {
                tradeBar = new TradeBar(bar.Time.ConvertFromUtc(request.ExchangeHours.TimeZone), request.Symbol, bar.OpenBid, bar.HighBid, bar.LowBid, bar.CloseBid, bar.Volume, period);
            }
            else
            {
                tradeBar = new TradeBar(bar.Time.ConvertFromUtc(request.ExchangeHours.TimeZone), request.Symbol, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume, period);
            }

            if (request.ExchangeHours.IsOpen(tradeBar.Time, tradeBar.EndTime, request.IncludeExtendedMarketHours))
            {
                yield return tradeBar;
            }
        }
    }
}
