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
using QuantConnect.Brokerages.Saxo.Models.Enums;
using QuantConnect.Brokerages.Saxo.Models;
using QuantConnect.Securities;
using QuantConnect.Securities.IndexOption;
using QuantConnect.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Linq;

namespace QuantConnect.Brokerages.Saxo;

/// <summary>
/// Provides the mapping between Lean symbols and brokerage specific symbols.
/// </summary>
public class SaxoSymbolMapper : ISymbolMapper
{
    // Thread-safe caches for mapping
    // QC Symbol -> Saxo Uic (as string, since it's used as the brokerage symbol)
    private readonly ConcurrentDictionary<Symbol, string> _symbolToBrokerage = new();
    // Saxo Uic (string) -> QC Symbol
    private readonly ConcurrentDictionary<string, Symbol> _brokerageToSymbol = new();

    private readonly SaxoAPIClient _apiClient;

    public readonly HashSet<SecurityType> SupportedSecurityType = new() { SecurityType.Equity, SecurityType.Option, SecurityType.Future, SecurityType.Index, SecurityType.IndexOption, SecurityType.Forex };

    public SaxoSymbolMapper(SaxoAPIClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Converts a Lean Symbol to a brokerage symbol (Saxo Uic).
    /// </summary>
    /// <param name="symbol">A Lean Symbol</param>
    /// <returns>The brokerage symbol (Saxo Uic as a string)</returns>
    public string GetBrokerageSymbol(Symbol symbol)
    {
        if (_symbolToBrokerage.TryGetValue(symbol, out var brokerageSymbol))
        {
            return brokerageSymbol;
        }

        Log.Trace($"SaxoBrokerageSymbolMapper.GetBrokerageSymbol(): Attempting to map QC Symbol: {symbol}");

        // Map QC SecurityType to Saxo AssetType
        var saxoAssetType = ConvertSecurityTypeToSaxoAssetType(symbol.SecurityType);
        if (saxoAssetType.Any(n => n == SaxoAssetType.Unknown))
        {
            throw new ArgumentException($"SaxoBrokerageSymbolMapper: Unhandled security type: {symbol.SecurityType}");
        }

        string keywords = "";

        try
        {
            SummaryInfo instrument;

            if (saxoAssetType.Any(n => n == (SaxoAssetType.Stock) || n == (SaxoAssetType.Etf)))
            {
                keywords = symbol.Value;

                // Synchronously wait for the async task.
                // This is common in QC mappers as the interface is not async.
                SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;

                instrument = searchResponse.Data.FirstOrDefault(d =>
                    d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                    saxoAssetType.Contains(d.AssetType));

                brokerageSymbol = instrument.Identifier.ToString();
            }
            else if (saxoAssetType.Any(n => n == SaxoAssetType.StockOption))
            {
                keywords = symbol.Underlying.Value;

                //Get the option root id of the underlying instrument first
                SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;

                var underlying = searchResponse.Data.FirstOrDefault(d =>
                    d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                    saxoAssetType.Contains(d.AssetType));

                var contractRootID = underlying.GroupOptionRootId;

                //Now get the option instrument using the underlying UIC
                SaxoContractOptionSpaceSearchResponse optionSearchResponse = _apiClient.SearchOptionRootSpace(contractRootID).Result;

                var options = optionSearchResponse.OptionSpace.First(d =>
                    d.Expiry.Year.Equals(symbol.ID.Date.Year) &&
                    d.Expiry.Month.Equals(symbol.ID.Date.Month) &&
                    d.Expiry.Day.Equals(symbol.ID.Date.Day));

                var option = options.SpecificOptions.First(d =>
                    d.StrikePrice.Equals(symbol.ID.StrikePrice) &&
                    d.PutCall.Equals(symbol.ID.OptionRight == OptionRight.Call ? "Call" : "Put"));

                brokerageSymbol = option.Uic.ToString();
            }
            else if (saxoAssetType.Any(n => n == SaxoAssetType.ContractFutures))
            {
                keywords = symbol.ID.Symbol;

                // Synchronously wait for the async task.
                // This is common in QC mappers as the interface is not async.
                SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;

                instrument = searchResponse.Data.FirstOrDefault(d =>
                    d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                    saxoAssetType.Contains(d.AssetType));

                brokerageSymbol = instrument.Identifier.ToString();
            }
            else if (saxoAssetType.Any(n => n == SaxoAssetType.FxSpot))
            {
                keywords = symbol.Value;

                // Synchronously wait for the async task.
                // This is common in QC mappers as the interface is not async.
                SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;
                instrument = searchResponse.Data.FirstOrDefault(d =>
                    d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                    saxoAssetType.Contains(d.AssetType));

                brokerageSymbol = instrument.Identifier.ToString();
            }
            else
            {
                keywords = symbol.Value;

                // Synchronously wait for the async task.
                // This is common in QC mappers as the interface is not async.
                SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;
                instrument = searchResponse.Data.FirstOrDefault(d =>
                    d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                    saxoAssetType.Contains(d.AssetType));

                brokerageSymbol = instrument.Identifier.ToString();
            }

            /*if (instrument == null)
            {
               throw new Exception($"No instrument found for keywords: {keywords} and AssetType: {saxoAssetType}");
            }*/

            Log.Trace($"SaxoBrokerageSymbolMapper.GetBrokerageSymbol(): Mapped QC Symbol {symbol} to Saxo Uic {brokerageSymbol}");

            // Add to both caches for two-way mapping
            _symbolToBrokerage[symbol] = brokerageSymbol;
            _brokerageToSymbol[brokerageSymbol] = symbol;

            return brokerageSymbol;
        }
        catch (Exception e)
        {
            Log.Error(e, $"SaxoBrokerageSymbolMapper.GetBrokerageSymbol(): Failed to map symbol {symbol}");
            throw;
        }
    }

    public bool TryGetLeanSymbol(string brokerageSymbol, SaxoAssetType saxoAssetType, DateTime expirationDateTime, out Symbol leanSymbol)
    {
        if (_brokerageToSymbol.TryGetValue(brokerageSymbol, out leanSymbol))
        {
            return true;
        }

        try
        {
            var ticker = brokerageSymbol;
            var optionRight = default(OptionRight);
            var strikePrice = default(decimal);
            switch (saxoAssetType)
            {
                //case SaxoAssetType.StockOption:
                //    (ticker, optionRight, strikePrice) = ParsePositionOptionSymbol(brokerageSymbol);
                //    break;
                //case SaxoAssetType.IndexOption:
                //    (ticker, optionRight, strikePrice) = ParsePositionOptionSymbol(brokerageSymbol);
                //    ticker = ConvertIndexBrokerageTickerInLeanTicker(ticker);
                //    break;
                case SaxoAssetType.ContractFutures:
                    ticker = SymbolRepresentation.ParseFutureTicker(brokerageSymbol).Underlying;
                    break;
                //case SaxoAssetType.Index:
                //    ticker = ConvertIndexBrokerageTickerInLeanTicker(ticker);
                //    break;
            }

            leanSymbol = GetLeanSymbol(ticker, saxoAssetType.ConvertAssetTypeToSecurityType(), expirationDate: expirationDateTime, strike: strikePrice, optionRight: optionRight);

            _brokerageToSymbol[brokerageSymbol] = leanSymbol;

            return true;
        }
        catch
        {
            leanSymbol = default;
            return false;
        }
    }

    /// <summary>
    /// Converts a brokerage symbol (Saxo Uic) to a Lean Symbol.
    /// </summary>
    /// <param name="brokerageSymbol">The brokerage symbol (Saxo Uic as a string)</param>
    /// <param name="securityType">The security type</param>
    /// <param name="market">The market</param>
    /// <param name="expirationDate">Expiration date (optional)</param>
    /// <param name="strike">Strike price (optional)</param>
    /// <param name="optionRight">Option right (optional)</param>
    /// <returns>A new Lean Symbol</returns>
    public Symbol GetLeanSymbol(string brokerageSymbol, SecurityType securityType, string market = "", DateTime expirationDate = default, decimal strike = 0, OptionRight optionRight = 0)
    {
        if (_brokerageToSymbol.TryGetValue(brokerageSymbol, out var symbol))
        {
            return symbol;
        }

        Log.Trace($"SaxoBrokerageSymbolMapper.GetLeanSymbol(): Attempting to reverse map Saxo Uic: {brokerageSymbol}");

        if (!long.TryParse(brokerageSymbol, out var uic))
        {
            throw new ArgumentException($"SaxoBrokerageSymbolMapper: Invalid brokerage symbol. Expected a long Uic, but got: {brokerageSymbol}");
        }

        var saxoAssetType = ConvertSecurityTypeToSaxoAssetType(securityType);
        if (saxoAssetType.Any(n => n == SaxoAssetType.Unknown))
        {
            throw new ArgumentException($"SaxoBrokerageSymbolMapper: Unhandled security type: {securityType}");
        }

        try
        {
            // Synchronously wait for the async task.
            var details = _apiClient.GetInstrumentsDetailsAsync(uic.ToString(), saxoAssetType).Result;

            if (string.IsNullOrEmpty(details.Symbol))
            {
                throw new Exception($"Could not retrieve details for Uic: {uic} and AssetType: {saxoAssetType}");
            }

            // Map Saxo ExchangeId to QC Market
            var qcMarket = ConvertSaxoExchangeIdToMarket(details.Exchange.ExchangeId);

            // Use ticker from Saxo (details.Symbol) as the Lean Symbol value
            var ticker = details.Symbol;

            // Create the new Lean Symbol
            symbol = Symbol.Create(ticker, securityType, qcMarket);

            Log.Trace($"SaxoBrokerageSymbolMapper.GetLeanSymbol(): Mapped Saxo Uic {brokerageSymbol} to QC Symbol {symbol}");

            // Add to both caches for two-way mapping
            _brokerageToSymbol[brokerageSymbol] = symbol;
            _symbolToBrokerage[symbol] = brokerageSymbol;

            return symbol;
        }
        catch (Exception e)
        {
            Log.Error(e, $"SaxoBrokerageSymbolMapper.GetLeanSymbol(): Failed to map brokerage symbol {brokerageSymbol}");
            throw;
        }
    }

    #region QC/Saxo Mapping Helpers

    /// <summary>
    /// Converts a QuantConnect SecurityType to a Saxo AssetType string.
    /// </summary>
    public static SaxoAssetType[] ConvertSecurityTypeToSaxoAssetType(SecurityType securityType)
    {
        switch (securityType)
        {
            case SecurityType.Equity:
                return [SaxoAssetType.Stock, SaxoAssetType.Etf];
            case SecurityType.Forex:
                return [SaxoAssetType.FxSpot];
            case SecurityType.Cfd:
                return [SaxoAssetType.CfdOnStock]; // Or CfdOnIndex, CfdOnFutures, etc. Defaulting to Stock.
            case SecurityType.Future:
                return [SaxoAssetType.ContractFutures];
            case SecurityType.Option:
                return [SaxoAssetType.StockOption]; // Or FuturesOption
            case SecurityType.Index:
                return [SaxoAssetType.StockIndex];
            case SecurityType.IndexOption:
                return [SaxoAssetType.StockIndexOption];
            // Add more mappings as needed
            default:
                Log.Error($"SaxoBrokerageSymbolMapper.ConvertSecurityType: Unhandled SecurityType: {securityType}");
                return [SaxoAssetType.Unknown];
        }
    }

    /// <summary>
    /// Converts a Saxo ExchangeId to a QuantConnect Market string.
    /// </summary>
    public static string ConvertSaxoExchangeIdToMarket(string saxoExchangeId)
    {
        if (string.IsNullOrEmpty(saxoExchangeId))
        {
            //return Market.e
            return Market.USA; // Default to USA if unknown
        }

        // Saxo provides many specific exchange IDs (e.g., "NASDAQ", "NYSE", "LSE")
        // QuantConnect typically groups them (e.g., Market.USA, Market.UK)
        // This mapping needs to be expanded based on the exchanges you trade.
        switch (saxoExchangeId.ToUpperInvariant())
        {
            case "NASDAQ":
            case "NYSE":
            case "BATS":
            case "ARCA":
                return Market.USA;
            case "EUREX":
                return Market.EUREX;
            // Add more mappings
            default:
                // Default to the Saxo ExchangeId if no specific mapping exists
                return saxoExchangeId.ToUpperInvariant();
        }
    }

    #endregion
}