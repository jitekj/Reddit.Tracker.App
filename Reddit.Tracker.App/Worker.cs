using Reddit.Tracker.App.Manager;

namespace Reddit.Tracker.App
{
    public sealed class Worker : IHostedService, IAsyncDisposable
    {
        private readonly Task _completedTask = Task.CompletedTask;
        private readonly ILogger<Worker> _logger;
        private readonly IWorkerManager _workerManager;
        private Timer? _timer;

        public Worker(ILogger<Worker> logger, IWorkerManager workerManager) 
        {
            _logger = logger;
            _workerManager = workerManager;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting...", nameof(App));
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

            return _completedTask;
        }

        private void DoWork(object? state)
        {
            //_timer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation($"Thread {Thread.CurrentThread.ManagedThreadId} processing...", nameof(App));

            _workerManager.TrackSubReddit();
            Task.Run(async () =>
            {
                _logger.LogInformation("Getting top Users...");
                var topUsers = await _workerManager.GetTopUsers();
                topUsers
                    .AsParallel()
                    .ForAll((user) =>
                {
                    _logger.LogInformation($"{user}");
                });

                _logger.LogInformation("Getting top Posts...");
                var topPosts = await _workerManager.GetTopPosts();
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
            _logger.LogInformation("{Service} is stopping.", nameof(App));

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