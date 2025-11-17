using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static NodaTime.TimeZones.TzdbZone1970Location;
using QuantConnect.Brokerages.Saxo.Models.Enums;
using MathNet.Numerics.Interpolation;

namespace QuantConnect.Brokerages.Saxo.Models;

// --- Main Instrument Struct ---

/// <summary>
/// Represents the detailed parameters for a financial instrument,
/// designed for JSON deserialization.
/// </summary>
public struct SaxoDetailedInstrumentInformation
{
    public bool AffiliateInfoRequired { get; }
    /// <summary>
    /// Additional Minimum Trade Size, only available for Mutual Funds
    /// </summary>
    public double AdditionalMinimumTradeSize { get; }

    /// <summary>
    /// Additional Minimum Trade Units, only available for Mutual Funds
    /// </summary>
    public double AdditionalMinimumTradeUnits { get; }

    /// <summary>
    /// The trading amount supported decimals.
    /// </summary>
    public int AmountDecimals { get; }

    /// <summary>
    /// AssetType (Note: OptionRoots also have an asset type (FuturesOption, StockOption, StockIndexOption).
    /// </summary>
    public SaxoAssetType AssetType { get; }

    /// <summary>
    /// Index ratio of bonds.
    /// </summary>
    public double BondIndexRatio { get; }

    /// <summary>
    /// The type of bond.
    /// </summary>
    public string BondType { get; }

    /// <summary>
    /// For FxVanillaOptions and ContractOption roots (true if it supports exchange traded option strategies).
    /// </summary>
    public bool CanParticipateInMultiLegOrder { get; }

    /// <summary>
    /// No of units of the underlying instrument covered by one contract.
    /// </summary>
    public double ContractSize { get; }

    /// <summary>
    /// Coupon (relevant for bonds only)
    /// </summary>
    public double Coupon { get; }

    /// <summary>
    /// Type of a coupon (relevant for bonds only)
    /// </summary>
    public string CouponType { get; }

    /// <summary>
    /// The ISO currency code of the instrument/symbol.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Cut off time for Mutual fund subscriptions, only available for Mutual Funds
    /// </summary>
    public string CutOffTimeForSubscriptions { get; }

    /// <summary>
    /// Default amount suggested for trading this Instrument
    /// </summary>
    public double DefaultAmount { get; }

    /// <summary>
    /// The default slippage (only set for FX DMA instruments)
    /// </summary>
    public double DefaultSlippage { get; }

    /// <summary>
    /// The default slippage type - either pips or percentage (only set for FX DMA instruments)
    /// </summary>
    public string DefaultSlippageType { get; }

    /// <summary>
    /// Deflation floor protection, applicable only for Inflation Linked Bonds.
    /// </summary>
    public LocalizedInfo? DeflationFloorProtectionType { get; }

    /// <summary>
    /// Description of Instrument (DAX Index - Nov 2013), in English.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Hint to the client application about how it should display the instrument.
    /// </summary>
    public string DisplayHint { get; }

    /// <summary>
    /// Distribution Policy, only available for Mutual Funds
    /// </summary>
    public string DistributionPolicy { get; }

    /// <summary>
    /// Dividend Periodicity, only available for Mutual Funds
    /// </summary>
    public string DividendPeriodicity { get; }

    /// <summary>
    /// Information about the exchange where this instrument or the underlying instrument is traded.
    /// </summary>
    public ExchangeSummary Exchange { get; }

    /// <summary>
    /// The latest time at which an option can be exercised within a given day. This cutoff is specified in Exchange local time.
    /// </summary>
    public string ExerciseCutOffTime { get; }

    /// <summary>
    /// Applicable for Types: Futures, CfdOnFutures, ContractOptions
    /// </summary>
    public DateTime ExpiryDate { get; }

    /// <summary>
    /// Applicable for Types: CfdOnFutures
    /// </summary>
    public DateTime ExpiryDateTime { get; }

    /// <summary>
    /// Price formatting information
    /// </summary>
    public PriceDisplayFormat Format { get; }

    public float FractionalMinimumLotSize { get; }

    /// <summary>
    /// The latest possible forward date for a forex instrument that can be traded as forward.
    /// </summary>
    public DateTime FxForwardMaxForwardDate { get; }

    /// <summary>
    /// The earliest possible forward date for a forex instrument that can be traded as forward.
    /// </summary>
    public DateTime FxForwardMinForwardDate { get; }

    /// <summary>
    /// Spot date for a forex instrument.
    /// </summary>
    public DateTime FxSpotDate { get; }

    /// <summary>
    /// The GroupId value is used to group and structure instruments list. 0 is being used for ungrouped data.
    /// </summary>
    public int GroupId { get; }

    /// <summary>
    /// The rate at which price should be incremented when done in steps.
    /// </summary>
    public double IncrementSize { get; }

    /// <summary>
    /// InitialPublicOffering details
    /// </summary>
    public IpoDetails IpoDetails { get; }

    /// <summary>
    /// Is the instrument (complex)?
    /// </summary>
    public bool IsComplex { get; }

    public bool IsBailIn { get; }

    public bool IsBarrierEqualsStrike { get; }

    public bool IsOcoOrderSupported { get; }

    public bool IsSwitchBySameCurrency { get; }

    public bool IsSystematicInternaliser { get; }

    /// <summary>
    /// Indicates if the instrument has direct market access (only FxSpot)
    /// </summary>
    public bool IsDmaEnabled { get; }

    /// <summary>
    /// Indicate whether Extended Trading is enabled for instrument True if enabled, false otherwise.
    /// </summary>
    public bool IsExtendedTradingHoursEnabled { get; }

    /// <summary>
    /// Indicates if the instrument is PEA Eligible
    /// </summary>
    public bool IsPEAEligible { get; }

    /// <summary>
    /// Indicates if the instrument is PEA-PME Eligible
    /// </summary>
    public bool IsPEASMEEligible { get; }

    /// <summary>
    /// Applicable for Types: Futures
    /// </summary>
    public bool IsPitTraded { get; }

    /// <summary>
    /// Redemption by amounts is enabled or not, only available for Mutual Funds
    /// </summary>
    public bool IsRedemptionByAmounts { get; }

    /// <summary>
    /// Is the instrument (currently) tradable by the user represented in the API token?
    /// </summary>
    public bool IsTradable { get; }

    /// <summary>
    /// Strategy Legs (for Futures Strategies).
    /// </summary>
    public StrategyLeg[] Legs { get; }

    /// <summary>
    /// The LotSize, how many contracts in a lot traded?.
    /// </summary>
    public double LotSize { get; }

    /// <summary>
    /// Lot size description for the instrument. Futures only.
    /// </summary>
    public string LotSizeText { get; }

    /// <summary>
    /// Applicable for Types: Shares, CfdOnFutures, ContractOptions
    /// </summary>
    public string LotSizeType { get; }

    /// <summary>
    /// The maximum size of a (guaranteed) stop order for this instrument.
    /// </summary>
    public double MaxGuaranteedStopOrderSize { get; }

    /// <summary>
    /// Since this is only pertinent to CFDs and stocks...
    /// </summary>
    public double MinimumLotSize { get; }

    /// <summary>
    /// Value of trade (Amount * Price) must be greater than or equal to
    /// </summary>
    public double MinimumOrderValue { get; }

    /// <summary>
    /// The minimum trade size of a given contract.
    /// </summary>
    public double MinimumTradeSize { get; }

    /// <summary>
    /// The minimum trade size currency of MutualFund
    /// </summary>
    public string MinimumTradeSizeCurrency { get; }

    /// <summary>
    /// Optional reason of why the is not tradable.
    /// </summary>
    public string NonTradableReason { get; }

    /// <summary>
    /// Applicable for Types: Futures
    /// </summary>
    public DateTime NoticeDate { get; }

    /// <summary>
    /// Options only. Is an options chain subscription allowed for this instrument.
    /// </summary>
    public bool OptionsChainSubscriptionAllowed { get; }

    /// <summary>
    /// Minimal order distances for each order type.
    /// </summary>
    public OrderDistances OrderDistances { get; }

    /// <summary>
    /// Order setting of an instrument
    /// </summary>
    public OrderSetting OrderSetting { get; }

    /// <summary>
    /// Price currency of the instrument.
    /// </summary>
    public string PriceCurrency { get; }

    /// <summary>
    /// Applicable for Types: Futures, CfdOnFutures, ContractOptions, CfdIndices
    /// </summary>
    public double PriceToContractFactor { get; }

    /// <summary>
    /// The uic of the primary listing of this instrument.
    /// </summary>
    public int PrimaryListing { get; }

    /// <summary>
    /// Contract options only. Indicates whether the options is a put option or a call option.
    /// </summary>
    public string PutCall { get; }

    /// <summary>
    /// Redemption Days, only available for Mutual Funds
    /// </summary>
    public int RedemptionDays { get; }

    /// <summary>
    /// Redemption Days Type, only available for Mutual Funds
    /// </summary>
    public string RedemptionDaysType { get; }

    /// <summary>
    /// List of related UICs and asset types.
    /// </summary>
    public InstrumentKey[] RelatedInstruments { get; }

    /// <summary>
    /// . Please use RelatedOptionRootsEnhanced instead.
    /// </summary>
    public int[] RelatedOptionRoots { get; }

    /// <summary>
    /// List of related Option root id's combined with asset type.
    /// </summary>
    public RelatedOptionRoot[] RelatedOptionRootsEnhanced { get; }

    /// <summary>
    /// Contract options and structured products only. The settlement style of the instrument.
    /// </summary>
    public string SettlementStyle { get; }

    /// <summary>
    /// Indicates whether short trade is disabled. True if disabled, false otherwise.
    /// </summary>
    public bool ShortTradeDisabled { get; }

    /// <summary>
    /// Standard amounts. Used for drop downs in trade tickets.
    /// </summary>
    public double[] StandardAmounts { get; }

    /// <summary>
    /// Strategy Type. For calendar spreads.
    /// </summary>
    public string StrategyType { get; }

    /// <summary>
    /// The strike price. Used on contract options.
    /// </summary>
    public double StrikePrice { get; }

    /// <summary>
    /// SubscriptionByShares, only available for Mutual Funds
    /// </summary>
    public double SubscriptionByAmounts { get; }

    /// <summary>
    /// SubscriptionByShares, only available for Mutual Funds
    /// </summary>
    public double SubscriptionByShares { get; }

    /// <summary>
    /// Subscription Days, only available for Mutual Funds
    /// </summary>
    public int SubscriptionDays { get; }

    /// <summary>
    /// Subscription Days Type, only available for Mutual Funds
    /// </summary>
    public string SubscriptionDaysType { get; }

    /// <summary>
    /// What types of orders can be placed on this instrument ( or option)
    /// </summary>
    public string[] SupportedOrderTypes { get; }

    /// <summary>
    /// Field Group - Order type settings for each order type supported by current instrument.
    /// </summary>
    public SupportedOrderTypeSetting[] SupportedOrderTypeSettings { get; }

    /// <summary>
    /// Algo order strategies which are allowed on this instrument.
    /// </summary>
    public string[] SupportedStrategies { get; }

    /// <summary>
    /// Symbol- A combination of letters used to uniquely identify a traded instrument.
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// Minimum price movement of an instrument.
    /// </summary>
    public double TickSize { get; }

    /// <summary>
    /// Minimum price increment when placing a limit order.
    /// </summary>
    public double TickSizeLimitOrder { get; }

    /// <summary>
    /// The tick size scheme of the instrument.
    /// </summary>
    public TickSizeScheme TickSizeScheme { get; }

    /// <summary>
    /// Minimum price increment when placing a stop order.
    /// </summary>
    public double TickSizeStopOrder { get; }

    /// <summary>
    /// For instruments: How can an instrument with "this" uic also be traded.
    /// </summary>
    public string[] TradableAs { get; }

    /// <summary>
    /// Specifies what accounts the calling client can trade this instrument or option space on.
    /// </summary>
    public string[] TradableOn { get; }

    /// <summary>
    /// Trading sessions of an instrument
    /// </summary>
    public InstrumentTradeSessions TradingSessions { get; }

    /// <summary>
    /// Indicates whether the instrument is currently tradable by the user represented in the API token.
    /// </summary>
    public string TradingStatus { get; }

    /// <summary>
    /// Applicable for Types: CfdOnFutures
    /// </summary>
    public string TradingUnitsPlural { get; }

    /// <summary>
    /// Applicable for Types: CfdOnFutures
    /// </summary>
    public string TradingUnitsSingular { get; }

    /// <summary>
    /// Universal Instrument Code.
    /// </summary>
    public int Uic { get; }

    /// <summary>
    /// The asset type of the underlying instrument.
    /// </summary>
    public string UnderlyingAssetType { get; }

    /// <summary>
    /// Whether the instrument has more than one underlying instruments (in that case it is a Basket).
    /// </summary>
    public string UnderlyingTypeCategory { get; }

    public string[] SupportedOrderTriggerPriceTypes { get; }

    public string TradingSignals { get; }

    /// <summary>
    /// Constructor for JSON deserialization.
    /// </summary>
    [JsonConstructor]
    public SaxoDetailedInstrumentInformation(bool affiliateInfoRequired, double additionalMinimumTradeSize, double additionalMinimumTradeUnits, int amountDecimals, SaxoAssetType assetType, double bondIndexRatio, string bondType, bool canParticipateInMultiLegOrder,
        double contractSize, double coupon, string couponType, string currencyCode, string cutOffTimeForSubscriptions, double defaultAmount, double defaultSlippage, string defaultSlippageType, LocalizedInfo deflationFloorProtectionType,
        string description, string displayHint, string distributionPolicy, string dividendPeriodicity, ExchangeSummary exchange, string exerciseCutOffTime, DateTime expiryDate, DateTime expiryDateTime, PriceDisplayFormat format,
        float fractionalMinimumLotSize, DateTime fxForwardMaxForwardDate, DateTime fxForwardMinForwardDate, DateTime fxSpotDate, int groupId, double incrementSize, IpoDetails ipoDetails, bool isComplex, bool isDmaEnabled, bool isExtendedTradingHoursEnabled,
        bool isPEAEligible, bool isPEASMEEligible, bool isPitTraded, bool isRedemptionByAmounts, bool isTradable, StrategyLeg[] legs, double lotSize, string lotSizeText, string lotSizeType, double maxGuaranteedStopOrderSize,
        double minimumLotSize, double minimumOrderValue, double minimumTradeSize, string minimumTradeSizeCurrency, string nonTradableReason, DateTime noticeDate, bool optionsChainSubscriptionAllowed, OrderDistances orderDistances,
        OrderSetting orderSetting, string priceCurrency, double priceToContractFactor, int primaryListing, string putCall, int redemptionDays, string redemptionDaysType, InstrumentKey[] relatedInstruments,
        int[] relatedOptionRoots, RelatedOptionRoot[] relatedOptionRootsEnhanced, string settlementStyle, bool shortTradeDisabled, double[] standardAmounts, string strategyType,
        double strikePrice, double subscriptionByAmounts, double subscriptionByShares, int subscriptionDays, string subscriptionDaysType, string[] supportedOrderTypes, SupportedOrderTypeSetting[] supportedOrderTypeSettings,
        string[] supportedStrategies, string symbol, double tickSize, double tickSizeLimitOrder, TickSizeScheme tickSizeScheme, double tickSizeStopOrder, string[] tradableAs, string[] tradableOn,
        InstrumentTradeSessions tradingSessions, string tradingStatus, string tradingUnitsPlural, string tradingUnitsSingular, int uic, string underlyingAssetType, string underlyingTypeCategory, string[] supportedOrderTriggerPriceTypes,
        string tradingSignals)
    {
        AffiliateInfoRequired = affiliateInfoRequired;
        AdditionalMinimumTradeSize = additionalMinimumTradeSize;
        AdditionalMinimumTradeUnits = additionalMinimumTradeUnits;
        AmountDecimals = amountDecimals;
        AssetType = assetType;
        BondIndexRatio = bondIndexRatio;
        BondType = bondType;
        CanParticipateInMultiLegOrder = canParticipateInMultiLegOrder;
        ContractSize = contractSize;
        Coupon = coupon;
        CouponType = couponType;
        CurrencyCode = currencyCode;
        CutOffTimeForSubscriptions = cutOffTimeForSubscriptions;
        DefaultAmount = defaultAmount;
        DefaultSlippage = defaultSlippage;
        DefaultSlippageType = defaultSlippageType;
        DeflationFloorProtectionType = deflationFloorProtectionType;
        Description = description;
        DisplayHint = displayHint;
        DistributionPolicy = distributionPolicy;
        DividendPeriodicity = dividendPeriodicity;
        Exchange = exchange;
        ExerciseCutOffTime = exerciseCutOffTime;
        ExpiryDate = expiryDate;
        ExpiryDateTime = expiryDateTime;
        Format = format;
        FractionalMinimumLotSize = fractionalMinimumLotSize;
        FxForwardMaxForwardDate = fxForwardMaxForwardDate;
        FxForwardMinForwardDate = fxForwardMinForwardDate;
        FxSpotDate = fxSpotDate;
        GroupId = groupId;
        IncrementSize = incrementSize;
        IpoDetails = ipoDetails;
        IsComplex = isComplex;
        IsDmaEnabled = isDmaEnabled;
        IsExtendedTradingHoursEnabled = isExtendedTradingHoursEnabled;
        IsPEAEligible = isPEAEligible;
        IsPEASMEEligible = isPEASMEEligible;
        IsPitTraded = isPitTraded;
        IsRedemptionByAmounts = isRedemptionByAmounts;
        IsTradable = isTradable;
        Legs = legs;
        LotSize = lotSize;
        LotSizeText = lotSizeText;
        LotSizeType = lotSizeType;
        MaxGuaranteedStopOrderSize = maxGuaranteedStopOrderSize;
        MinimumLotSize = minimumLotSize;
        MinimumOrderValue = minimumOrderValue;
        MinimumTradeSize = minimumTradeSize;
        MinimumTradeSizeCurrency = minimumTradeSizeCurrency;
        NonTradableReason = nonTradableReason;
        NoticeDate = noticeDate;
        OptionsChainSubscriptionAllowed = optionsChainSubscriptionAllowed;
        OrderDistances = orderDistances;
        OrderSetting = orderSetting;
        PriceCurrency = priceCurrency;
        PriceToContractFactor = priceToContractFactor;
        PrimaryListing = primaryListing;
        PutCall = putCall;
        RedemptionDays = redemptionDays;
        RedemptionDaysType = redemptionDaysType;
        RelatedInstruments = relatedInstruments;
        RelatedOptionRoots = relatedOptionRoots;
        RelatedOptionRootsEnhanced = relatedOptionRootsEnhanced;
        SettlementStyle = settlementStyle;
        ShortTradeDisabled = shortTradeDisabled;
        StandardAmounts = standardAmounts;
        StrategyType = strategyType;
        StrikePrice = strikePrice;
        SubscriptionByAmounts = subscriptionByAmounts;
        SubscriptionByShares = subscriptionByShares;
        SubscriptionDays = subscriptionDays;
        SubscriptionDaysType = subscriptionDaysType;
        SupportedOrderTypes = supportedOrderTypes;
        SupportedOrderTypeSettings = supportedOrderTypeSettings;
        SupportedStrategies = supportedStrategies;
        Symbol = symbol;
        TickSize = tickSize;
        TickSizeLimitOrder = tickSizeLimitOrder;
        TickSizeScheme = tickSizeScheme;
        TickSizeStopOrder = tickSizeStopOrder;
        TradableAs = tradableAs;
        TradableOn = tradableOn;
        TradingSessions = tradingSessions;
        TradingStatus = tradingStatus;
        TradingUnitsPlural = tradingUnitsPlural;
        TradingUnitsSingular = tradingUnitsSingular;
        Uic = uic;
        UnderlyingAssetType = underlyingAssetType;
        UnderlyingTypeCategory = underlyingTypeCategory;
        SupportedOrderTriggerPriceTypes = supportedOrderTriggerPriceTypes;
        TradingSignals = tradingSignals;
    }
}

public struct ExchangeSummary
{
    public string CountryCode { get; }
    public string ExchangeId { get; }
    public string Name { get; }
    public string PriceSourceName { get; }
    public string TimeZoneId { get; }

    [JsonConstructor]
    public ExchangeSummary(string countryCode, string exchangeId, string name, string priceSourceName, string timeZoneId)
    {
        CountryCode = countryCode;
        ExchangeId = exchangeId;
        Name = name;
        PriceSourceName = priceSourceName;
        TimeZoneId = timeZoneId;
    }
}

public struct PriceDisplayFormat
{
    public int BarrierDecimals { get; }
    public string BarriereFormat { get; }
    public int Decimals { get; }
    public string Format { get; }
    public int NumeratorDecimals { get; }
    public int OrderDecimals { get; }
    public string PriceCurrency { get; }
    public int StrikeDecimals { get; }
    public string StrikeFormat { get; }

    [JsonConstructor]
    public PriceDisplayFormat(int barriereDecimals, string barriereFormat, int decimals, string format, int numeratorDecimals, int orderDecimals, string priceCurrency, int strikeDecimals, string strikeFormat)
    {
        BarrierDecimals = barriereDecimals;
        BarriereFormat = barriereFormat;
        Decimals = decimals;
        Format = format;
        NumeratorDecimals = numeratorDecimals;
        OrderDecimals = orderDecimals;
        PriceCurrency = priceCurrency;
        StrikeDecimals = strikeDecimals;
        StrikeFormat = strikeFormat;
    }
}

public struct OrderDistances
{
    public float EntryDefaultDistance { get;  }
    public string EntryDefaultDistanceType { get; }
    public float LimitDefaultDistance { get; }
    public string LimitDefaultDistanceType { get; }
    public float StopLimitDefaultDistance { get; }
    public string StopLimitDefaultDistanceType { get; }
    public float StopLossDefaultDistance { get; }
    public string StopLossDefaultDistanceType { get; }
    public bool StopLossDefaultEnabled { get; }
    public string StopLossDefaultOrderType { get; }
    public float TakeProfitDefaultDistance { get; }
    public string TakeProfitDefaultDistanceType { get; }
    public bool TakeProfitDefaultEnabled { get; }

    [JsonConstructor]
    public OrderDistances(float entryDefaultDistance, string entryDefaultDistanceType, float limitDefaultDistance, string limitDefaultDistanceType,
        float stopLimitDefaultDistance, string stopLimitDefaultDistanceType, float stopLossDefaultDistance, string stopLossDefaultDistanceType,
        bool stopLossDefaultEnabled, string stopLossDefaultOrderType, float takeProfitDefaultDistance, string takeProfitDefaultDistanceType,
        bool takeProfitDefaultEnabled)
    {
        EntryDefaultDistance = entryDefaultDistance;
        EntryDefaultDistanceType = entryDefaultDistanceType;
        LimitDefaultDistance = limitDefaultDistance;
        LimitDefaultDistanceType = limitDefaultDistanceType;
        StopLimitDefaultDistance = stopLimitDefaultDistance;
        StopLimitDefaultDistanceType = stopLimitDefaultDistanceType;
        StopLossDefaultDistance = stopLossDefaultDistance;
        StopLossDefaultDistanceType = stopLossDefaultDistanceType;
        StopLossDefaultEnabled = stopLossDefaultEnabled;
        StopLossDefaultOrderType = stopLossDefaultOrderType;
        TakeProfitDefaultDistance = takeProfitDefaultDistance;
        TakeProfitDefaultDistanceType = takeProfitDefaultDistanceType;
        TakeProfitDefaultEnabled = takeProfitDefaultEnabled;
    }
}

public struct InstrumentKey
{
    public SaxoAssetType AssetType { get; }
    public int Uic { get; }

    [JsonConstructor]
    public InstrumentKey(SaxoAssetType assettype, int uic)
    {
        AssetType = assettype;
        Uic = uic;
    }
}

public struct RelatedOptionRoot
{
    public SaxoAssetType AssetType { get; }
    public int OptionRootId { get; }
    public string OptionType { get; }
    public string[] SupportedStrategies { get; }

    [JsonConstructor]
    public RelatedOptionRoot(SaxoAssetType assetType, int optionRootId, string optionType, string[] supportedStrategies)
    {
        AssetType = assetType;
        OptionRootId = optionRootId;
        optionType = OptionType;
        SupportedStrategies = supportedStrategies;
    }
}

public struct TickSizeSchemeElement
{
    public float HighPrice { get; }
    public float TickSize { get; }

    [JsonConstructor]
    public TickSizeSchemeElement(float highPrice, float tickSize)
    {
        HighPrice = highPrice;
        TickSize = tickSize;
    }
}

public struct TickSizeScheme
{
    public float DefaultTickSize { get; }

    public TickSizeSchemeElement[] Elements { get; }

    [JsonConstructor]
    public TickSizeScheme(float defaultTickSize, TickSizeSchemeElement[] elements)
    {
        DefaultTickSize = defaultTickSize;
        Elements = elements;
    }
}

public struct LocalizedInfo
{
    public string Key { get; }
    public string Value { get; }

    [JsonConstructor]
    public LocalizedInfo(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

public struct IpoDetails
{
    public DateTime ActiviationFrom { get; }
    public DateTime ActiviationTo { get; }
    public DateTime AllotmentDate { get; }
    public int[] Denominations { get; }
    public DateTime ListingDate { get; }
    public DateTime MarketDeadline { get; }
    public int MaxLeveragePct { get; }
    public int MaxLotSize { get; }

    [JsonConstructor]
    public IpoDetails(DateTime activiationFrom, DateTime activiationTo, DateTime allotmentDate, int[] denominations, DateTime listingDate, DateTime marketDeadline, int maxLeveragePct, int maxLotSize)
    {
        ActiviationFrom = activiationFrom;
        ActiviationTo = activiationTo;
        AllotmentDate = allotmentDate;
        Denominations = denominations;
        ListingDate = listingDate;
        MarketDeadline = marketDeadline;
        MaxLeveragePct = maxLeveragePct;
        MaxLotSize = maxLotSize;
    }
}

public struct StrategyLeg
{
    public SaxoAssetType AssetType { get; }
    public string BuySell { get; }

    public string Description { get; }

    public int LegNumber { get; }

    public float Multiplier { get; }

    public int Uic { get; }

    [JsonConstructor]
    public StrategyLeg(SaxoAssetType assetType, string buySell, string description, int legNumber, float multiplier, int uic)
    {
        AssetType = assetType;
        BuySell = buySell;
        Description = description;
        LegNumber = legNumber;
        Multiplier = multiplier;
        Uic = uic;
    }
}

public struct OrderSetting
{
    public string Currency { get; }
    public int MaxOrderSize { get; }   
    public int MaxOrderValue { get; }
    public int MinOrderValue { get; }

    [JsonConstructor]
    public OrderSetting(string currency, int maxOrderSize, int maxOrderValue, int minOrderValue)
    {
        Currency = currency;
        MaxOrderSize = maxOrderSize;
        MaxOrderValue = maxOrderValue;
        MinOrderValue = minOrderValue;
    }
}

public struct SupportedOrderTypeSetting
{
    public string[] DurationTypes { get; }
    public SaxoOrderType OrderType { get; }

    [JsonConstructor]
    public SupportedOrderTypeSetting(string[] durationTypes, SaxoOrderType orderType)
    {
        DurationTypes = durationTypes;
        OrderType = orderType;
    }
}

public struct InstrumentSession
{
    public DateTime EndTime { get; }
    public DateTime StartTime { get; }

    public string InstrumentSessionState { get; }

    [JsonConstructor]
    public InstrumentSession(DateTime endTime, DateTime startTime, string instrumentSessionState)
    {
        EndTime = endTime;
        StartTime = startTime;
        InstrumentSessionState = instrumentSessionState;
    }
}

public struct InstrumentTradeSessions
{
    public InstrumentSession[] Sessions { get; }
    public int TimeZone { get; }
    public string TimeZoneAbbreviation { get; }

    public string TimeZoneOffset { get; }

    [JsonConstructor]
    public InstrumentTradeSessions(InstrumentSession[] sessions, int timeZone, string timeZoneAbbreviation, string timeZoneOffset)
    {
        Sessions = sessions;
        TimeZone = timeZone;
        TimeZoneAbbreviation = timeZoneAbbreviation;
        TimeZoneOffset = timeZoneOffset;
    }
}