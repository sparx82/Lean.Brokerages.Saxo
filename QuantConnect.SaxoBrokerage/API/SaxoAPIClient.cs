using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QuantConnect.Brokerages.Saxo.Models;
using QuantConnect.Brokerages.Saxo.Models.Enums;

//using QuantConnect.Brokerages.Saxo.Models.Enums;
using QuantConnect.Brokerages.Saxo.Models.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Orders;
using QuantConnect.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    
    /// <summary>
    /// Maximum number of bars that can be requested in a single call to <see cref="GetBars(string, TradeStationUnitTimeIntervalType, DateTime, DateTime)"/>.
    /// </summary>
    private const int MaxBars = 1200;
    
    private readonly string _clientId;

    private bool _isConnected;

    private string _fileAccessToken = "";

    private SaxoLoadTokenFromFile _SaxoLoadTokenFromFile = new SaxoLoadTokenFromFile();

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

        //SaxoAuthenticate();

        _fileAccessToken = _SaxoLoadTokenFromFile.LoadTokenFromFileAsync("C:\\AlgoTrading\\Authentication\\access_token.json").SynchronouslyAwaitTaskResult();
        var httpClientHandler = new HttpClientHandler();
        var tokenRefreshHandler = new TokenRefreshHandler(httpClientHandler, _fileAccessToken);
        _httpClient = new(tokenRefreshHandler);

        /*var config = new SaxoOAuthConfig
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
        _httpClient = new(tokenRefreshHandler);*/

        //var tokenResponse = oauthClient.ExchangeCodeForTokenAsync(_authorizationCode).SynchronouslyAwaitTaskResult();

        SaxoAccount  saxo_account = GetCurrentClientDetails().SynchronouslyAwaitTaskResult();

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

    public async Task<SaxoAccount> GetCurrentClientDetails()
    {
        return await RequestAsync<SaxoAccount>(_baseUrl, "/port/v1/clients/me", HttpMethod.Get);
    }

    public async Task<SaxoOrderResponse> GetOrders()
    {
        return await RequestAsync<SaxoOrderResponse>(_baseUrl, "/port/v1/orders/me?FieldGroups=DisplayAndFormat", HttpMethod.Get);
    }

    public async Task<SaxoInstrumentSearchResponse> SearchInstrumentsAsync(SaxoAssetType saxoAssetType, string keywords)
    {   
        var resource = $"/ref/v1/instruments?AssetTypes={saxoAssetType}&Keywords={keywords}";
        var response = await RequestAsync<SaxoInstrumentSearchResponse>(_baseUrl, resource, HttpMethod.Get);
        return response;
        //return await RequestAsync<SaxoInstrumentSearchResponse>(_baseUrl, resource, HttpMethod.Get);
    }

    public async Task<SaxoDetailedInstrumentInformation> GetInstrumentsDetailsAsync(string instrumentId, SaxoAssetType assetType)
    {
        var resource = $"/ref/v1/instruments/details/{instrumentId}/{assetType}";
        return await RequestAsync<SaxoDetailedInstrumentInformation>(_baseUrl, resource, HttpMethod.Get);
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

                //var deserializeResponse = JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings { TraceWriter = new ConsoleTraceWriter() });
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

    public class ConsoleTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter
        {
            // trace all messages (Verbose and above)
            get { return TraceLevel.Verbose; }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine(level.ToString() + ": " + message + " Ex: " + ex.Message);
            }
            else
            {
                Console.WriteLine(level.ToString() + ": " + message);
            }
        }
    }
    public async IAsyncEnumerable<SaxoChartSample> GetBars(SaxoAssetType assetType, int uic, SaxoUnitTimeIntervalType unitOfTime, DateTime firstDate, DateTime lastDate)
    {
        var count = MaxBars;
        var totalDateRange = lastDate - firstDate;

        var totalUnitTimeByIntervalTime = unitOfTime switch
        {
            SaxoUnitTimeIntervalType.Minute => totalDateRange.TotalMinutes,
            SaxoUnitTimeIntervalType.Hour => totalDateRange.TotalHours,
            SaxoUnitTimeIntervalType.Daily => totalDateRange.TotalDays,
            _ => throw new NotSupportedException($"{nameof(SaxoAPIClient)}.{nameof(GetBars)}: Unsupported time interval type '{unitOfTime}'")
        };

        var totalRequestAmount = totalUnitTimeByIntervalTime / MaxBars;
        var remainingCount = totalUnitTimeByIntervalTime % MaxBars;

        do
        {
            var newLastDate = unitOfTime switch
            {
                SaxoUnitTimeIntervalType.Minute => firstDate.AddMinutes(MaxBars),
                SaxoUnitTimeIntervalType.Hour => firstDate.AddHours(MaxBars),
                SaxoUnitTimeIntervalType.Daily => firstDate.AddDays(MaxBars),
                _ => throw new NotSupportedException($"{nameof(SaxoAPIClient)}.{nameof(GetBars)}: Unsupported time interval type '{unitOfTime}'")
            };

            if (newLastDate > lastDate)
            {
                newLastDate = lastDate;
            }

            if (totalRequestAmount < 1)
            {
                count = (int)remainingCount;
            }

            await foreach (var bar in GetBarsAsync(assetType, uic, unitOfTime, newLastDate, count, "UpTo"))
            {
                yield return bar;
            }

            firstDate = newLastDate;

        } while (--totalRequestAmount >= 0);
    }

    private async IAsyncEnumerable<SaxoChartSample> GetBarsAsync(SaxoAssetType assetType, int uic, SaxoUnitTimeIntervalType unitOfTime, DateTime time = new DateTime(), int count = 1200, string mode = "From")
    {
        int horizon = 1;
        if (unitOfTime == SaxoUnitTimeIntervalType.Hour)
        {
            horizon = 60;
        }
        else if (unitOfTime == SaxoUnitTimeIntervalType.Daily)
        {
            horizon = 1440;
        }

        var url = new StringBuilder($"/chart/v3/charts?AssetType={assetType}&Count={count}&Horizon={horizon}&Mode={mode}&Time={time.ToString("yyyy-MM-ddTH:mm:ss")}&Uic={uic}");
       
        var bars = default(IEnumerable<SaxoChartSample>);
        try
        {
            bars = (await RequestAsync<SaxoBars>(_baseUrl, url.ToString(), HttpMethod.Get)).Data;
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(SaxoAPIClient)}.{nameof(GetBarsAsync)}: Failed to retrieve bars for Uic '{uic}'. Exception: {ex.Message}");
            yield break;
        }

        foreach (var bar in bars)
        {
            yield return bar;
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