using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Reddit.Tracker.Api;
using Reddit.Tracker.Api.Dto;
using Reddit.Tracker.Api.Model;
using Reddit.Tracker.Repository;
using Reddit.Tracker.Repository.Model;
using System.Net;

namespace Reddit.Tracker.App.UnitTest
{
    public class RedditClientUnitTest
    {
        private RedditClient _redditClient;
        private RedditClient _redditAuthExceptionClient;

        [SetUp]
        public void SetUp()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .Callback(() => Thread.Sleep(100))
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent("{ \"data\": { \"children\": [{ \"data\": { \"title\": \"test title\", \"author\": \"test author\" } }] } }"),
               })
               .Verifiable();

            var handlerAuthExceptionMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerAuthExceptionMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               // prepare the expected response of the mocked http call
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.Unauthorized
               })
               .Verifiable();

            var mockHttp = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://www.reddit.com")
            };
            var mockAuthExceptionHttp = new HttpClient(handlerAuthExceptionMock.Object)
            {
                BaseAddress = new Uri("http://www.reddit.com")
            };

            var mockConfig = Options.Create<RedditClientConfiguration>(
                new RedditClientConfiguration()
                {
                    AccessToken = "AccessToken",
                    AppId = "TestAppId",
                    AuthUri = "http://oauth.reddit.com",
                    RefreshToken = "RefreshToken",
                    SubReddit = "AskReddit",
                    Uri = "http://www.reddit.com"
                });

            _redditClient = new RedditClient(mockHttp, mockConfig);
            _redditAuthExceptionClient = new RedditClient(mockAuthExceptionHttp, mockConfig);
        }

        [Test, Order(1)]
        public async Task GetTopPostsByDayAsync()
        {
            var result = await _redditClient.GetTopPostsByDayAsync();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Data.Children.Count, Is.EqualTo(1), "Result should have one element");
        }

        [Test, Order(2)]
        public async Task GetPostsByDayAsync()
        {
            var result = await _redditClient.GetPostsByDayAsync();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Data.Children.Count, Is.EqualTo(1), "Result should have one element");
        }

        [Test, Order(3)]
        public void GetTopPostsByDayAsync_UnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                 await _redditAuthExceptionClient.GetTopPostsByDayAsync();
            });
        }

        [Test, Order(4)]
        public void GetPostsByDayAsync_UnauthorizedAccessException()
        {
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                 await _redditAuthExceptionClient.GetPostsByDayAsync();
            });
        }

        [Test, Order(5)]
        public void GetPostsByDayAsync_RateLimit()
        {
            Assert.ThrowsAsync<RateLimitException>(async () =>
            {
                for (int i = 0; i < 70; i++)
                {
                    await _redditClient.GetPostsByDayAsync();
                }
            });
        }

        [Test, Order(6)]
        public void GetTopPostsByDayAsync_RateLimit()
        {
            Assert.ThrowsAsync<RateLimitException>(async () =>
            {
                for (int i = 0; i < 70; i++)
                {
                    await _redditClient.GetTopPostsByDayAsync();
                }
            });
        }

        
    }
}