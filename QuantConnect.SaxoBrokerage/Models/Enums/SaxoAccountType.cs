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

namespace QuantConnect.Brokerages.Saxo.Models.Enums;

/// <summary>
/// Specifies the type of account on Saxo.
/// </summary>
[JsonConverter(typeof(AccountTypeEnumConverter))]
public enum SaxoAccountType
{
    AutoTradingFollower = 1,
    AutoTradingLeader = 2,
    BlockTrading = 3,
    Collateral = 4,
    Commission = 5,
    Funding = 6,
    Interest = 7,
    MarginLending = 8,
    Normal = 9,
    Omnibus = 10,
    Other = 11,
    Pension = 12,
    Saving = 13,
    Settlement = 14,
    SettlementTrading = 15,
    Tax = 16,
    TaxFavoredAccount = 17,
    Unknown = 18,
}
