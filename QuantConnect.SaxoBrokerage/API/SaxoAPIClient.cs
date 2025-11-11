using Newtonsoft.Json;
using QuantConnect.Brokerages.Saxo.Models;
//using QuantConnect.Brokerages.Saxo.Models.Enums;
using QuantConnect.Brokerages.Saxo.Models.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using Lean = QuantConnect.Orders;

namespace QuantConnect.Brokerages.Saxo.API;

public class SaxoAPIClient : IDisposable
{
    /// <summary>
    /// Represents the API Key used by the client application to authenticate requests.
    /// </summary>
    /// <remarks>
    /// In the documentation, this API Key is referred to as <c>client_id</c>.
    /// </remarks>
    private readonly string _clientId;

    private bool _isConnected;

    /// <summary>
    /// Represents the URI to which the user will be redirected after authentication.
    /// </summary>
    private readonly string _redirectUri;

    /// <summary>
    /// Gets or sets the JSON serializer settings used for serialization.
    /// </summary>
    private JsonSerializerSettings jsonSerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore };

    /// <summary>
    /// HttpClient is used for making HTTP requests and handling HTTP responses from web resources identified by a Uri.
    /// </summary>
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// The base URL used for constructing API endpoints.
    /// </summary>
    private readonly string _baseUrl;

    public SaxoAPIClient(string clientId, string restApiUrl, string redirectUri)
    {
        _clientId = clientId;
        _redirectUri = redirectUri;
        _baseUrl = restApiUrl;

        var _authorizationCode = "";
        var _refreshToken = "";

        //SaxoAuthenticate();

        var config = new SaxoOAuthConfig
        {
            AppKey = _clientId,
            AppUrl = _redirectUri,
            AuthenticationUrl = "https://sim.logonvalidation.net",
            OpenApiBaseUrl = "https://gateway.saxobank.com/sim/openapi/",
            RedirectPort = 5000
        };

        Log.Trace("SaxoBrokerage.Connect(): Connecting...");

        var oauthClient = new SaxoOAuthClient(config);
        _authorizationCode = oauthClient.GetAuthCode().SynchronouslyAwaitTaskResult();

        var httpClientHandler = new HttpClientHandler();
        var signInUri = "https://sim.logonvalidation.net";
        var tokenRefreshHandler = new TokenRefreshHandler(httpClientHandler, clientId, oauthClient._codeVerifier, _authorizationCode, signInUri, redirectUri, _refreshToken);
        _httpClient = new(tokenRefreshHandler);

        //var tokenResponse = oauthClient.ExchangeCodeForTokenAsync(_authorizationCode).SynchronouslyAwaitTaskResult();

        IEnumerable<Account>  saxo_account = GetCurrentClientDetails().SynchronouslyAwaitTaskResult();

        _isConnected = true;
    }

    public void Connect()
    {
        if (!_isConnected)
        {
            throw new NotImplementedException("SaxoAPIClient not conntected");
        }
        return;
    }

    public async Task<SaxoBalance> GetAccountBalance()
    {
        return await RequestAsync<SaxoBalance>(_baseUrl, "/port/v1/balances/me", HttpMethod.Get);
    }

    public async Task<IEnumerable<Account>> GetCurrentClientDetails()
    {
        return (await RequestAsync<SaxoAccount>(_baseUrl, "/port/v1/clients/me", HttpMethod.Get)).Accounts;
    }

    public async Task<SaxoOrderResponse> GetOrders()
    {
        return await RequestAsync<SaxoOrderResponse>(_baseUrl, "/port/v1/orders/me?FieldGroups=DisplayAndFormat", HttpMethod.Get);
    }

    private async Task<T> RequestAsync<T>(string baseUrl, string resource, HttpMethod httpMethod, string jsonBody = null)
    {
        using (var requestMessage = new HttpRequestMessage(httpMethod, $"{baseUrl}{resource}"))
        {
            if (jsonBody != null)
            {
                requestMessage.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            try
            {
                var repsonseMessage = await _httpClient.SendAsync(requestMessage);

                if (!repsonseMessage.IsSuccessStatusCode)
                {
                    throw new Exception(JsonConvert.DeserializeObject<SaxoError>(await repsonseMessage.Content.ReadAsStringAsync()).Message);
                }

                var response = await repsonseMessage.Content.ReadAsStringAsync();

                var deserializeResponse = JsonConvert.DeserializeObject<T>(response);

                if (deserializeResponse is ISaxoError errors && errors.Errors != null)
                {
                    foreach (var positionError in errors.Errors)
                    {
                        throw new Exception($"Error in {nameof(SaxoAPIClient)}.{nameof(RequestAsync)}: {positionError.Message} while accessing resource: {resource}");
                    }
                }
                return deserializeResponse;
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(SaxoAPIClient)}.{nameof(RequestAsync)}.Exception: {ex}. Request: {requestMessage.Method} {requestMessage.RequestUri}");
                throw;
            }
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance.
    /// </summary>
    public void Dispose()
    {
        _httpClient.DisposeSafely();
    }
}