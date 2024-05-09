using Reddit.Tracker.App.Manager;

namespace Reddit.Tracker.App
{
    public sealed class Worker : IHostedService, IAsyncDisposable
    {
        private readonly Task _completedTask = Task.CompletedTask;
        private readonly ILogger<Worker> _logger;
        private readonly IPostManager _postManager;
        private Timer? _timer;

        public Worker(ILogger<Worker> logger, IPostManager postManager) 
        {
            _logger = logger;
            _postManager = postManager;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Starting...");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

            return _completedTask;
        }

        private void DoWork(object? state)
        {
            _logger.LogInformation($"Thread {Thread.CurrentThread.ManagedThreadId} processing...");

            Task.Run(async () =>
            {
                _logger.LogInformation("Tracking posts...");
                await _postManager.TrackPosts();

                _logger.LogInformation("Getting top Users...");
                var topUsers = await _postManager.GetTopUsers();
                topUsers
                    .AsParallel()
                    .ForAll((user) =>
                {
                    _logger.LogInformation($"{user}");
                });

                _logger.LogInformation("Getting top Posts...");
                var topPosts = await _postManager.GetTopPosts();
                topPosts
                    .AsParallel()
                    .ForAll((post) =>
                {
                    _logger.LogInformation($"{post}");
                });
            });
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping...");

             _timer?.Change(Timeout.Infinite, 0);

            return _completedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer is IAsyncDisposable timer)
            {
                await timer.DisposeAsync();
            }

            _timer = null;
        }
    }
}
