using Newtonsoft.Json;
using System;
using QuantConnect.Brokerages.Saxo.Models.Enums;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoContractOptionSpaceSearchResponse
{
    /// <summary>
    /// Represents the immutable response for the GetContractOptionSpaces endpoint.
    /// </summary>
        public int AmountDecimals { get; }
        public SaxoAssetType AssetType { get; }
        public bool CanParticipateInMultiLegOrder { get; }
        public decimal ContractSize { get; }
        public string CurrencyCode { get; }
        public decimal DefaultAmount { get; }
        public string Description { get; }
        public string DisplayHint { get; }
        public ExchangeSummary Exchange { get; }
        public string ExerciseStyle { get; }
        public DateTime ExpiryDate { get; }
        public PriceDisplayFormat Format { get; }
        public int GroupId { get; }
        public decimal IncrementSize { get; }
        public bool IsComplex { get; }
        public bool IsTradable { get; }
        public decimal LotSize { get; }
        public string LotSizeType { get; }
        public decimal MinimumLotSize { get; }
        public decimal MinimumOrderValue { get; }
        public DateTime NoticeDate { get; }
        public int OptionRootId { get; }
        public ContractOptionEntry[] OptionSpace { get; }
        public OrderDistances OrderDistances { get; }
        public string PriceCurrency { get; }
        public decimal PriceToContractFactor { get; }
        public int PrimaryListing { get; }
        public InstrumentKey[] RelatedInstruments { get; }
        public RelatedOptionRoot[] RelatedOptionRootsEnhanced { get; }
        public string SettlementStyle { get; }
        public decimal[] StandardAmounts { get; }
        public string[] SupportedOrderTypes { get; }
        public string[] SupportedStrategies { get; }
        public string Symbol { get; }
        public decimal TickSize { get; }
        public string[] TradableOn { get; }
        public string UnderlyingAssetType { get; }

        [JsonConstructor]
        public SaxoContractOptionSpaceSearchResponse(
            int amountDecimals,
            SaxoAssetType assetType,
            bool canParticipateInMultiLegOrder,
            decimal contractSize,
            string currencyCode,
            decimal defaultAmount,
            string defaultExpiry,
            object defaultOption,
            string description,
            string displayHint,
            ExchangeSummary exchange,
            string exerciseStyle,
            DateTime expiryDate,
            PriceDisplayFormat format,
            int groupId,
            decimal incrementSize,
            bool isComplex,
            bool isTradable,
            decimal lotSize,
            string lotSizeType,
            decimal minimumLotSize,
            decimal minimumOrderValue,
            DateTime noticeDate,
            int optionRootId,
            ContractOptionEntry[] optionSpace,
            OrderDistances orderDistances,
            string priceCurrency,
            decimal priceToContractFactor,
            int primaryListing,
            InstrumentKey[] relatedInstruments,
            int[] relatedOptionRoots,
            RelatedOptionRoot[] relatedOptionRootsEnhanced,
            string settlementStyle,
            decimal[] standardAmounts,
            string[] supportedOrderTypes,
            string[] supportedStrategies,
            string symbol,
            decimal tickSize,
            string[] tradableOn,
            string underlyingAssetType)
        {
            AmountDecimals = amountDecimals;
            AssetType = assetType;
            CanParticipateInMultiLegOrder = canParticipateInMultiLegOrder;
            ContractSize = contractSize;
            CurrencyCode = currencyCode;
            DefaultAmount = defaultAmount;
            Description = description;
            DisplayHint = displayHint;
            Exchange = exchange;
            ExerciseStyle = exerciseStyle;
            ExpiryDate = expiryDate;
            Format = format;
            GroupId = groupId;
            IncrementSize = incrementSize;
            IsComplex = isComplex;
            IsTradable = isTradable;
            LotSize = lotSize;
            LotSizeType = lotSizeType;
            MinimumLotSize = minimumLotSize;
            MinimumOrderValue = minimumOrderValue;
            NoticeDate = noticeDate;
            OptionRootId = optionRootId;
            OptionSpace = optionSpace;
            OrderDistances = orderDistances;
            PriceCurrency = priceCurrency;
            PriceToContractFactor = priceToContractFactor;
            PrimaryListing = primaryListing;
            RelatedInstruments = relatedInstruments;
            RelatedOptionRootsEnhanced = relatedOptionRootsEnhanced;
            SettlementStyle = settlementStyle;
            StandardAmounts = standardAmounts;
            SupportedOrderTypes = supportedOrderTypes;
            SupportedStrategies = supportedStrategies;
            Symbol = symbol;
            TickSize = tickSize;
            TradableOn = tradableOn;
            UnderlyingAssetType = underlyingAssetType;
        }
}

public readonly struct ContractOptionEntry
{
    public int DisplayDaysToExpiry { get; }
    public int DisplayDaysToLastTradeDate { get; }
    public DateTime DisplayExpiry { get; }
    public DateTime Expiry { get; }
    public DateTime LastTradeDate { get; }
    public SpecificOption[] SpecificOptions { get; }
    public TickSizeScheme TickSizeScheme { get; }

[JsonConstructor]
    public ContractOptionEntry(int displayDaysToExpiry, int displayDaysToLastTradeDate, DateTime displayExpiry, DateTime expiry,
        DateTime lastTradeDate, SpecificOption[] specificOptions, TickSizeScheme tickSizeScheme)
    {
        DisplayDaysToExpiry = displayDaysToExpiry;
        DisplayDaysToLastTradeDate = displayDaysToLastTradeDate;
        DisplayExpiry = displayExpiry;
        Expiry = expiry;
        LastTradeDate = lastTradeDate;
        SpecificOptions = specificOptions;
        TickSizeScheme = tickSizeScheme;
    }
}

public readonly struct SpecificOption
{
    public string PutCall { get; }
    public decimal StrikePrice { get; }
    public string TradingStatus { get; }
    public int Uic { get; }
    public int UnderlyingUic { get; }

    [JsonConstructor]
    public SpecificOption(string putCall, decimal strikePrice, string tradingStatus, int uic, int underlyingUic)
    {
        PutCall = putCall;
        StrikePrice = strikePrice;
        TradingStatus = tradingStatus;
        Uic = uic;
        UnderlyingUic = underlyingUic;
    }
}
