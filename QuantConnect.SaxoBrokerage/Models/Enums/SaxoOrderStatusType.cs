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
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace QuantConnect.Brokerages.Saxo.Models.Enums;

//ToDo: Those error descriptions come attached to HTTP Error 400

[JsonConverter(typeof(StringEnumConverter))]
public enum SaxoOrderStatusType
{
    [EnumMember(Value = "ActiveFollowerCannotDoManualTrade")]
    ActiveFollowerCannotDoManualTrade,

    [EnumMember(Value = "AlgoOrderTimeBeforeExchangeOpen")]
    AlgoOrderTimeBeforeExchangeOpen,

    [EnumMember(Value = "AmountBelowMinimumLotSize")]
    AmountBelowMinimumLotSize,

    [EnumMember(Value = "AmountCannotBeLessThanFilledAmount")]
    AmountCannotBeLessThanFilledAmount,

    [EnumMember(Value = "AmountNotInLotSize")]
    AmountNotInLotSize,

    [EnumMember(Value = "BlocksExistForOwnedContracts")]
    BlocksExistForOwnedContracts,

    [EnumMember(Value = "BreakoutUpPriceCannotBeLessThanDownPrice")]
    BreakoutUpPriceCannotBeLessThanDownPrice,

    [EnumMember(Value = "BrokerOption")]
    BrokerOption,

    [EnumMember(Value = "CashAmountLessThanMinInitialInvestmentAmount")]
    CashAmountLessThanMinInitialInvestmentAmount,

    [EnumMember(Value = "ClientCountrySanctioned")]
    ClientCountrySanctioned,

    [EnumMember(Value = "ClientExposureLimitation")]
    ClientExposureLimitation,

    [EnumMember(Value = "ClientNotEnabledForExtendedTradingHours")]
    ClientNotEnabledForExtendedTradingHours,

    [EnumMember(Value = "ClientOnReduceForcedExposureReductionViolation")]
    ClientOnReduceForcedExposureReductionViolation,

    [EnumMember(Value = "ClosingPositionNotAllowedForMultiDayExecutionOrder")]
    ClosingPositionNotAllowedForMultiDayExecutionOrder,

    [EnumMember(Value = "ClosingPositionNotAllowedOnJointExecutionModel")]
    ClosingPositionNotAllowedOnJointExecutionModel,

    [EnumMember(Value = "ConditionDurationNotSupported")]
    ConditionDurationNotSupported,

    [EnumMember(Value = "ConditionOnWrongSideOfMarket")]
    ConditionOnWrongSideOfMarket,

    [EnumMember(Value = "ConditionPriceExceedsAggressiveTolerance")]
    ConditionPriceExceedsAggressiveTolerance,

    [EnumMember(Value = "ConditionTooFarFromMarket")]
    ConditionTooFarFromMarket,

    [EnumMember(Value = "ContactExposureLimitation")]
    ContactExposureLimitation,

    [EnumMember(Value = "CouldNotCompleteRequest")]
    CouldNotCompleteRequest,

    [EnumMember(Value = "CrossCurrencyOrderOnMarginLendingAccountNotAllowed")]
    CrossCurrencyOrderOnMarginLendingAccountNotAllowed,

    [EnumMember(Value = "DealCaptureAllocationKeyNotCorrect")]
    DealCaptureAllocationKeyNotCorrect,

    [EnumMember(Value = "DealCaptureInformationNotCorrect")]
    DealCaptureInformationNotCorrect,

    [EnumMember(Value = "DurationNotSupported")]
    DurationNotSupported,

    [EnumMember(Value = "ExpirationDateInPast")]
    ExpirationDateInPast,

    [EnumMember(Value = "ExpirationDateRequired")]
    ExpirationDateRequired,

    [EnumMember(Value = "ExplicitCloseNotAllowedForIntradayNetting")]
    ExplicitCloseNotAllowedForIntradayNetting,

    [EnumMember(Value = "ExtendedHoursTradingCannotBeChanged")]
    ExtendedHoursTradingCannotBeChanged,

    [EnumMember(Value = "ForceOpenNotAllowed")]
    ForceOpenNotAllowed,

    [EnumMember(Value = "ForcedExposureReductionViolation")]
    ForcedExposureReductionViolation,

    [EnumMember(Value = "ForwardDateInPast")]
    ForwardDateInPast,

    [EnumMember(Value = "ForwardDateRequired")]
    ForwardDateRequired,

    [EnumMember(Value = "GtdOrderCannotBeLaterThanExpiry")]
    GtdOrderCannotBeLaterThanExpiry,

    [EnumMember(Value = "IllegalAccount")]
    IllegalAccount,

    [EnumMember(Value = "IllegalAmount")]
    IllegalAmount,

    [EnumMember(Value = "IllegalAssetType")]
    IllegalAssetType,

    [EnumMember(Value = "IllegalDate")]
    IllegalDate,

    [EnumMember(Value = "IllegalInstrumentId")]
    IllegalInstrumentId,

    [EnumMember(Value = "IllegalRequest")]
    IllegalRequest,

    [EnumMember(Value = "IllegalStrike")]
    IllegalStrike,

    [EnumMember(Value = "IncorrectValuationTypeAndNettingMethod")]
    IncorrectValuationTypeAndNettingMethod,

    [EnumMember(Value = "InstrumentDisabledForTrading")]
    InstrumentDisabledForTrading,

    [EnumMember(Value = "InstrumentForcedExposureReductionViolation")]
    InstrumentForcedExposureReductionViolation,

    [EnumMember(Value = "InstrumentHasExpired")]
    InstrumentHasExpired,

    [EnumMember(Value = "InstrumentNotAllowed")]
    InstrumentNotAllowed,

    [EnumMember(Value = "InstrumentNotSupportedForExtendedHours")]
    InstrumentNotSupportedForExtendedHours,

    [EnumMember(Value = "InsufficientCash")]
    InsufficientCash,

    [EnumMember(Value = "InsufficientCash_CausedByOvernightAddOn")]
    InsufficientCash_CausedByOvernightAddOn,

    [EnumMember(Value = "InvalidAllocationKeyUsed")]
    InvalidAllocationKeyUsed,

    [EnumMember(Value = "InvalidModelState")]
    InvalidModelState,

    [EnumMember(Value = "InvalidOrderRelation")]
    InvalidOrderRelation,

    [EnumMember(Value = "InvalidRequest")]
    InvalidRequest,

    [EnumMember(Value = "InvalidUic")]
    InvalidUic,

    [EnumMember(Value = "InvalidValueDate")]
    InvalidValueDate,

    [EnumMember(Value = "LimitTooFarFromStop")]
    LimitTooFarFromStop,

    [EnumMember(Value = "MarketClosed")]
    MarketClosed,

    [EnumMember(Value = "NotOwned")]
    NotOwned,

    [EnumMember(Value = "NotPrimarySession")]
    NotPrimarySession,

    [EnumMember(Value = "NotSuitable")]
    NotSuitable,

    [EnumMember(Value = "NotTradableAtPresent")]
    NotTradableAtPresent,

    [EnumMember(Value = "OnWrongSideOfMarket")]
    OnWrongSideOfMarket,

    [EnumMember(Value = "OnlyLimitOrderAllowedForExtendedHours")]
    OnlyLimitOrderAllowedForExtendedHours,

    [EnumMember(Value = "OnlySidedReductionAllowed")]
    OnlySidedReductionAllowed,

    [EnumMember(Value = "OptionExerciseAfterCutoff")]
    OptionExerciseAfterCutoff,

    [EnumMember(Value = "OptionExerciseNotAllowedDueExDateOfUpcomingCorporateActionOnInstrument")]
    OptionExerciseNotAllowedDueExDateOfUpcomingCorporateActionOnInstrument,

    [EnumMember(Value = "OrderCommandPending")]
    OrderCommandPending,

    [EnumMember(Value = "OrderCommandTimeout")]
    OrderCommandTimeout,

    [EnumMember(Value = "OrderExceedsMarketLimitUpDownTolerance")]
    OrderExceedsMarketLimitUpDownTolerance,

    [EnumMember(Value = "OrderIsRestrictedForAccountManagementType")]
    OrderIsRestrictedForAccountManagementType,

    [EnumMember(Value = "OrderNotFound")]
    OrderNotFound,

    [EnumMember(Value = "OrderNotPlaced")]
    OrderNotPlaced,

    [EnumMember(Value = "OrderNotSupportedForAccountType")]
    OrderNotSupportedForAccountType,

    [EnumMember(Value = "OrderPriceOutsideLimit")]
    OrderPriceOutsideLimit,

    [EnumMember(Value = "OrderRejectedByBroker")]
    OrderRejectedByBroker,

    [EnumMember(Value = "OrderRelatedPositionIsClosed")]
    OrderRelatedPositionIsClosed,

    [EnumMember(Value = "OrderRelatedPositionMissMatch")]
    OrderRelatedPositionMissMatch,

    [EnumMember(Value = "OrderRequestAfterLastTradingDate")]
    OrderRequestAfterLastTradingDate,

    [EnumMember(Value = "OrderSizeGreaterThanMaximumAllowed")]
    OrderSizeGreaterThanMaximumAllowed,

    [EnumMember(Value = "OrderTypeNotSupported")]
    OrderTypeNotSupported,

    [EnumMember(Value = "OrderValueToSmall")]
    OrderValueToSmall,

    [EnumMember(Value = "OrderValueTooLarge")]
    OrderValueTooLarge,

    [EnumMember(Value = "OtherError")]
    OtherError,

    [EnumMember(Value = "PriceExceedsAggressiveTolerance")]
    PriceExceedsAggressiveTolerance,

    [EnumMember(Value = "PriceHasMoved")]
    PriceHasMoved,

    [EnumMember(Value = "PriceNotInTickSizeIncrements")]
    PriceNotInTickSizeIncrements,

    [EnumMember(Value = "RelatedOrClosingOrderNotAllowedForIntradayNettingClients")]
    RelatedOrClosingOrderNotAllowedForIntradayNettingClients,

    [EnumMember(Value = "RelatedOrderNotAllowedForInstrument")]
    RelatedOrderNotAllowedForInstrument,

    [EnumMember(Value = "RelatedOrderNotAllowedForIpo")]
    RelatedOrderNotAllowedForIpo,

    [EnumMember(Value = "RelatedOrderNotAllowedForShortLived")]
    RelatedOrderNotAllowedForShortLived,

    [EnumMember(Value = "RelatedOrderNotAllowedOnChildOrder")]
    RelatedOrderNotAllowedOnChildOrder,

    [EnumMember(Value = "RelatedOrderNotAllowedOnMultiLegPartialFilled")]
    RelatedOrderNotAllowedOnMultiLegPartialFilled,

    [EnumMember(Value = "RelatedOrderNotAllowedOnTrailingStopOrder")]
    RelatedOrderNotAllowedOnTrailingStopOrder,

    [EnumMember(Value = "RelatedPositionAlreadyHasImplicitClosingOrders")]
    RelatedPositionAlreadyHasImplicitClosingOrders,

    [EnumMember(Value = "RelatedPositionAlreadyHasRelatedOrderType")]
    RelatedPositionAlreadyHasRelatedOrderType,

    [EnumMember(Value = "RelatedPositionAlreadyHasRelatedOrders")]
    RelatedPositionAlreadyHasRelatedOrders,

    [EnumMember(Value = "RelatedPositionNotFound")]
    RelatedPositionNotFound,

    [EnumMember(Value = "RepeatTradeOnAutoQuote")]
    RepeatTradeOnAutoQuote,

    [EnumMember(Value = "ReservedByShortContractOptions")]
    ReservedByShortContractOptions,

    [EnumMember(Value = "SellExceedsSettledExposure")]
    SellExceedsSettledExposure,

    [EnumMember(Value = "SellOrdersAlreadyExistForOwnedContracts")]
    SellOrdersAlreadyExistForOwnedContracts,

    [EnumMember(Value = "ShortTradeDisabled")]
    ShortTradeDisabled,

    [EnumMember(Value = "ToOpenNotAllowedOnInstrument")]
    ToOpenNotAllowedOnInstrument,

    [EnumMember(Value = "TooCloseToMarket")]
    TooCloseToMarket,

    [EnumMember(Value = "TooCloseToOcoRelatedOrderPrice")]
    TooCloseToOcoRelatedOrderPrice,

    [EnumMember(Value = "TooFarFromEntryOrder")]
    TooFarFromEntryOrder,

    [EnumMember(Value = "TooFarFromMarket")]
    TooFarFromMarket,

    [EnumMember(Value = "TradingDisabled")]
    TradingDisabled,

    [EnumMember(Value = "TradingOnNonFundedAccount")]
    TradingOnNonFundedAccount,

    [EnumMember(Value = "WouldComeTooCloseToAccountValueProtectionLimit")]
    WouldComeTooCloseToAccountValueProtectionLimit,

    [EnumMember(Value = "WouldExceedAccountValueProtectionLimit")]
    WouldExceedAccountValueProtectionLimit,

    [EnumMember(Value = "WouldExceedMargin")]
    WouldExceedMargin,

    [EnumMember(Value = "WouldExceedMarginCeiling")]
    WouldExceedMarginCeiling,

    [EnumMember(Value = "WouldExceedMaxCreditLine")]
    WouldExceedMaxCreditLine,

    [EnumMember(Value = "WouldExceedMaxCreditLineLimit")]
    WouldExceedMaxCreditLineLimit,

    [EnumMember(Value = "WouldExceedTradingLine")]
    WouldExceedTradingLine,

    [EnumMember(Value = "WrongSideOfRelatedOrder")]
    WrongSideOfRelatedOrder,
}
