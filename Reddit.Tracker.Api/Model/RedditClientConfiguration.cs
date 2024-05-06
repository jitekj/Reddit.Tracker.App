namespace Reddit.Tracker.Api.Model
{
    /// <summary>
    /// Reddit client configuration
    /// </summary>
    public class RedditClientConfiguration
    {
        public string SubReddit { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string AuthUri { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
