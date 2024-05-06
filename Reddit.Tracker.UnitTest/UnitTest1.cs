using Reddit.Tracker.Repository;
using Reddit.Tracker.Repository.Model;

namespace Reddit.Tracker.UnitTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        private MemoryRepository<Post> _postRepository;

        [SetUp]
        public void SetUp()
        {
            _postRepository = new MemoryRepository<Post>();
        }

        [Test]
        public async Task AddItem()
        {
            await _postRepository.Add("key", new Post()
            {
                Author = "Author",
                Title = "Title",
                UpVotes = 1000
            });
            var result = await _postRepository.GetAll();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Count, Is.EqualTo(1), "Result should have one element");
        }

        [Test]
        public async Task UpdateItem()
        {
            await _postRepository.Update("key", new Post()
            {
                Author = "Author",
                Title = "Title",
                UpVotes = 1000
            });
            await _postRepository.Update("key", new Post()
            {
                Author = "AuthorNew",
                Title = "TitleNew",
                UpVotes = 2000
            });

            var result = await _postRepository.GetAll();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Count, Is.EqualTo(1), "Result should have one element");
            Assert.That(result.First().Value.Key, Is.EqualTo("AuthorNew::TitleNew"), "Result should have updated item");
        }

        [Test]
        public async Task DeleteItem()
        {
            await _postRepository.Add("key1", new Post()
            {
                Author = "Author1",
                Title = "Title1",
                UpVotes = 1000
            });
            await _postRepository.Add("key2", new Post()
            {
                Author = "Author2",
                Title = "Title2",
                UpVotes = 2000
            });

            await _postRepository.Delete("key1");

            var result = await _postRepository.GetAll();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Count, Is.EqualTo(1), "Result should have one element");
            Assert.That(result.First().Value.Key, Is.EqualTo("Author2::Title2"), "Result should have item with key2");
        }

        [Test]
        public async Task GetAllItems()
        {
            await _postRepository.Add("key1", new Post()
            {
                Author = "Author1",
                Title = "Title1",
                UpVotes = 1000
            });
            await _postRepository.Add("key2", new Post()
            {
                Author = "Author2",
                Title = "Title2",
                UpVotes = 2000
            });

            var result = await _postRepository.GetAll();

            Assert.That(result, Is.Not.Null, "Result should not be null");
            Assert.That(result.Count, Is.EqualTo(1), "Result should have 2 elements");
        }
    }
}
