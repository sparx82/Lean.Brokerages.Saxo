using QuantConnect.Brokerages.Saxo.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoAccount
{
    public float AccountValueProtectionLimit { get; }
    public string[] AllowedNettingProfiles { get; }
    public string AllowedTradingSessions { get; }
    public string ClientId { get; }
    public string ClientKey { get; }

    public string ClientType { get; }
    public string CollateralMonitoringMode { get; }
    public string ContractOptionsTradingProfile { get; }
    public string ContractType { get; }
    public int CurrencyDecimals { get; }
    public string DefaultAccountId { get; }
    public string DefaultAccountKey { get; }
    public string DefaultCurrency { get; }
    public bool ForceOpenDefaultValue { get; }
    public bool IsMarginTradingAllowed { get; }
    public bool IsVariationMarginEligible { get; }
    public SaxoAssetType[] LegalAssetTypes { get; }
    public bool LegalAssetTypesAreIndicative { get; }
    public string MarginCalculationMethod { get; }
    public string MarginMonitoringMode { get; }
    public string MutualFundsCashAmountOrderCurrency { get; }
    public string Name { get; }
    public string PartnerPlatformID { get; }
    public string PositionNettingMethod { get; }
    public string PositionNettingMode { get; }
    public string PositionNettingProfile { get; }
    public string ReduceExposureOnly { get; }
    public string SecurityLendingEnabled { get; }
    public string SupportsAccountValueProtectionLimit { get; }

    [JsonConstructor]
    public SaxoAccount(float accountValueProtectionLimit, string[] allowedNettingProfiles, string allowedTradingSessions, string clientId,
        string clientKey, string clientType, string collateralMonitoringMode, string contractOptionsTradingProfile, string contractType, int currencyDecimals,
        string defaultAccountId, string defaultAccountKey, string defaultCurrency, bool forceOpenDefaultValue, bool isMarginTradingAllowed,
        bool isVariationMarginEligible, SaxoAssetType[] legalAssetTypes, bool legalAssetTypesAreIndicative, string marginCalculationMethod,
        string marginMonitoringMode, string mutualFundsCashAmountOrderCurrency, string name, string positionNettingMethod, string partnerPlatformID,
        string positionNettingMode, string positionNettingProfile, string reduceExposureOnly, string securityLendingEnabled, string supportsAccountValueProtectionLimit)
    {
        AccountValueProtectionLimit = accountValueProtectionLimit;
        AllowedNettingProfiles = allowedNettingProfiles;
        AllowedTradingSessions = allowedTradingSessions;
        ClientId = clientId;
        ClientKey = clientKey;
        ClientType = clientType;
        CollateralMonitoringMode = collateralMonitoringMode;
        ContractOptionsTradingProfile = contractOptionsTradingProfile;
        ContractType = contractType;
        CurrencyDecimals = currencyDecimals;
        DefaultAccountId = defaultAccountId;
        DefaultAccountKey = defaultAccountKey;
        DefaultCurrency = defaultCurrency;
        ForceOpenDefaultValue = forceOpenDefaultValue;
        IsMarginTradingAllowed = isMarginTradingAllowed;
        IsVariationMarginEligible = isVariationMarginEligible;
        LegalAssetTypes = legalAssetTypes;
        LegalAssetTypesAreIndicative = legalAssetTypesAreIndicative;
        MarginCalculationMethod = marginCalculationMethod;
        MarginMonitoringMode = marginMonitoringMode;
        MutualFundsCashAmountOrderCurrency = mutualFundsCashAmountOrderCurrency;
        Name = name;
        PartnerPlatformID = partnerPlatformID;
        PositionNettingMethod = positionNettingMethod;
        PositionNettingMode = positionNettingMode;
        PositionNettingProfile = positionNettingProfile;
        ReduceExposureOnly = reduceExposureOnly;
        SecurityLendingEnabled = securityLendingEnabled;
        SupportsAccountValueProtectionLimit = supportsAccountValueProtectionLimit;
    }
}
