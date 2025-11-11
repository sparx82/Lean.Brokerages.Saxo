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

using QuantConnect.Brokerages.Saxo.API;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;

namespace QuantConnect.Brokerages.Saxo;

[BrokerageFactory(typeof(SaxoBrokerageFactory))]
public class SaxoBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler, IDataQueueUniverseProvider
{
    private SaxoAPIClient _saxoAPIClient;


    private readonly IDataAggregator _aggregator;
    private readonly EventBasedDataQueueHandlerSubscriptionManager _subscriptionManager;

    private bool _isInitialized = false;
    private bool _isConneted = false;

    /// <summary>
    /// Returns true if we're currently connected to the broker
    /// </summary>
    public override bool IsConnected { get => _isConneted; }

    /// <summary>
    /// Parameterless constructor for brokerage
    /// </summary>
    /// <remarks>This parameterless constructor is required for brokerages implementing <see cref="IDataQueueHandler"/></remarks>
    public SaxoBrokerage()
        : this(Composer.Instance.GetPart<IDataAggregator>())
    {
    }

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="aggregator">consolidate ticks</param>
    public SaxoBrokerage(IDataAggregator aggregator) : base("SaxoBrokerage")
    {
        _aggregator = aggregator;
        _subscriptionManager = new EventBasedDataQueueHandlerSubscriptionManager();
        _subscriptionManager.SubscribeImpl += (s, t) => Subscribe(s);
        _subscriptionManager.UnsubscribeImpl += (s, t) => Unsubscribe(s);
    }

    public SaxoBrokerage(string clientId, string apiUrl, string redirectUrl) : base("SaxoBrokerage")
    {
        Initialize(clientId, apiUrl, redirectUrl);
    }

    protected void Initialize(string clientId, string restApiUrl, string redirectUrl)
    {
        if (_isInitialized)
        {
            return;
        }
        _saxoAPIClient = new SaxoAPIClient(clientId, restApiUrl, redirectUrl);

        _isInitialized = true;
    }

    protected override void OnMessage(object sender, WebSocketMessage e)
    {
        throw new NotImplementedException();
    }

    protected override bool Subscribe(IEnumerable<Symbol> symbols)
    {
        throw new NotImplementedException();
    }

    #region IDataQueueHandler

    /// <summary>
    /// Subscribe to the specified configuration
    /// </summary>
    /// <param name="dataConfig">defines the parameters to subscribe to a data feed</param>
    /// <param name="newDataAvailableHandler">handler to be fired on new data available</param>
    /// <returns>The new enumerator for this subscription request</returns>
    public IEnumerator<BaseData> Subscribe(SubscriptionDataConfig dataConfig, EventHandler newDataAvailableHandler)
    {
        if (!CanSubscribe(dataConfig.Symbol))
        {
            return null;
        }

        var enumerator = _aggregator.Add(dataConfig, newDataAvailableHandler);
        _subscriptionManager.Subscribe(dataConfig);

        return enumerator;
    }

    /// <summary>
    /// Removes the specified configuration
    /// </summary>
    /// <param name="dataConfig">Subscription config to be removed</param>
    public void Unsubscribe(SubscriptionDataConfig dataConfig)
    {
        _subscriptionManager.Unsubscribe(dataConfig);
        _aggregator.Remove(dataConfig);
    }

    /// <summary>
    /// Sets the job we're subscribing for
    /// </summary>
    /// <param name="job">Job we're subscribing for</param>
    public void SetJob(LiveNodePacket job)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Brokerage

    /// <summary>
    /// Gets all open orders on the account.
    /// NOTE: The order objects returned do not have QC order IDs.
    /// </summary>
    /// <returns>The open orders returned from IB</returns>
    public override List<Order> GetOpenOrders()
    {
        var orders = _saxoAPIClient.GetOrders().SynchronouslyAwaitTaskResult();

        var leanOrders = new List<Order>();
        return leanOrders;
        
        /*
        var leanOrders = new List<Order>();

        foreach (var order in orders.Orders.Where(o => o.Status is SaxoOrderStatusType.Ack or SaxoOrderStatusType.Don))
        {
            if (order.Legs.Count == 1)
            {
                var leg = order.Legs.First();

                if (TryCreateLeanOrder(order, leg, out var leanOrder))
                {
                    leanOrders.Add(leanOrder);
                }
            }
            else
            {
                var groupQuantity = GroupOrderExtensions.GetGroupQuantityByEachLegQuantity(
                    order.Legs.Select(leg => leg.QuantityOrdered),
                    decimal.IsNegative(order.LimitPrice) ? OrderDirection.Sell : OrderDirection.Buy
                );
                var groupOrderManager = new GroupOrderManager(order.Legs.Count, groupQuantity);

                var tempLegOrders = new List<Order>();
                foreach (var leg in order.Legs)
                {
                    if (TryCreateLeanOrder(order, leg, out var leanOrder, groupOrderManager))
                    {
                        tempLegOrders.Add(leanOrder);
                    }
                    else
                    {
                        // If any leg fails to create a Lean order, clear tempLegOrders to prevent partial group orders.
                        tempLegOrders.Clear();
                        break;
                    }
                }

                if (tempLegOrders.Count > 0)
                {
                    leanOrders.AddRange(tempLegOrders);
                }
            }
        }
        return leanOrders;*/
    }

    /// <summary>
    /// Gets all holdings for the account
    /// </summary>
    /// <returns>The current holdings from the account</returns>
    public override List<Holding> GetAccountHoldings()
    {
        var holdings = new List<Holding>();
        return holdings;
    }

    /// <summary>
    /// Gets the current cash balance for each currency held in the brokerage account
    /// </summary>
    /// <returns>The current cash balance for each currency available for trading</returns>
    public override List<CashAmount> GetCashBalance()
    {
        var balance = _saxoAPIClient.GetAccountBalance().SynchronouslyAwaitTaskResult();

        var cashBalance = new List<CashAmount>();
        cashBalance.Add(new CashAmount(decimal.Parse(balance.CashBalance, CultureInfo.InvariantCulture), balance.Currency)); // ToDO: Get currency from balance

        return cashBalance;
    }

    /// <summary>
    /// Places a new order and assigns a new broker ID to the order
    /// </summary>
    /// <param name="order">The order to be placed</param>
    /// <returns>True if the request for a new order has been placed, false otherwise</returns>
    public override bool PlaceOrder(Order order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the order with the same id
    /// </summary>
    /// <param name="order">The new order information</param>
    /// <returns>True if the request was made for the order to be updated, false otherwise</returns>
    public override bool UpdateOrder(Order order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cancels the order with the specified ID
    /// </summary>
    /// <param name="order">The order to cancel</param>
    /// <returns>True if the request was made for the order to be canceled, false otherwise</returns>
    public override bool CancelOrder(Order order)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Connects the client to the broker's remote servers
    /// </summary>
    public override void Connect()
    {
        _saxoAPIClient.Connect();
        _isConneted = true;
    }

    /// <summary>
    /// Disconnects the client from the broker's remote servers
    /// </summary>
    public override void Disconnect()
    {
        //throw new NotImplementedException();
        //ToDo: Check if it's necessary to stop all streams
        _isConneted = false;
        return;
    }

    #endregion

    #region IDataQueueUniverseProvider

    /// <summary>
    /// Method returns a collection of Symbols that are available at the data source.
    /// </summary>
    /// <param name="symbol">Symbol to lookup</param>
    /// <param name="includeExpired">Include expired contracts</param>
    /// <param name="securityCurrency">Expected security currency(if any)</param>
    /// <returns>Enumerable of Symbols, that are associated with the provided Symbol</returns>
    public IEnumerable<Symbol> LookupSymbols(Symbol symbol, bool includeExpired, string securityCurrency = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns whether selection can take place or not.
    /// </summary>
    /// <remarks>This is useful to avoid a selection taking place during invalid times, for example IB reset times or when not connected,
    /// because if allowed selection would fail since IB isn't running and would kill the algorithm</remarks>
    /// <returns>True if selection can take place</returns>
    public bool CanPerformSelection()
    {
        throw new NotImplementedException();
    }

    #endregion

    private bool CanSubscribe(Symbol symbol)
    {
        if (symbol.Value.IndexOfInvariant("universe", true) != -1 || symbol.IsCanonical())
        {
            return false;
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes the specified symbols to the subscription
    /// </summary>
    /// <param name="symbols">The symbols to be removed keyed by SecurityType</param>
    private bool Unsubscribe(IEnumerable<Symbol> symbols)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the history for the requested symbols
    /// <see cref="IBrokerage.GetHistory(Data.HistoryRequest)"/>
    /// </summary>
    /// <param name="request">The historical data request</param>
    /// <returns>An enumerable of bars covering the span specified in the request</returns>
    public override IEnumerable<BaseData> GetHistory(Data.HistoryRequest request)
    {
        if (!CanSubscribe(request.Symbol))
        {
            return null; // Should consistently return null instead of an empty enumerable
        }

        throw new NotImplementedException();
    }
}
