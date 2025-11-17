using Newtonsoft.Json;
using QuantConnect.Brokerages.Saxo.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo.API;

public class TokenRefreshHandler : DelegatingHandler
{
    private int _retryCount = 0;

    private int _maxRetryCount = 3;

    private TimeSpan _retryInverval = TimeSpan.FromSeconds(2);

    private readonly string _baseSignInUrl;

    private readonly string _clientId;

    private readonly string _clientSecret;

    private readonly string _authorizationCodeFromUrl;

    private readonly string _redirectUri;

    private SaxoAccessToken _saxoAccessToken;

    private string _refreshToken;

    public TokenRefreshHandler(HttpMessageHandler innerHandler, string clientId, string clientSecret, string authorizationCodeFromUrl, string baseSignInUrl, string redirectUri, string refreshToken) : base(innerHandler)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _redirectUri = redirectUri;
        _authorizationCodeFromUrl = authorizationCodeFromUrl;
        _baseSignInUrl = baseSignInUrl;
        _refreshToken = refreshToken;
    }

    public TokenRefreshHandler(HttpMessageHandler innerHandler, string accessToken) : base(innerHandler)
    {
        _saxoAccessToken = new SaxoAccessToken(accessToken, "", "", "", 0, "Bearer", "");
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = null;

        for (_retryCount = 0; _retryCount < _maxRetryCount; _retryCount++)
        {
            if(_saxoAccessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(_saxoAccessToken.TokenType, _saxoAccessToken.AccessToken);
            }

            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    break;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    if (_saxoAccessToken == null && string.IsNullOrEmpty(_refreshToken))
                    {
                        _saxoAccessToken = await GetAuthenticationToken(cancellationToken);
                        _refreshToken = _saxoAccessToken.RefreshToken;
                    }
                    else
                    {
                        _saxoAccessToken = await RefreshAccessToken(_refreshToken, cancellationToken);
                    }
                }
                else
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{nameof(TokenRefreshHandler)}.{nameof(SendAsync)}.{nameof(TaskCanceledException)}: {ex}. " + $"Request: {request.Method} {request.RequestUri}, attempt {_retryCount + 1}/{_maxRetryCount}.");

                if(cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
            }

            await Task.Delay(_retryInverval, cancellationToken);
        }

        return response;
    }

    private async Task<SaxoAccessToken> GetAuthenticationToken(CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", _clientId },
            { "code_verifier", _clientSecret },
            { "code", _authorizationCodeFromUrl },
            { "redirect_uri", $"{_redirectUri}:5000" },
        };

        var response = await SendSignInAsync(new FormUrlEncodedContent(parameters), cancellationToken);

        return JsonConvert.DeserializeObject<SaxoAccessToken>(response);
    }

    private async Task<SaxoAccessToken> RefreshAccessToken(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException($"{nameof(SaxoAPIClient)}.{nameof(RefreshAccessToken)}:" + $"The refresh token provided is null or empty. Please ensure a valid refresh token is provided.");
        }

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "refresh_toekn" },
            { "client_id", _clientId },
            { "refresh_token", refreshToken }
        };

        if (!string.IsNullOrEmpty(_clientSecret))
        {
            parameters["client_secret"] = _clientSecret;
        }

        var response = await SendSignInAsync(new FormUrlEncodedContent(parameters), cancellationToken);

        return JsonConvert.DeserializeObject<SaxoAccessToken>(response);
    }

    private async Task<string> SendSignInAsync(FormUrlEncodedContent content, CancellationToken cancellationToken = default)
    {
        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseSignInUrl}/token"))
        {
            requestMessage.Content = content;

            try
            {
                var responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                responseMessage.EnsureSuccessStatusCode();

                return await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logging.Log.Error($"{nameof(TokenRefreshHandler)}.{nameof(SendSignInAsync)} failed. Request: [{requestMessage.Method}] {requestMessage.RequestUri}. " +
                    $"IsCancellationRequested = {cancellationToken.IsCancellationRequested}, ExceptionType: {ex.GetType().Name}, Message: {ex}");
                throw;
            }
        }
    }
}
