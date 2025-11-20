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

using Newtonsoft.Json.Linq;
using NodaTime;
using QuantConnect.Benchmarks;
using QuantConnect.Brokerages.LevelOneOrderBook;
using QuantConnect.Brokerages.Saxo.API;
using QuantConnect.Configuration;
using QuantConnect.Data;
using QuantConnect.Data.Fundamental;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Orders.Fees;
using QuantConnect.Packets;
using QuantConnect.Securities;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
//using static QuantConnect.Messages;

namespace QuantConnect.Brokerages.Saxo;

[BrokerageFactory(typeof(SaxoBrokerageFactory))]
public partial class SaxoBrokerage : Brokerage
{
    private SaxoAPIClient _saxoAPIClient;

    private SaxoSymbolMapper _symbolMapper;

    private readonly EventBasedDataQueueHandlerSubscriptionManager _subscriptionManager;

    private bool _isInitialized = false;
    private bool _isConneted = false;

    /// <summary>
    /// Returns true if we're currently connected to the broker
    /// </summary>
    public override bool IsConnected { get => _isConneted; }

    /// <summary>
    /// Represents a type capable of fetching the holdings for the specified symbol
    /// </summary>
    protected ISecurityProvider SecurityProvider { get; private set; }

    /// <summary>
    /// Order provider
    /// </summary>
    protected IOrderProvider OrderProvider { get; private set; }

    /// <summary>
    /// Brokerage helper class to lock message stream while executing an action, for example placing an order
    /// </summary>
    private BrokerageConcurrentMessageHandler<string> _messageHandler;

    /// <summary>
    /// Containing the available trading routes.
    /// </summary>
    /// <remarks>
    /// The routes are only loaded when accessed for the first time, ensuring efficient resource usage.
    /// </remarks>
    private Lazy<Dictionary<SecurityType, ReadOnlyCollection<Route>>> _routes;

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

    public SaxoBrokerage(string clientId, string apiUrl, string redirectUrl) :this (clientId, apiUrl, redirectUrl, null, null)
    {
    }

    public SaxoBrokerage(string clientId, string apiUrl, string redirectUrl, IOrderProvider orderProvider, ISecurityProvider securityProvider) : base("SaxoBrokerage")
    {
        Initialize(clientId, apiUrl, redirectUrl, orderProvider, securityProvider);
    }

    protected void Initialize(string clientId, string restApiUrl, string redirectUrl, IOrderProvider orderProvider, ISecurityProvider securityProvider)
    {
        if (_isInitialized)
        {
            return;
        }
        _saxoAPIClient = new SaxoAPIClient(clientId, restApiUrl, redirectUrl);
        _symbolMapper = new SaxoSymbolMapper(_saxoAPIClient);

        SecurityProvider = securityProvider;
        OrderProvider = orderProvider;

        _isInitialized = true;

        _messageHandler = new(HandleSaxoMessage);

        _aggregator = Composer.Instance.GetPart<IDataAggregator>();
        if (_aggregator == null)
        {
            // toolbox downloader case
            var aggregatorName = Config.Get("data-aggregator", "QuantConnect.Lean.Engine.DataFeeds.AggregationManager");
            Log.Trace($"{nameof(SaxoBrokerage)}.{nameof(Initialize)}: found no data aggregator instance, creating {aggregatorName}");
            _aggregator = Composer.Instance.GetExportedValueByTypeName<IDataAggregator>(aggregatorName);
        }

        _levelOneServiceManager = new LevelOneServiceManager(
            _aggregator,
            (symbols, _) => Subscribe(symbols),
            (symbols, _) => Unsubscribe(symbols));

        _routes = new Lazy<Dictionary<SecurityType, ReadOnlyCollection<Route>>>(() =>
        {
            return _saxoAPIClient.GetRoutes().SynchronouslyAwaitTaskResult().Routes
            .SelectMany(route => route.AssetTypes.Select(assetType => new { SecurityType = assetType.ConvertAssetTypeToSecurityType(), Route = route }))
            .GroupBy(x => x.SecurityType)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Route).ToList().AsReadOnly());
        });
    }

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

        return _symbolMapper.SupportedSecurityType.Contains(symbol.SecurityType);
    }

    internal void HandleSaxoMessage(string json)
    {
        return;

        /*if (OrderProvider == null)
        {
            // we are used as a data source only, not a brokerage
            return;
        }

        try
        {
            var jObj = JObject.Parse(json);
            if (_isSubscribeOnStreamOrderUpdate && jObj["AccountID"] != null)
            {
                if (Log.DebuggingEnabled)
                {
                    Log.Debug($"{nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}.WebSocket.JSON: {json}");
                }

                var brokerageOrder = jObj.ToObject<TradeStationOrder>();

                var globalLeanOrderStatus = default(OrderStatus);
                var eventMessage = string.Empty;
                switch (brokerageOrder.Status)
                {
                    case TradeStationOrderStatusType.Ack:
                        // Remove the order entry when the order is acknowledged (indicating successful submission)
                        _updateSubmittedResponseResultByBrokerageID.TryRemove(new(brokerageOrder.OrderID, true));
                        return;
                    // Sometimes, a filled event is received without the ClosedDateTime property set.
                    // Subsequently, another event is received with the ClosedDateTime property correctly populated.
                    case TradeStationOrderStatusType.Fll:
                    case TradeStationOrderStatusType.Brf:
                        globalLeanOrderStatus = OrderStatus.Filled;
                        break;
                    case TradeStationOrderStatusType.Fpr:
                        globalLeanOrderStatus = OrderStatus.PartiallyFilled;
                        break;
                    case TradeStationOrderStatusType.Rej:
                    case TradeStationOrderStatusType.Tsc:
                    case TradeStationOrderStatusType.Rjr:
                    case TradeStationOrderStatusType.Bro:
                        globalLeanOrderStatus = OrderStatus.Invalid;
                        break;
                    case TradeStationOrderStatusType.Exp:
                        eventMessage = "Expired";
                        globalLeanOrderStatus = OrderStatus.Canceled;
                        break;
                    // Sometimes, a Out event is received without the ClosedDateTime property set.
                    // Subsequently, another event is received with the ClosedDateTime property correctly populated.
                    case TradeStationOrderStatusType.Out when brokerageOrder.ClosedDateTime != default:
                        // Remove the order entry if it was marked as submitted but is now out
                        // Sometimes, the order receives an "Out" status on every even occurrence
                        if (_updateSubmittedResponseResultByBrokerageID.TryRemove(new(brokerageOrder.OrderID, true)))
                        {
                            return;
                        }
                        globalLeanOrderStatus = OrderStatus.Canceled;
                        break;
                    default:
                        Log.Debug($"{nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}.TradeStationStreamStatus: {json}");
                        return;
                }
                ;

                var leanOrders = new List<Order>();
                if (!TryGetOrRemoveCrossZeroOrder(brokerageOrder.OrderID, globalLeanOrderStatus, out var crossZeroLeanOrder))
                {
                    leanOrders = OrderProvider.GetOrdersByBrokerageId(brokerageOrder.OrderID);
                }
                else
                {
                    leanOrders.Add(crossZeroLeanOrder);
                }

                if (leanOrders == null || leanOrders.Count == 0)
                {
                    Log.Error($"{nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}. order id not found: {brokerageOrder.OrderID}");
                    return;
                }

                var sendFeesOnce = default(bool);
                foreach (var leg in brokerageOrder.Legs.DistinctBy(x => x.Symbol))
                {
                    var legOrderStatus = globalLeanOrderStatus;
                    // Manually update the order status to 'Filled' because one of the combo order legs is fully filled.
                    // This prevents excessive event generation in Lean by avoiding repeated 'PartiallyFilled' updates.
                    if (legOrderStatus != OrderStatus.Filled && legOrderStatus == OrderStatus.PartiallyFilled && leg.QuantityRemaining == 0)
                    {
                        legOrderStatus = OrderStatus.Filled;
                    }

                    Order leanOrder;
                    if (leanOrders.Count == 1)
                    {
                        // If there is only one order, use it directly
                        leanOrder = leanOrders[0];
                    }
                    else
                    {
                        // If there are multiple orders, find the one that matches the leg's symbol
                        if (!_symbolMapper.TryGetLeanSymbol(leg.Symbol, leg.AssetType, leg.ExpirationDate, out var leanSymbol))
                        {
                            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, $"{nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}: " +
                                $"Failed to map a Lean Symbol using the following details:: {leg} "));
                            return;
                        }

                        // Ensure there is an order with the specific symbol in leanOrders.
                        leanOrder = leanOrders.FirstOrDefault(order => order.Symbol == leanSymbol);

                        if (leanOrder == null)
                        {
                            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, $"Error in {nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}: " +
                                $"Could not find order with symbol '{leanSymbol}' in leanOrders. " +
                                $"Brokerage Order ID: {brokerageOrder.OrderID}. Leg details - {leg}" +
                                $"Please verify that the order was correctly added to leanOrders."));
                            return;
                        }
                    }

                    // TradeStation may occasionally send duplicate event messages where the only difference is the order of the legs.
                    // If the order status is 'Filled', skip processing this message to avoid handling the same event multiple times.
                    if (leanOrder.Status == OrderStatus.Filled)
                    {
                        continue;
                    }

                    // TradeStation sends the accumulative filled quantity but we need the partial amount for our event
                    _orderIdToFillQuantity.TryGetValue(leanOrder.Id, out var previousExecutionAmount);
                    var accumulativeFilledQuantity = _orderIdToFillQuantity[leanOrder.Id] = leg.BuyOrSell.IsShort() ? decimal.Negate(leg.ExecQuantity) : leg.ExecQuantity;

                    if (globalLeanOrderStatus.IsClosed())
                    {
                        _orderIdToFillQuantity.TryRemove(leanOrder.Id, out _);
                    }

                    var orderEvent = new OrderEvent(
                        leanOrder,
                        DateTime.UtcNow,
                        OrderFee.Zero,
                        brokerageOrder.RejectReason)
                    {
                        Status = legOrderStatus,
                        FillPrice = leg.ExecutionPrice,
                        FillQuantity = accumulativeFilledQuantity - previousExecutionAmount,
                        Message = eventMessage
                    };

                    // When updating a combo order with multiple legs, each leg's update is received separately via WebSocket.
                    // However, it's possible for one leg to be partially filled while another leg is still waiting to be filled.
                    // In these cases, to avoid generating unnecessary events in Lean (and causing spam),
                    // we skip processing if the current leg's update does not include any new fill quantity (i.e., the leg has not had any additional quantity filled).
                    if ((legOrderStatus == OrderStatus.PartiallyFilled || leanOrder.Status == OrderStatus.Filled) && orderEvent.FillQuantity == 0)
                    {
                        continue;
                    }

                    // Fees should only be sent once when the order is fully filled.
                    // The sendFeesOnce flag ensures that we don't send the OrderFee multiple times,
                    // especially for ComboOrders with multiple legs where each leg might trigger an update.
                    if (!sendFeesOnce && globalLeanOrderStatus == OrderStatus.Filled)
                    {
                        sendFeesOnce = true;
                        orderEvent.OrderFee = new OrderFee(new CashAmount(brokerageOrder.CommissionFee, Currencies.USD));
                    }

                    // if we filled the order and have another contingent order waiting, submit it
                    if (!TryHandleRemainingCrossZeroOrder(leanOrder, orderEvent))
                    {
                        OnOrderEvent(orderEvent);
                    }
                }

                // Sometimes, TradeStation returns incorrect responses with a duplicate leg symbol or without the leg being fully executed quantity.
                // This issue occurs only when dealing with OrderType.ComboMarket or OrderType.ComboLimit Orders.
                if (globalLeanOrderStatus == OrderStatus.Filled && leanOrders.Any(x => x.GroupOrderManager != null))
                {
                    leanOrders = OrderProvider.GetOrdersByBrokerageId(brokerageOrder.OrderID);
                    foreach (var leanOrder in leanOrders)
                    {
                        if (leanOrder.Status != OrderStatus.Filled)
                        {
                            // if we don't fill our order from TradeStation's response, we can keep the quantity in collection to calculate the correct holdings.
                            _orderIdToFillQuantity.TryRemove(leanOrder.Id, out var previousExecutionAmount);
                            var orderEvent = new OrderEvent(leanOrder, DateTime.UtcNow, OrderFee.Zero, brokerageOrder.RejectReason)
                            {
                                Status = OrderStatus.Filled,
                                FillQuantity = leanOrder.Quantity - previousExecutionAmount
                            };
                            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Warning, -1, $"Detected missing fill event for OrderID: {leanOrder.Id} creating inferred filled event."));
                            OnOrderEvent(orderEvent);
                        }
                    }
                }
            }
            else if (jObj["StreamStatus"] != null)
            {
                var status = jObj.ToObject<TradeStationStreamStatus>();
                switch (status.StreamStatus)
                {
                    case "EndSnapshot":
                        _isSubscribeOnStreamOrderUpdate = true;
                        _autoResetEvent.Set();
                        break;
                    default:
                        Log.Debug($"{nameof(TradeStationBrokerage)}.{nameof(HandleTradeStationMessage)}.TradeStationStreamStatus: {json}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Raw json: {json}");
            throw;
        }*/
    }
}
