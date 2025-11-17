using QuantConnect.Brokerages.Saxo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo;

internal class SaxoLoadTokenFromFile
{
        /// <summary>
        /// Reads the token.json file, deserializes it, and stores
        /// the access_token in the _saxoAccessToken variable.
        /// </summary>
        /// <param name="filePath">The path to the token.json file</param>
        public async Task<String> LoadTokenFromFileAsync(string filePath)
        {
            try
            {
                // 1. Read the entire file content
                string jsonContent = await File.ReadAllTextAsync(filePath);

                // 2. Deserialize the JSON string into our SaxoToken struct
                SaxoToken token = JsonSerializer.Deserialize<SaxoToken>(jsonContent);

                // 3. Store the access token in the class variable
                return token.AccessToken;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: The file was not found at {filePath}");
            }
            catch (JsonException)
            {
                Console.WriteLine("Error: Could not parse the token.json file. Check format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

        return null;
        }
    }

    /// <summary>
    /// A struct that models the token.json file structure.
    /// [JsonPropertyName] maps the json_snake_case to C# PascalCase.
    /// </summary>
    public struct SaxoToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; }

        [JsonPropertyName("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; }

        [JsonPropertyName("base_uri")]
        public string BaseUri { get; }

        [JsonPropertyName("expires_at")]
        public double ExpiresAt { get; }

        [JsonConstructor]
        public SaxoToken(string accessToken, string tokenType, int expiresIn, string refreshToken, int refreshTokenExpiresIn, string baseUri, double expiresAt)
        {
            AccessToken = accessToken;
            TokenType = tokenType;
            ExpiresIn = expiresIn;
            RefreshToken = refreshToken;
            RefreshTokenExpiresIn = refreshTokenExpiresIn;
            BaseUri = baseUri;
            ExpiresAt = expiresAt;
        }
    }
