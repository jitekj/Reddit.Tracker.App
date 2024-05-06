using Reddit.Tracker.Api.Dto;

namespace Reddit.Tracker.Api
{
    public interface IRedditClient
    {
        void SetToken(string token);
        Task<SubRedditDto> GetTopPostsByDayAsync();
        Task<SubRedditDto> GetPostsByDayAsync();
    }
}
