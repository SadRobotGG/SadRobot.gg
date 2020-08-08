using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SadRobot.Core.Apis.BlizzardApi
{
    class BlizzardApiAuthHandler : DelegatingHandler
    {
        string token;
        DateTime expiry;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (token == null || expiry <= DateTime.Now)
            {
                using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://us.battle.net/oauth/token");
                
                var clientId = Environment.GetEnvironmentVariable("BNET_CLIENTID");
                var clientSecret = Environment.GetEnvironmentVariable("BNET_SECRET");

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)) throw new InvalidOperationException("OAuth credentials not found");

                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes(clientId + ":" + clientSecret));
                tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                tokenRequest.Content = new FormUrlEncodedContent(new []{new KeyValuePair<string, string>("grant_type", "client_credentials") });
                
                using var response = await HttpClientHelper.Client.SendAsync(tokenRequest, cancellationToken);
                response.EnsureSuccessStatusCode();
                var tokenString = await response.Content.ReadAsStringAsync();
                var tokenValue = JsonSerializer.Deserialize<TokenValue>( tokenString );
                token = tokenValue.AccessToken;
                expiry = DateTime.Now.AddSeconds((int) tokenValue.ExpiresIn);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }

        class TokenValue
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }
    }
}
