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

/// <summary>
/// Indicates the asset type of the position.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum SaxoAssetType
{
    [EnumMember(Value = "Unknown")]
    Unknown = 0,

    [EnumMember(Value = "Bond")]
    Bond = 1,

    [EnumMember(Value = "Cash")]
    Cash = 2,

    [EnumMember(Value = "CBBCCategoryN")]
    CBBCCategoryN = 3,

    [EnumMember(Value = "CBBCCategoryR")]
    CBBCCategoryR = 4,

    [EnumMember(Value = "CertificateBarrierDiscount")]
    CertificateBarrierDiscount = 5,

    [EnumMember(Value = "CertificateBarrierReverseConvertibles")]
    CertificateBarrierReverseConvertibles = 6,

    [EnumMember(Value = "CertificateBonus")]
    CertificateBonus = 7,

    [EnumMember(Value = "CertificateCapitalProtectionWithCoupon")]
    CertificateCapitalProtectionWithCoupon = 8,

    [EnumMember(Value = "CertificateCapitalProtectionWithKnockOut")]
    CertificateCapitalProtectionWithKnockOut = 9,

    [EnumMember(Value = "CertificateCappedBonus")]
    CertificateCappedBonus = 10,

    [EnumMember(Value = "CertificateCappedCapitalProtected")]
    CertificateCappedCapitalProtected = 11,

    [EnumMember(Value = "CertificateCappedOutperformance")]
    CertificateCappedOutperformance = 12,

    [EnumMember(Value = "CertificateConstantLeverage")]
    CertificateConstantLeverage = 13,

    [EnumMember(Value = "CertificateDiscount")]
    CertificateDiscount = 14,

    [EnumMember(Value = "CertificateExpress")]
    CertificateExpress = 15,

    [EnumMember(Value = "CertificateOtherCapitalProtection")]
    CertificateOtherCapitalProtection = 16,

    [EnumMember(Value = "CertificateOtherConstantLeverage")]
    CertificateOtherConstantLeverage = 17,

    [EnumMember(Value = "CertificateOtherParticipation")]
    CertificateOtherParticipation = 18,

    [EnumMember(Value = "CertificateOtherYieldEnhancement")]
    CertificateOtherYieldEnhancement = 19,

    [EnumMember(Value = "CertificateOutperformanceBonus")]
    CertificateOutperformanceBonus = 20,

    [EnumMember(Value = "CertificateReverseConvertibles")]
    CertificateReverseConvertibles = 21,

    [EnumMember(Value = "CertificateTracker")]
    CertificateTracker = 22,

    [EnumMember(Value = "CertificateTwinWin")]
    CertificateTwinWin = 23,

    [EnumMember(Value = "CertificateUncappedCapitalProtection")]
    CertificateUncappedCapitalProtection = 24,

    [EnumMember(Value = "CertificateUncappedOutperformance")]
    CertificateUncappedOutperformance = 25,

    [EnumMember(Value = "CfdIndexOption")]
    CfdIndexOption = 26,

    [EnumMember(Value = "CfdOnCompanyWarrant")]
    CfdOnCompanyWarrant = 27,

    [EnumMember(Value = "CfdOnEtc")]
    CfdOnEtc = 28,

    [EnumMember(Value = "CfdOnEtf")]
    CfdOnEtf = 29,

    [EnumMember(Value = "CfdOnEtn")]
    CfdOnEtn = 30,

    [EnumMember(Value = "CfdOnFund")]
    CfdOnFund = 31,

    [EnumMember(Value = "CfdOnFutures")]
    CfdOnFutures = 32,

    [EnumMember(Value = "CfdOnIndex")]
    CfdOnIndex = 33,

    [EnumMember(Value = "CfdOnRights")]
    CfdOnRights = 34,

    [EnumMember(Value = "CfdOnStock")]
    CfdOnStock = 35,

    [EnumMember(Value = "CompanyWarrant")]
    CompanyWarrant = 36,

    [EnumMember(Value = "ContractFutures")]
    ContractFutures = 37,

    [EnumMember(Value = "Etc")]
    Etc = 38,

    [EnumMember(Value = "Etf")]
    Etf = 39,

    [EnumMember(Value = "Etn")]
    Etn = 40,

    [EnumMember(Value = "Fund")]
    Fund = 41,

    [EnumMember(Value = "FuturesOption")]
    FuturesOption = 42,

    [EnumMember(Value = "FuturesStrategy")]
    FuturesStrategy = 43,

    [EnumMember(Value = "FxBinaryOption")]
    FxBinaryOption = 44,

    [EnumMember(Value = "FxCrypto")]
    FxCrypto = 45,

    [EnumMember(Value = "FxForwards")]
    FxForwards = 46,

    [EnumMember(Value = "FxKnockInOption")]
    FxKnockInOption = 47,

    [EnumMember(Value = "FxKnockOutOption")]
    FxKnockOutOption = 48,

    [EnumMember(Value = "FxNoTouchOption")]
    FxNoTouchOption = 49,

    [EnumMember(Value = "FxOneTouchOption")]
    FxOneTouchOption = 50,

    [EnumMember(Value = "FxSpot")]
    FxSpot = 51,

    [EnumMember(Value = "FxSwap")]
    FxSwap = 52,

    [EnumMember(Value = "FxVanillaOption")]
    FxVanillaOption = 53,

    [EnumMember(Value = "GuaranteeNote")]
    GuaranteeNote = 54,

    [EnumMember(Value = "InlineWarrant")]
    InlineWarrant = 55,

    [EnumMember(Value = "IpoOnStock")]
    IpoOnStock = 56,

    [EnumMember(Value = "ManagedFund")]
    ManagedFund = 57,

    [EnumMember(Value = "MiniFuture")]
    MiniFuture = 58,

    [EnumMember(Value = "MutualFund")]
    MutualFund = 59,

    [EnumMember(Value = "PortfolioNote")]
    PortfolioNote = 60,

    [EnumMember(Value = "Rights")]
    Rights = 61,

    [EnumMember(Value = "SrdOnEtf")]
    SrdOnEtf = 62,

    [EnumMember(Value = "SrdOnStock")]
    SrdOnStock = 63,

    [EnumMember(Value = "Stock")]
    Stock = 64,

    [EnumMember(Value = "StockIndex")]
    StockIndex = 65,

    [EnumMember(Value = "StockIndexOption")]
    StockIndexOption = 66,

    [EnumMember(Value = "StockOption")]
    StockOption = 67,

    [EnumMember(Value = "SubscriptionOnCertificate")]
    SubscriptionOnCertificate = 68,

    [EnumMember(Value = "Warrant")]
    Warrant = 69,

    [EnumMember(Value = "WarrantDoubleKnockOut")]
    WarrantDoubleKnockOut = 70,

    [EnumMember(Value = "WarrantKnockOut")]
    WarrantKnockOut = 71,

    [EnumMember(Value = "WarrantOpenEndKnockOut")]
    WarrantOpenEndKnockOut = 72,

    [EnumMember(Value = "WarrantOtherLeverageWithKnockOut")]
    WarrantOtherLeverageWithKnockOut = 73,

    [EnumMember(Value = "WarrantOtherLeverageWithoutKnockOut")]
    WarrantOtherLeverageWithoutKnockOut = 74,

    [EnumMember(Value = "WarrantSpread")]
    WarrantSpread = 75,
}
