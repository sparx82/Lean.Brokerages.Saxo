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

using System.Linq;
using QuantConnect.Orders;
using QuantConnect.Securities;
using System.Collections.Generic;
using QuantConnect.Logging;

namespace QuantConnect.Brokerages.Saxo.Tests;

public class SaxoBrokerageTest : SaxoBrokerage
{
    /// <summary>
    /// Constructor for the TradeStation brokerage.
    /// </summary>
    /// <remarks>
    /// This constructor initializes a new instance of the TradeStationBrokerage class with the provided parameters.
    /// </remarks>
    public SaxoBrokerageTest(string clientId, string apiUrl, string redirectUrl) : base(clientId, apiUrl, redirectUrl)
    {
    }

    /// <summary>
    /// Retrieves the last price of the specified symbol.
    /// </summary>
    /// <param name="symbol">The symbol for which to retrieve the last price.</param>
    /// <returns>The last price of the specified symbol as a decimal.</returns>
    /*public QuoteActualPrices GetPrice(Symbol symbol)
    {
        var quotes = GetQuote(symbol).Quotes.Single();
        Log.Trace($"{nameof(TradeStationBrokerageTest)}.{nameof(GetPrice)}: {symbol}: Ask = {quotes.Ask}, Bid = {quotes.Bid}, Last = {quotes.Last}");
        return new(quotes.Ask.Value, quotes.Bid.Value, quotes.Last.Value);
    }*/

    /// <summary>
    /// Holds ask, bid, and last prices.
    /// </summary>
    /// <param name="Ask">Ask price.</param>
    /// <param name="Bid">Bid price.</param>
    /// <param name="Last">Last traded price.</param>
    /*public record QuoteActualPrices(decimal Ask, decimal Bid, decimal Last);

    public bool GetTradeStationOrderRouteIdByOrder(TradeStationOrderProperties tradeStationOrderProperties, IReadOnlyCollection<SecurityType> securityTypes, out string routeId)
    {
        routeId = default;
        return GetSaxorderRouteIdByOrderSecurityTypes(tradeStationOrderProperties, securityTypes, out routeId);
    }*/
}