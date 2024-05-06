using Reddit.Tracker.Api.Dto;

namespace Reddit.Tracker.Api
{
    public interface IRedditAuthClient
    {
        Task<string> GenerateToken();
    }
}
