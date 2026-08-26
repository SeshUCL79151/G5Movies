using System.IO;
using MoviesG5.Core;

namespace MoviesG5.Tests
{
    [TestClass]
    public class RepositoryJsonTests
    {
        private string _filePath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _filePath = Path.Combine(Path.GetTempPath(), $"movies_{Guid.NewGuid()}.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        [TestMethod]
        public void Constructor_FileDoesNotExist_StartsEmpty()
        {
            var repo = new RepositoryJson<Movie>(_filePath);

            Assert.AreEqual(0, repo.GetAll().Count);
        }

        [TestMethod]
        public void Constructor_MalformedJson_StartsEmpty()
        {
            File.WriteAllText(_filePath, "{ not valid json ]");

            var repo = new RepositoryJson<Movie>(_filePath);

            Assert.AreEqual(0, repo.GetAll().Count);
        }

        [TestMethod]
        public void Add_FirstItem_AssignsIdOne()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            var movie = new Movie { Title = "Inception", Genre = Genre.SciFi };

            repo.Add(movie);

            Assert.AreEqual(1, movie.Id);
        }

        [TestMethod]
        public void Add_SecondItem_AssignsNextId()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            repo.Add(new Movie { Title = "Inception", Genre = Genre.SciFi });
            var second = new Movie { Title = "Alien", Genre = Genre.Gyser };

            repo.Add(second);

            Assert.AreEqual(2, second.Id);
        }

        [TestMethod]
        public void GetById_ExistingId_ReturnsItem()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            var movie = new Movie { Title = "Inception", Genre = Genre.SciFi };
            repo.Add(movie);

            var result = repo.GetById(movie.Id);

            Assert.AreEqual(movie, result);
        }

        [TestMethod]
        public void GetById_MissingId_ReturnsNull()
        {
            var repo = new RepositoryJson<Movie>(_filePath);

            var result = repo.GetById(42);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Update_ExistingItem_ReplacesIt()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            var movie = new Movie { Title = "Inception", Genre = Genre.SciFi };
            repo.Add(movie);
            var updated = new Movie { Id = movie.Id, Title = "Inception 2", Genre = Genre.Action };

            repo.Update(updated);

            Assert.AreEqual("Inception 2", repo.GetById(movie.Id)!.Title);
        }

        [TestMethod]
        public void Update_MissingItem_DoesNothing()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            var ghost = new Movie { Id = 99, Title = "Ghost", Genre = Genre.Fantasy };

            repo.Update(ghost);

            Assert.AreEqual(0, repo.GetAll().Count);
        }

        [TestMethod]
        public void Remove_ExistingItem_DeletesIt()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            var movie = new Movie { Title = "Inception", Genre = Genre.SciFi };
            repo.Add(movie);

            repo.Remove(movie.Id);

            Assert.AreEqual(0, repo.GetAll().Count);
        }

        [TestMethod]
        public void Remove_MissingItem_DoesNothing()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            repo.Add(new Movie { Title = "Inception", Genre = Genre.SciFi });

            repo.Remove(99);

            Assert.AreEqual(1, repo.GetAll().Count);
        }

        [TestMethod]
        public void Save_ThenReload_PersistsItems()
        {
            var repo = new RepositoryJson<Movie>(_filePath);
            repo.Add(new Movie { Title = "Inception", Duration = new TimeSpan(2, 28, 0), Genre = Genre.SciFi });
            repo.Save();

            var reloaded = new RepositoryJson<Movie>(_filePath);

            Assert.AreEqual(1, reloaded.GetAll().Count);
            Assert.AreEqual("Inception", reloaded.GetAll()[0].Title);
            Assert.AreEqual(new TimeSpan(2, 28, 0), reloaded.GetAll()[0].Duration);
        }
    }
}
