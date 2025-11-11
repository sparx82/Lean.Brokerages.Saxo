using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace QuantConnect.Brokerages.Saxo.API;

public class SaxoOAuthConfig
{
    public string AppKey { get; set; }
    public string AppUrl { get; set; }
    public string AuthenticationUrl { get; set; }
    public string OpenApiBaseUrl { get; set; }
    public int RedirectPort { get; set; } = 5000;
}

public class TokenResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; }
    public string refresh_token { get; set; }
    public int refresh_token_expires_in { get; set; }
}

public class SaxoOAuthClient
{
    private readonly SaxoOAuthConfig _config;
    private readonly HttpClient _httpClient;
    public string _codeVerifier { get; set; }
    private string _codeChallenge;
    private string _state;

    public SaxoOAuthClient(SaxoOAuthConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Generates a cryptographically random code verifier (43-128 chars)
    /// </summary>
    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Base64UrlEncode(bytes);
    }

    /// <summary>
    /// Creates SHA256 hash of the code verifier for PKCE
    /// </summary>
    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(hash);
        }
    }

    /// <summary>
    /// Base64 URL encoding without padding
    /// </summary>
    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Generates a random state parameter
    /// </summary>
    private string GenerateState()
    {
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Step 1: Build the authorization URL and return it
    /// </summary>
    public string GetAuthorizationUrl()
    {
        _codeVerifier = GenerateCodeVerifier();
        _codeChallenge = GenerateCodeChallenge(_codeVerifier);
        _state = GenerateState();

        var redirectUri = $"{_config.AppUrl}:{_config.RedirectPort}";

        var queryParams = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", _config.AppKey },
                { "state", _state },
                { "redirect_uri", redirectUri },
                { "code_challenge", _codeChallenge },
                { "code_challenge_method", "S256" }
            };

        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{_config.AuthenticationUrl}/authorize?{queryString}";
    }

    /// <summary>
    /// Step 2: Start a local HTTP listener to capture the redirect
    /// </summary>
    public async Task<string> ListenForAuthorizationCodeAsync()
    {
        var redirectUri = $"{_config.AppUrl}:{_config.RedirectPort}/";
        var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);

        try
        {
            listener.Start();
            //Console.WriteLine($"Listening for authorization callback on {redirectUri}");

            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            // Extract code and state from query string
            var query = HttpUtility.ParseQueryString(request.Url.Query);
            var code = query["code"];
            var returnedState = query["state"];

            // Verify state matches
            if (returnedState != _state)
            {
                throw new InvalidOperationException("State parameter mismatch. Possible CSRF attack.");
            }

            // Send success response to browser
            var responseString = "<html><body><h1>Authentication successful!</h1><p>You can close this window.</p></body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();

            return code;
        }
        finally
        {
            listener.Stop();
        }
    }

    /// <summary>
    /// Step 3: Exchange authorization code for access token
    /// </summary>
    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        var redirectUri = $"{_config.AppUrl}:{_config.RedirectPort}";
        var tokenUrl = $"{_config.AuthenticationUrl}/token";

        var requestData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _config.AppKey },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri },
                { "code_verifier", _codeVerifier }
            };

        var content = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync(tokenUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token exchange failed: {response.StatusCode} - {responseContent}");
        }

        return JsonSerializer.Deserialize<TokenResponse>(responseContent);
    }

    /// <summary>
    /// Step 4: Refresh the access token using refresh token
    /// </summary>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenUrl = $"{_config.AuthenticationUrl}/token";

        var requestData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "code_verifier", _codeVerifier }
            };

        var content = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync(tokenUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token refresh failed: {response.StatusCode} - {responseContent}");
        }

        return JsonSerializer.Deserialize<TokenResponse>(responseContent);
    }

    /// <summary>
    /// Complete authentication flow
    /// </summary>
    public async Task<string> GetAuthCode()
    {
        // Step 1: Get authorization URL
        var authUrl = GetAuthorizationUrl();

        //Console.WriteLine("Please navigate to the following URL to authenticate:");
        //Console.WriteLine(authUrl);
        //Console.WriteLine();

        // Open browser automatically (optional)
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });

        // Step 2: Listen for callback
        var authCode = await ListenForAuthorizationCodeAsync();
        //Console.WriteLine($"Authorization code received: {authCode}");

        return authCode;
    }

    /// <summary>
    /// Complete authentication flow
    /// </summary>
    public async Task<TokenResponse> AuthenticateAsync()
    {
        // Step 1: Get authorization URL
        var authUrl = GetAuthorizationUrl();

        //Console.WriteLine("Please navigate to the following URL to authenticate:");
        //Console.WriteLine(authUrl);
        //Console.WriteLine();

        // Open browser automatically (optional)
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });

        // Step 2: Listen for callback
        var authCode = await ListenForAuthorizationCodeAsync();
        //Console.WriteLine($"Authorization code received: {authCode}");

        // Step 3: Exchange code for token
        var tokenResponse = await ExchangeCodeForTokenAsync(authCode);
        //Console.WriteLine("Access token obtained successfully!");

        return tokenResponse;
    }
}