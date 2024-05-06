using Microsoft.Extensions.Options;
using Polly;
using Reddit.Tracker.Api;
using Reddit.Tracker.Api.Model;
using Reddit.Tracker.App;
using Reddit.Tracker.App.Manager;
using Reddit.Tracker.Repository;
using Reddit.Tracker.Repository.Model;
using System.Net.Mime;
using System.Reflection;
using System.Text;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IGenericRepository<Post>, MemoryRepository<Post>>();
builder.Services.AddSingleton<IGenericRepository<TopPost>, MemoryRepository<TopPost>>();
builder.Services.AddSingleton<IWorkerManager, WorkerManager>();
builder.Services.Configure<RedditClientConfiguration>(builder.Configuration.GetSection("RedditClient"));
builder.Services
    .AddHttpClient<IRedditClient, RedditClient>((sp, c) => ConfigureHttpClient(sp, c))
    .AddTransientHttpErrorPolicy(GetHttpRetryPolicy);
builder.Services
    .AddHttpClient<IRedditAuthClient, RedditAuthClient>((sp, c) => ConfigureAuthHttpClient(sp, c))
    .AddTransientHttpErrorPolicy(GetHttpRetryPolicy);

var host = builder.Build();
host.Run();

static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client)
{
    var clientConfig = serviceProvider.GetService<IOptions<RedditClientConfiguration>>();

    client.DefaultRequestHeaders.Accept.Add(new (MediaTypeNames.Application.Json));
    client.DefaultRequestHeaders.Add("Authorization", $"bearer {clientConfig.Value.AccessToken}");
    client.DefaultRequestHeaders.Add("User-Agent", $"Windows|Reddit.Tracker.App|v{Assembly.GetExecutingAssembly().GetName().Version}");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri(clientConfig.Value.Uri);
}

static void ConfigureAuthHttpClient(IServiceProvider serviceProvider, HttpClient client)
{
    var clientConfig = serviceProvider.GetService<IOptions<RedditClientConfiguration>>();

    client.DefaultRequestHeaders.Accept.Add(new (MediaTypeNames.Application.Json));
    client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientConfig.Value.AppId}:"))}");
    client.DefaultRequestHeaders.Add("User-Agent", $"Windows|Reddit.Tracker.App|v{Assembly.GetExecutingAssembly().GetName().Version}");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri(clientConfig.Value.AuthUri);
}

static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(PolicyBuilder<HttpResponseMessage> policyBuilder)
{
    return policyBuilder
        .WaitAndRetryAsync(3, retryCount => TimeSpan.FromMilliseconds(1000));
}