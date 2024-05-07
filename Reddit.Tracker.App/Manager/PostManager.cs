using Polly;
using Reddit.Tracker.Api;
using Reddit.Tracker.Api.Dto;
using Reddit.Tracker.Repository;

namespace Reddit.Tracker.App.Manager
{
    /// <summary>
    /// Manager to orchestrate retrieval and presistance of Reddit posts
    /// </summary>
    public class PostManager : IPostManager
    {
        private readonly ILogger<PostManager> _logger;
        private readonly IRedditClient _redditClient;
        private readonly IRedditAuthClient _redditAuthClient;
        private readonly IGenericRepository<Repository.Model.Post> _postRepository;
        private readonly IGenericRepository<Repository.Model.TopPost> _topPostRepository;
        public PostManager(ILogger<PostManager> logger, 
            IRedditClient redditClient, 
            IRedditAuthClient redditAuthClient, 
            IGenericRepository<Repository.Model.Post> postRepository,
            IGenericRepository<Repository.Model.TopPost> topPostRepository) 
        { 
            _logger = logger;
            _redditClient = redditClient;
            _redditAuthClient = redditAuthClient;
            _postRepository = postRepository;
            _topPostRepository = topPostRepository;
        }

        public async Task<IEnumerable<string>> GetTopUsers()
        {
            var posts = await _postRepository.GetAll();
            return posts.Select(s => s.Value)
                .GroupBy(
                s => s.Author, 
                //s => s.Title, 
                (key, g) => new { Author = key, PostCount = g.Count()})
                .OrderByDescending(s => s.PostCount)
                .Select(s => $"Author: {s.Author}, PostCount: {s.PostCount}");
        }

        public async Task<IEnumerable<Repository.Model.TopPost>> GetTopPosts()
        {
            var result = await _topPostRepository.GetAll();
            return result.Select(s => s.Value).OrderByDescending(s => s.UpVotes).ToList();
        }

        public async Task TrackPosts()
        {
            var topPostsByDay = await GetTopPostsByDayAsync();
            var postsByDay = await GetPostsByDayAsync();

            await SaveDailyTopPostsAsync(topPostsByDay);
            await SaveDailyPostsAsync(postsByDay);

            return;
        }

        private async Task<SubRedditDto> GetTopPostsByDayAsync()
        {
            var subRedditDto = default(SubRedditDto);
            int retryCnt = 0;
            do
            {
                try
                {
                    subRedditDto = await _redditClient.GetTopPostsByDayAsync();
                    break;
                }
                catch(UnauthorizedAccessException)
                {
                    await SetToken();
                    retryCnt++;
                }
                catch(RateLimitException)
                {
                    _logger.LogError("Rate limit exceeded");
                    Thread.Sleep(5000);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in GetTopPostsByDayAsync");
                    break;
                }

            } while (retryCnt < 3);

            return subRedditDto ?? new SubRedditDto();
        }

        private async Task SaveDailyTopPostsAsync(SubRedditDto subRedditDto)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (subRedditDto?.Data?.Children == null)
                    {
                        return;
                    }
                    foreach (var postDto in subRedditDto.Data.Children)
                    {
                        Repository.Model.TopPost topPost = new ()
                        {
                            Title = postDto.Data.Title,
                            Author = postDto.Data.Author,
                            UpVotes = postDto.Data.Ups,
                        };

                        _topPostRepository.Update(topPost.Key, topPost);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in SaveDailyTopPostsAsync");
                }
            });
        }

        private async Task<SubRedditDto> GetPostsByDayAsync()
        {
            var subRedditDto = default(SubRedditDto);
            int retryCnt = 0;
            do
            {
                try
                {
                    subRedditDto = await _redditClient.GetPostsByDayAsync();
                    break;
                }
                catch(UnauthorizedAccessException)
                {
                    await SetToken();
                    retryCnt++;
                }
                catch(RateLimitException)
                {
                    _logger.LogError("Rate limit exceeded");
                    Thread.Sleep(5000);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in GetPostsByDayAsync");
                    break;
                }

            } while (retryCnt < 3);

            return subRedditDto ?? new SubRedditDto();
        }

        private async Task SaveDailyPostsAsync(SubRedditDto subRedditDto)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    if (subRedditDto?.Data?.Children == null)
                    {
                        return;
                    }
                    foreach (var postDto in subRedditDto.Data.Children)
                    {
                        Repository.Model.Post post = new ()
                        {
                            Title = postDto.Data.Title,
                            Author = postDto.Data.Author,
                            UpVotes = postDto.Data.Ups,
                        };

                        _postRepository.Update(post.Key, post);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in SaveDailyPostsAsync");
                }
            });
        }

        private async Task SetToken()
        {
            try
            {
                string token = await _redditAuthClient.GenerateToken();
                _redditClient.SetToken(token);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetToken");
            }
        }
    }
}
