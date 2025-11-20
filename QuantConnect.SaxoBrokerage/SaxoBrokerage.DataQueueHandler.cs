using System;
using System.Linq;
using System.Threading;
using QuantConnect.Data;
using QuantConnect.Util;
using QuantConnect.Logging;
using QuantConnect.Packets;
using System.Threading.Tasks;
using QuantConnect.Interfaces;
using System.Collections.Generic;
using QuantConnect.Configuration;
using System.Collections.Concurrent;

using QuantConnect.Brokerages.LevelOneOrderBook;
using QuantConnect.Brokerages.Saxo.Models;
using QuantConnect.Brokerages.Saxo.Streaming;

namespace QuantConnect.Brokerages.Saxo;

public partial class SaxoBrokerage : IDataQueueHandler
{
    /// <summary>
    /// Manages Level 1 market data subscriptions and routing of updates to the shared <see cref="IDataAggregator"/>.
    /// Responsible for tracking and updating individual <see cref="LevelOneMarketData"/> instances per symbol.
    /// </summary>
    private LevelOneServiceManager _levelOneServiceManager;

    /// <summary>
    /// Manages the list of active quote stream managers.
    /// </summary>
    private readonly List<StreamingTaskManager> _quoteStreamManagers = [];

    /// <summary>
    /// Indicates whether delayed streaming data is enabled for the application.
    /// </summary>
    private readonly bool _enableDelayedStreamingData = Config.GetBool("trade-station-enable-delayed-streaming-data");

    /// <summary>
    /// A thread-safe dictionary used to track whether delay checks have been performed for specific symbols.
    /// The key represents the symbol, and the value indicates whether the delay has been verified.
    /// </summary>
    private readonly ConcurrentDictionary<Symbol, bool> _symbolsDelayChecked = [];

    /// <summary>
    /// Aggregates ticks and bars based on given subscriptions.
    /// </summary>
    protected IDataAggregator _aggregator;

    /// <summary>
    /// Sets the job we're subscribing for
    /// </summary>
    /// <param name="job">Job we're subscribing for</param>
    public void SetJob(LiveNodePacket job)
    {
        Initialize(
            clientId: job.BrokerageData["saxo-client-id"],
            restApiUrl: job.BrokerageData["saxo-api-url"],
            redirectUrl: job.BrokerageData.TryGetValue("saxo-redirect-url", out var redirectUrl) ? redirectUrl : string.Empty,
            orderProvider: null,
            securityProvider: null
        );

        if (!IsConnected)
        {
            Connect();
        }
    }

    /// <summary>
    /// Removes the specified configuration
    /// </summary>
    /// <param name="dataConfig">Subscription config to be removed</param>
    public void Unsubscribe(SubscriptionDataConfig dataConfig)
    {
        _levelOneServiceManager.Unsubscribe(dataConfig);
        _aggregator.Remove(dataConfig);
    }


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
        _levelOneServiceManager.Subscribe(dataConfig);

        return enumerator;
    }

    /// <summary>
    /// Subscribes to updates for the specified collection of symbols.
    /// </summary>
    /// <param name="symbols">A collection of symbols to subscribe to.</param>
    /// <returns>Always, Returns <c>true</c> if the subscription was successful</returns>
    private bool Subscribe(IEnumerable<Symbol> symbols)
    {
        var subscribedBrokerageSymbolsQueue = new Queue<string>();
        foreach (var brokerageSymbol in symbols.Select(_symbolMapper.GetBrokerageSymbol))
        {
            subscribedBrokerageSymbolsQueue.Enqueue(brokerageSymbol);
        }

        foreach (var quoteStream in _quoteStreamManagers)
        {
            // Skip this quote stream as its subscription is full
            if (!quoteStream.IsSubscriptionFilled)
            {
                ProcessSubscriptions(quoteStream, subscribedBrokerageSymbolsQueue);
            }
        }

        while (subscribedBrokerageSymbolsQueue.Count > 0)
        {
            var streamQuoteTask = new StreamingTaskManager(StreamHandleQuoteEvents);
            _quoteStreamManagers.Add(streamQuoteTask);
            ProcessSubscriptions(streamQuoteTask, subscribedBrokerageSymbolsQueue);
        }

        return true;
    }

    /// <summary>
    /// Unsubscribes from updates for the specified collection of symbols.
    /// </summary>
    /// <param name="symbols">A collection of symbols to unsubscribe from.</param>
    /// <returns>Always, Returns <c>true</c> if the unSubscription was successful</returns>
    private bool Unsubscribe(IEnumerable<Symbol> symbols)
    {
        var streamsToRemove = new List<StreamingTaskManager>();

        foreach (var brokerageSymbol in symbols.Select(_symbolMapper.GetBrokerageSymbol))
        {
            foreach (var streamQuoteTask in _quoteStreamManagers.Where(x => x.RemoveSubscriptionItem(brokerageSymbol)))
            {
                if (streamQuoteTask.IsSubscriptionBrokerageTickerEmpty)
                {
                    streamsToRemove.Add(streamQuoteTask);
                }
            }
        }

        // Remove the streams that have no remaining subscriptions
        foreach (var streamToRemove in streamsToRemove)
        {
            streamToRemove.DisposeSafely();
            _quoteStreamManagers.Remove(streamToRemove);
            Log.Debug($"{nameof(SaxoBrokerage)}.{nameof(Unsubscribe)}: Stream removed. Remaining active streams: {_quoteStreamManagers.Count}");
        }

        return true;
    }

    /// <summary>
    /// Handles streaming quote events for the specified brokerage tickers.
    /// </summary>
    /// <param name="brokerageTickers">A read-only collection of brokerage tickers to subscribe to for streaming quotes.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the streaming operation.</param>
    /// <returns>A task that represents the asynchronous operation, returning <c>false</c> upon completion.</returns>
    private async Task<bool> StreamHandleQuoteEvents(IReadOnlyCollection<string> brokerageTickers, CancellationToken cancellationToken)
    {
        await foreach (var quote in _saxoAPIClient.StreamQuotes(brokerageTickers, cancellationToken))
        {
            HandleQuoteEvents(quote);
        }
        return false;
    }

    /// <summary>
    /// Handles incoming quote events and updates the order books accordingly.
    /// </summary>
    /// <param name="quote">The incoming quote containing bid, ask, and trade information.</param>
    private void HandleQuoteEvents(Quote quote)
    {
        if (!_symbolMapper.TryGetLeanSymbol(quote.Symbol, default, default, out var leanSymbol))
        {
            Log.Error($"{nameof(SaxoBrokerage)}.{nameof(HandleQuoteEvents)}: Failed to map symbol '{quote.Symbol}' from received quote: {quote}.");
            return;
        }

        if (!_enableDelayedStreamingData && quote.MarketFlags.IsDelayed != null && quote.MarketFlags.IsDelayed.Value && _symbolsDelayChecked.TryAdd(leanSymbol, true))
        {

            OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, "DelayStreamingData",
                $"Detected delay streaming data for {leanSymbol}. Expected delayed streaming data to be '{_enableDelayedStreamingData}', but received '{quote.MarketFlags.IsDelayed}'."));
        }

        var utcNow = DateTime.UtcNow;
        _levelOneServiceManager.HandleQuote(leanSymbol, utcNow, quote.Bid, quote.BidSize, quote.Ask, quote.AskSize);
        _levelOneServiceManager.HandleLastTrade(leanSymbol, quote.TradeTime, quote.LastSize, quote.Last);
        _levelOneServiceManager.HandleOpenInterest(leanSymbol, utcNow, quote.DailyOpenInterest);
    }

    /// <summary>
    /// Processes subscription items from the queue and adds them to the quote stream manager.
    /// </summary>
    /// <param name="quoteStream">The quote stream manager responsible for handling the subscription items.</param>
    /// <param name="symbolsQueue">
    /// A queue of symbols representing the brokerage symbols to be subscribed. 
    /// Items that cannot be added to the subscription are re-enqueued, and the process stops when the subscription limit is reached.
    /// </param>
    private static void ProcessSubscriptions(StreamingTaskManager quoteStream, Queue<string> symbolsQueue)
    {
        while (symbolsQueue.Count > 0)
        {
            var brokerageSymbol = symbolsQueue.Dequeue();

            if (!quoteStream.AddSubscriptionItem(brokerageSymbol))
            {
                // Re-enqueue the symbol since adding it to the subscription failed
                symbolsQueue.Enqueue(brokerageSymbol);
                // The subscription limit is reached and no more items can be added.
                break;
            }
        }
    }
}
