using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reddit.Tracker.Api.Dto;
using Reddit.Tracker.Api.Model;
using System.Collections.Immutable;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace Reddit.Tracker.Api
{
    /// <summary>
    /// Client to manage authentication and token refresh for application
    /// </summary>
    public class RedditAuthClient : IRedditAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<RedditClientConfiguration> _redditClientConfiguration;
        public RedditAuthClient(HttpClient httpClient, IOptions<RedditClientConfiguration> redditClientConfiguration) 
        { 
            _httpClient = httpClient;
            _redditClientConfiguration = redditClientConfiguration;
        }

        public async Task<string> GenerateToken()
        {
            string accessToken = string.Empty;
            string tokenUri = $"/api/v1/access_token";
            var authParams = new Dictionary<string, string>();
            authParams.Add("grant_type", "refresh_token");
            authParams.Add("refresh_token", _redditClientConfiguration.Value.RefreshToken);
            var tokenResponse = await _httpClient.PostAsync(tokenUri, new FormUrlEncodedContent(authParams));

            if (tokenResponse != null && tokenResponse.IsSuccessStatusCode)
            {
                accessToken = JsonConvert.DeserializeObject<JObject>(tokenResponse.Content.ReadAsStringAsync().Result)
                    .GetValue("access_token").ToString();
            }
            else
            {
                throw new ApplicationException(tokenResponse.ReasonPhrase);
            }
            return accessToken;
        }
    }
}
