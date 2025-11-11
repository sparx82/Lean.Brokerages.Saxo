using System;
using Newtonsoft.Json;

namespace QuantConnect.Brokerages.Saxo.Models;

public class SaxoAccessToken
{
    [JsonProperty("access_token")]
    public string AccessToken { get; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; }

    [JsonProperty("id_token")]
    public string IdToken { get; }

    [JsonProperty("scope")]
    public string Scope {  get; }

    [JsonProperty("expores_in")]
    public int ExpiresIn { get; }

    [JsonProperty("token_type")]
    public string TokenType { get; }

    [JsonProperty("code_verifier")]
    public string CodeVerifier { get; set; }

    public SaxoAccessToken(string accessToken, string refreshToken, string idToken, string scope, int expiresIn, string tokenType, string codeVerifier)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IdToken = idToken;
        Scope = scope;
        ExpiresIn = expiresIn;
        TokenType = tokenType;
        CodeVerifier = codeVerifier;
    }
}
