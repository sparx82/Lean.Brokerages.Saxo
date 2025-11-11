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

using System;
using QuantConnect.Packets;
using QuantConnect.Configuration;
using QuantConnect.Brokerages;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using System.Collections.Generic;

namespace QuantConnect.Brokerages.Saxo;

/// <summary>
/// Provides a template implementation of BrokerageFactory
/// </summary>
public class SaxoBrokerageFactory : BrokerageFactory
{
    /// <summary>
    /// Gets the brokerage data required to run the brokerage from configuration/disk
    /// </summary>
    /// <remarks>
    /// The implementation of this property will create the brokerage data dictionary required for
    /// running live jobs. See <see cref="IJobQueueHandler.NextJob"/>
    /// </remarks>
    public override Dictionary<string, string> BrokerageData => new Dictionary<string, string>
    {
        { "saxo-client-id", Config.Get("saxo-client-id")},
        { "saxo-redirect-url", Config.Get("saxo-redirect-url")},
        { "saxo-api-url", Config.Get("saxo-api-url")},
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SaxoBrokerageFactory"/> class
    /// </summary>
    public SaxoBrokerageFactory() : base(typeof(SaxoBrokerage))
    {
    }

    /// <summary>
    /// Gets a brokerage model that can be used to model this brokerage's unique behaviors
    /// </summary>
    /// <param name="orderProvider">The order provider</param>
    public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new SaxoBrokerageModel();

    /// <summary>
    /// Creates a new IBrokerage instance
    /// </summary>
    /// <param name="job">The job packet to create the brokerage for</param>
    /// <param name="algorithm">The algorithm instance</param>
    /// <returns>A new brokerage instance</returns>
    public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
    {
        var errors = new List<string>();

        var clientId = Read<string>(job.BrokerageData, "saxo-client-id", errors);
        var apiUrl = Read<string>(job.BrokerageData, "saxo-api-url", errors);

        if (errors.Count != 0)
        {
            // if we had errors then we can't create the instance
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }

        var brokerage = default(SaxoBrokerage);
        var redirectUrl = Read<string>(job.BrokerageData, "saxo-redirect-url", errors);

        if (string.IsNullOrEmpty(redirectUrl))
        {
            throw new ArgumentException("RedirectUrl or AuthorizationCode cannot be empty or null. Please ensure these values are correctly set in the configuration file.");
        }

        // Case 1: authentication with using redirectUrl, authorizationCode
        brokerage = new SaxoBrokerage(clientId, apiUrl, redirectUrl);

        return brokerage;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}