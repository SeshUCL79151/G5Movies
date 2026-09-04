using MoviesG5.Core;
using MoviesG5.Tests.Fakes;
using MoviesG5.UI;
using System.Collections.ObjectModel;

namespace MoviesG5.Tests
{
    [TestClass]
    public class MovieViewModelTests
    {
        private InMemoryRepository<Movie> _repo = new();
        private FakeDialogService _dialogService = new();
        private MovieViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _repo = new InMemoryRepository<Movie>();
            _dialogService = new FakeDialogService();
            var movies = new ObservableCollection<Movie>(Enumerable.Reverse(_repo.GetAll()));
            _viewModel = new MovieViewModel(_repo, movies, _dialogService);
        }

        [TestMethod]
        public void AddMovie_ValidInput_AddsToMoviesCollection()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.MovieDirector = "Christopher Nolan";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(1, _viewModel.Movies.Count);
            Assert.AreEqual("Inception", _viewModel.Movies[0].Title);
            Assert.AreEqual(new TimeSpan(2, 28, 0), _viewModel.Movies[0].Duration);
            Assert.AreEqual(Genre.SciFi, _viewModel.Movies[0].Genre);
        }

        [TestMethod]
        public void AddMovie_ValidInput_SavesToRepository()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.MovieDirector = "Christopher Nolan";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(1, _repo.GetAll().Count);
            Assert.AreEqual(1, _repo.SaveCallCount);
        }

        [TestMethod]
        public void AddMovie_ValidInput_ShowsInfoDialogAndClearsForm()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.MovieDirector = "Christopher Nolan";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(1, _dialogService.InfoMessages.Count);
            Assert.AreEqual(0, _dialogService.ErrorMessages.Count);
            Assert.AreEqual(string.Empty, _viewModel.MovieTitle);
            Assert.AreEqual(string.Empty, _viewModel.MovieDirector);
            Assert.AreEqual(string.Empty, _viewModel.DurationInput);
            Assert.IsNull(_viewModel.SelectedGenre);
        }

        [TestMethod]
        public void AddMovie_MultipleMovies_NewestIsFirstInCollection()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.MovieDirector = "Christopher Nolan";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;
            _viewModel.AddMovieCommand.Execute(null);

            _viewModel.MovieTitle = "Alien";
            _viewModel.MovieDirector = "Ridley Scott";
            _viewModel.DurationInput = "1:57";
            _viewModel.SelectedGenre = Genre.Gyser;
            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual("Alien", _viewModel.Movies[0].Title);
            Assert.AreEqual("Inception", _viewModel.Movies[1].Title);
        }

        [TestMethod]
        public void AddMovie_EmptyTitle_ShowsErrorAndDoesNotAdd()
        {
            _viewModel.MovieTitle = "   ";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(0, _viewModel.Movies.Count);
            Assert.AreEqual(0, _repo.GetAll().Count);
            Assert.AreEqual(1, _dialogService.ErrorMessages.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "titel");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("25:00")]
        [DataRow("1:5")]
        [DataRow("not a duration")]
        public void AddMovie_InvalidDuration_ShowsErrorAndDoesNotAdd(string durationInput)
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.DurationInput = durationInput;
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(0, _viewModel.Movies.Count);
            Assert.AreEqual(1, _dialogService.ErrorMessages.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "Varighed");
        }

        [TestMethod]
        public void AddMovie_NoGenreSelected_ShowsErrorAndDoesNotAdd()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = null;

            _viewModel.AddMovieCommand.Execute(null);

            Assert.AreEqual(0, _viewModel.Movies.Count);
            Assert.AreEqual(1, _dialogService.ErrorMessages.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "genre");
        }

        [TestMethod]
        public void AddMovie_AllFieldsInvalid_CombinesAllErrorMessages()
        {
            _viewModel.MovieTitle = "";
            _viewModel.DurationInput = "";
            _viewModel.SelectedGenre = null;

            _viewModel.AddMovieCommand.Execute(null);

            var message = _dialogService.ErrorMessages[0];
            StringAssert.Contains(message, "titel");
            StringAssert.Contains(message, "Varighed");
            StringAssert.Contains(message, "genre");
        }

        [TestMethod]
        public void ClearForm_ResetsAllFields()
        {
            _viewModel.MovieTitle = "Inception";
            _viewModel.DurationInput = "2:28";
            _viewModel.SelectedGenre = Genre.SciFi;

            _viewModel.ClearFormCommand.Execute(null);

            Assert.AreEqual(string.Empty, _viewModel.MovieTitle);
            Assert.AreEqual(string.Empty, _viewModel.DurationInput);
            Assert.IsNull(_viewModel.SelectedGenre);
        }

        [TestMethod]
        public void Constructor_ExistingMoviesInRepo_ShowsNewestFirst()
        {
            var repo = new InMemoryRepository<Movie>();
            repo.Add(new Movie { Title = "Inception", Genre = Genre.SciFi });
            repo.Add(new Movie { Title = "Alien", Genre = Genre.Gyser });

            var movies = new ObservableCollection<Movie>(Enumerable.Reverse(repo.GetAll()));
            var viewModel = new MovieViewModel(repo, movies, new FakeDialogService());

            Assert.AreEqual("Alien", viewModel.Movies[0].Title);
            Assert.AreEqual("Inception", viewModel.Movies[1].Title);
        }
    }
}
