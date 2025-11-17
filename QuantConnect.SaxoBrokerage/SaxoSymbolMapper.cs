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

    public readonly HashSet<SecurityType> SupportedSecurityType = new() { SecurityType.Equity, SecurityType.Option, SecurityType.Future, SecurityType.Index, SecurityType.IndexOption };

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
        if (saxoAssetType == SaxoAssetType.Unknown)
        {
            throw new ArgumentException($"SaxoBrokerageSymbolMapper: Unhandled security type: {symbol.SecurityType}");
        }

        // Use the QC ticker (e.g., "AAPL") as the keyword for the Saxo API search
        var keywords = symbol.Value;

        try
        {
            // Synchronously wait for the async task.
            // This is common in QC mappers as the interface is not async.
            SaxoInstrumentSearchResponse searchResponse = _apiClient.SearchInstrumentsAsync(saxoAssetType, keywords).Result;

            var instrument = searchResponse.Data?.FirstOrDefault(d =>
                d.Symbol.Equals(keywords, StringComparison.OrdinalIgnoreCase) &&
                d.AssetType.Equals(saxoAssetType));

            if (instrument == null)
            {
                // Fallback: try to find the first valid instrument if exact match fails
                instrument = searchResponse.Data?.FirstOrDefault(d => d.Identifier > 0);

                if (instrument == null)
                {
                    throw new Exception($"No instrument found for keywords: {keywords} and AssetType: {saxoAssetType}");
                }

                Log.Error($"SaxoBrokerageSymbolMapper.GetBrokerageSymbol(): Exact match not found for {keywords}. Using first result: {instrument?.Description} (Uic: {instrument?.Identifier})");
            }

            brokerageSymbol = instrument?.Identifier.ToString();
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
        if (saxoAssetType == SaxoAssetType.Unknown)
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
    public static SaxoAssetType ConvertSecurityTypeToSaxoAssetType(SecurityType securityType)
    {
        switch (securityType)
        {
            case SecurityType.Equity:
                return SaxoAssetType.Stock;
            case SecurityType.Forex:
                return SaxoAssetType.FxSpot;
            case SecurityType.Cfd:
                return SaxoAssetType.CfdOnStock; // Or CfdOnIndex, CfdOnFutures, etc. Defaulting to Stock.
            case SecurityType.Future:
                return SaxoAssetType.ContractFutures;
            case SecurityType.Option:
                return SaxoAssetType.StockOption; // Or FuturesOption
            case SecurityType.Index:
                return SaxoAssetType.StockIndex;
            case SecurityType.IndexOption:
                return SaxoAssetType.StockIndexOption;
            // Add more mappings as needed
            default:
                Log.Error($"SaxoBrokerageSymbolMapper.ConvertSecurityType: Unhandled SecurityType: {securityType}");
                return SaxoAssetType.Unknown;
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