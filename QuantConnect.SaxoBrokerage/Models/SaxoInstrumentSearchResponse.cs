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

using Newtonsoft.Json;
using QuantConnect.Brokerages.Saxo.Models.Enums;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoInstrumentSearchResponse
{
    /// <summary>
    /// The total count of items in the feed.
    /// </summary>
    public int? Count { get; }

    /// <summary>
    /// The link for the next page of items in the feed.
    /// </summary>
    public string Next { get; }

    /// <summary>
    /// The collection of entities for this feed.
    /// </summary>
    public SummaryInfo[] Data { get; }

    /// <summary>
    /// The maximum number of rows that can be returned (if applicable).
    /// </summary>
    public int? MaxRows { get; }

    [JsonConstructor]
    public SaxoInstrumentSearchResponse(int? count, string next, SummaryInfo[] data, int? maxRows)
    {
        Count = count;
        Next = next;
        Data = data;
        MaxRows = maxRows;
    }
}

/// <summary>
/// Provides summary information about an instrument or contract option root.
/// </summary>
public struct SummaryInfo
{
    /// <summary>
    /// The Asset Type of the instrument (e.g., "FxSpot", "Stock").
    /// </summary>
    public SaxoAssetType AssetType { get; }

    /// <summary>
    /// The currency code of the instrument (e.g., "USD", "EUR").
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// The description of the instrument.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The Id of the exchange where this instrument or the underlying instrument is traded.
    /// </summary>
    public string ExchangeId { get; }

    /// <summary>
    /// Name of the exchange where this instrument or the underlying instrument is traded.
    /// </summary>
    public string ExchangeName { get; }

    /// <summary>
    /// The GroupId value is used to group and structure instruments list. 0 is being used for ungrouped data.
    /// </summary>
    public int GroupId { get; }

    /// <summary>
    /// The GroupOptionRootId value is used to get the option root instruments for a given underlying instrument.
    /// </summary>
    public int GroupOptionRootId { get; }

    /// <summary>
    /// Uniquely identifies the instrument (Uic) or the option root (ContractOptionRootId).
    /// </summary>
    public long Identifier { get; }

    /// <summary>
    /// Indicates that this summary was found as a result of a keyword match.
    /// </summary>
    public bool IsKeywordMatch { get; }

    /// <summary>
    /// The country of the issuer (e.g., for Stocks or Indices).
    /// </summary>
    public string IssuerCountry { get; }

    /// <summary>
    /// Optional reason of why the instrument is not tradable.
    /// </summary>
    public string NonTradableReason { get; }

    /// <summary>
    /// The Uic of the primary listing of this instrument.
    /// </summary>
    public long PrimaryListing { get; }

    /// <summary>
    /// Type of the summary (e.g., "Instrument", "OptionRoot").
    /// </summary>
    public string SummaryType { get; }

    /// <summary>
    /// A combination of letters used to uniquely identify a traded instrument.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// List of asset types this Uic can also be traded as.
    /// </summary>
    public SaxoAssetType[] TradableAs { get; }

    /// <summary>
    /// The asset type of the underlying instrument.
    /// </summary>
    public SaxoAssetType UnderlyingAssetType { get; }

    [JsonConstructor]
    public SummaryInfo(SaxoAssetType assetType, string currencyCode, string description, string exchangeId, string exchangeName, int groupId, int groupOptionRootId, long identifier, bool isKeywordMatch,
        string issuerCountry, string nonTradableReason, long primaryListing, string summaryType, string symbol, SaxoAssetType[] tradableAs, SaxoAssetType underlyingAssetType)
    {
        AssetType = assetType;
        CurrencyCode = currencyCode;
        Description = description;
        ExchangeId = exchangeId;
        ExchangeName = exchangeName;
        GroupId = groupId;
        GroupOptionRootId = groupOptionRootId;
        Identifier = identifier;
        IsKeywordMatch = isKeywordMatch;
        IssuerCountry = issuerCountry;
        NonTradableReason = nonTradableReason;
        PrimaryListing = primaryListing;
        SummaryType = summaryType;
        Symbol = symbol;
        TradableAs = tradableAs;
        UnderlyingAssetType = underlyingAssetType;
    }
}
