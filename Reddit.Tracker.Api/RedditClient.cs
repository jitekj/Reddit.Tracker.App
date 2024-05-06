using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Reddit.Tracker.Api.Dto;
using Reddit.Tracker.Api.Model;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Reddit.Tracker.Api
{
    /// <summary>
    /// Client to manage call to Reddit Api and handle Rate limiting.
    /// Using rules from https://github.com/reddit-archive/reddit/wiki/API#rules
    /// </summary>
    public class RedditClient : IRedditClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<RedditClientConfiguration> _redditClientConfiguration;
        private static readonly ConcurrentDictionary<DateTime, int> _callQueue = new ConcurrentDictionary<DateTime, int>();
        private string _subRedditName = string.Empty;
        public RedditClient(HttpClient httpClient, IOptions<RedditClientConfiguration> redditClientConfiguration) 
        { 
            _httpClient = httpClient;
            _redditClientConfiguration = redditClientConfiguration;
            _subRedditName = !string.IsNullOrWhiteSpace(_redditClientConfiguration.Value.SubReddit) ? 
                _redditClientConfiguration.Value.SubReddit
                : "all";
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
        }

        /// <summary>
        /// To get the top posts by day under a SubReddit
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RateLimitException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<SubRedditDto> GetTopPostsByDayAsync()
        {
            var subRedditDto = new SubRedditDto();
            string subRedditUri = $"r/{_subRedditName}/top/?t=day&show=all&sr_detail=false&after=&before=&limit=100&raw_json=1&count=0";
            if (!CanBeQueued())
            {
                throw new RateLimitException();
            }
            _callQueue.TryAdd(DateTime.Now, Thread.CurrentThread.ManagedThreadId);
            var response = await _httpClient.GetAsync(subRedditUri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                subRedditDto = JsonConvert.DeserializeObject<SubRedditDto>(content) ?? new SubRedditDto();
                CheckRateLimit(response);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            return subRedditDto;
        }

        /// <summary>
        /// To get all posts for the day under a SubReddit
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RateLimitException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<SubRedditDto> GetPostsByDayAsync()
        {
            var subRedditDto = new SubRedditDto();
            string subRedditUri = $"r/{_subRedditName}/?t=day&show=all&sr_detail=false&after=&before=&limit=100&raw_json=1&count=0";
            if (!CanBeQueued())
            {
                throw new RateLimitException();
            }
            _callQueue.TryAdd(DateTime.Now, Thread.CurrentThread.ManagedThreadId);
            var response = await _httpClient.GetAsync(subRedditUri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                subRedditDto = JsonConvert.DeserializeObject<SubRedditDto>(content) ?? new SubRedditDto();
                CheckRateLimit(response);
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            return subRedditDto;
        }

        private bool CanBeQueued()
        {
            //60 calls per minute
            int maxRequests = 60;
            //Remove expired calls
            foreach(var call in _callQueue)
            {
                if (call.Key.AddMinutes(1) < DateTime.Now)
                {
                    _callQueue.Remove(call.Key, out int val);
                }
            }

            return _callQueue.Count < maxRequests;
        }

        private void CheckRateLimit(HttpResponseMessage response)
        {
            IEnumerable<string>? headerValues = null;
            response.Headers.TryGetValues("X-Ratelimit-Remaining", out headerValues);
            var rateLimitRemaining = headerValues?.FirstOrDefault()?.ToString();
            if (rateLimitRemaining == "0")
            {
                throw new RateLimitException();
            }
        }
    }
}
