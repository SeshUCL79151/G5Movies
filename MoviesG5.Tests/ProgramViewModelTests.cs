using MoviesG5.Core;
using MoviesG5.Tests.Fakes;
using MoviesG5.UI;
using System.Collections.ObjectModel;

namespace MoviesG5.Tests
{
    [TestClass]
    public class ProgramViewModelTests
    {
        private InMemoryRepository<Movie> _movieRepo = new();
        private InMemoryRepository<Cinema> _cinemaRepo = new();
        private FakeDialogService _dialogService = new();
        private ProgramViewModel _viewModel = null!;
        private Movie _movie = null!;
        private Cinema _cinema = null!;
        private Screen _screen = null!;

        [TestInitialize]
        public void Setup()
        {
            _movieRepo = new InMemoryRepository<Movie>();
            _cinemaRepo = new InMemoryRepository<Cinema>();
            _dialogService = new FakeDialogService();

            _movie = new Movie { Title = "Inception", Director = "Christopher Nolan", Duration = new TimeSpan(2, 28, 0), Genre = Genre.SciFi };
            _movieRepo.Add(_movie);

            _screen = new Screen { Id = 1, Name = 1, Capacity = 100 };
            _cinema = new Cinema { Name = "Hjerm biograf", Screens = new List<Screen> { _screen } };
            _cinemaRepo.Add(_cinema);

            var movies = new ObservableCollection<Movie>(_movieRepo.GetAll());
            _viewModel = new ProgramViewModel(_movieRepo, _cinemaRepo, movies, _dialogService);
        }

        private static DateTime NextMonthFirstDay()
        {
            var next = DateTime.Now.AddMonths(1);
            return new DateTime(next.Year, next.Month, 1);
        }

        [TestMethod]
        public void Constructor_DefaultsStartAndEndDateToFirstOfNextMonth()
        {
            Assert.AreEqual(NextMonthFirstDay(), _viewModel.StartDate!.Value);
            Assert.AreEqual(NextMonthFirstDay(), _viewModel.EndDate!.Value);
        }

        [TestMethod]
        public void Constructor_NoScreenSelected_PlannedScreeningsIsEmpty()
        {
            Assert.AreEqual(0, _viewModel.PlannedScreenings.Count);
        }

        [TestMethod]
        public void SelectedCinema_Changed_ResetsSelectedScreen()
        {
            _viewModel.SelectedScreen = _screen;

            _viewModel.SelectedCinema = _cinema;

            Assert.IsNull(_viewModel.SelectedScreen);
        }

        [TestMethod]
        public void SelectedScreen_Selected_PopulatesPlannedScreeningsWithLedigeSlots()
        {
            _viewModel.SelectedCinema = _cinema;

            _viewModel.SelectedScreen = _screen;

            var next = NextMonthFirstDay();
            int daysInMonth = DateTime.DaysInMonth(next.Year, next.Month);
            Assert.AreEqual(daysInMonth, _viewModel.PlannedScreenings.Count);
            Assert.IsTrue(_viewModel.PlannedScreenings.All(item => item.ScreeningText.Count == 3));
            Assert.IsTrue(_viewModel.PlannedScreenings.All(item => item.ScreeningText.All(t => t == "Ledig")));
        }

        [TestMethod]
        public void AddScreenings_MissingFields_ShowsErrorAndDoesNotAdd()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            // SelectedMovie and StartTimeInput left unset

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.AreEqual(1, _dialogService.ErrorMessages.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "film");
            Assert.AreEqual(0, _screen.Screenings.Count);
        }

        [TestMethod]
        public void AddScreenings_ValidSingleDay_AddsScreeningAndSaves()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.SelectedMovie = _movie;
            var start = NextMonthFirstDay();
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30";

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.AreEqual(1, _screen.Screenings.Count);
            Assert.AreEqual(_movie.Id, _screen.Screenings[0].Movie.Id);
            Assert.AreEqual(new TimeOnly(18, 30), _screen.Screenings[0].StartTime);
            Assert.AreEqual(1, _cinemaRepo.SaveCallCount);
        }

        [TestMethod]
        public void AddScreenings_DateRange_AddsOneScreeningPerDay()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.SelectedMovie = _movie;
            var start = NextMonthFirstDay();
            _viewModel.StartDate = start;
            _viewModel.EndDate = start.AddDays(2);
            _viewModel.StartTimeInput = "18:30";

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.AreEqual(3, _screen.Screenings.Count);
        }

        [TestMethod]
        public void AddScreenings_OverlappingTime_ReportsConflictAndDoesNotAdd()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            var start = NextMonthFirstDay();
            _screen.Screenings.Add(new Screening { Date = DateOnly.FromDateTime(start), StartTime = new TimeOnly(18, 0), Movie = _movie });

            _viewModel.SelectedMovie = _movie;
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30"; // overlaps the existing 18:00 screening (2:28 + 30 min buffer)

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.AreEqual(1, _screen.Screenings.Count);
            Assert.AreEqual(1, _dialogService.ErrorMessages.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "overlappende");
        }

        [TestMethod]
        public void AddScreenings_DayAlreadyHasThreeScreenings_ReportsFullConflict()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            _screen.Screenings.Add(new Screening { Date = date, StartTime = new TimeOnly(6, 0), Movie = _movie });
            _screen.Screenings.Add(new Screening { Date = date, StartTime = new TimeOnly(10, 0), Movie = _movie });
            _screen.Screenings.Add(new Screening { Date = date, StartTime = new TimeOnly(14, 0), Movie = _movie });

            _viewModel.SelectedMovie = _movie;
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "23:00"; // no time overlap, but the day is already full

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.AreEqual(3, _screen.Screenings.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "fyldte");
        }

        [TestMethod]
        public void AddScreenings_Success_ClearsFormAndRefreshesPlannedScreenings()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.SelectedMovie = _movie;
            var start = NextMonthFirstDay();
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30";

            _viewModel.AddScreeningsCommand.Execute(null);

            Assert.IsNull(_viewModel.SelectedMovie);
            Assert.AreEqual(string.Empty, _viewModel.StartTimeInput);
            var dayItem = _viewModel.PlannedScreenings.First(d => d.DateText == start.Day.ToString());
            StringAssert.Contains(dayItem.ScreeningText[0], "Inception");
        }

        [TestMethod]
        public void DeleteScreenings_NoMatch_ShowsInfoMessage()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.SelectedMovie = _movie;
            var start = NextMonthFirstDay();
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30";

            _viewModel.DeleteScreeningsCommand.Execute(null);

            Assert.AreEqual(1, _dialogService.InfoMessages.Count);
            StringAssert.Contains(_dialogService.InfoMessages[0], "Ingen");
        }

        [TestMethod]
        public void DeleteScreenings_MatchingScreening_RemovesAndSaves()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            _screen.Screenings.Add(new Screening { Date = date, StartTime = new TimeOnly(18, 30), Movie = _movie });

            _viewModel.SelectedMovie = _movie;
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30";

            _viewModel.DeleteScreeningsCommand.Execute(null);

            Assert.AreEqual(0, _screen.Screenings.Count);
            Assert.AreEqual(1, _cinemaRepo.SaveCallCount);
            Assert.AreEqual(1, _dialogService.InfoMessages.Count);
            StringAssert.Contains(_dialogService.InfoMessages[0], "Inception");
        }

        [TestMethod]
        public void DeleteScreenings_DifferentStartTime_DoesNotMatch()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            _screen.Screenings.Add(new Screening { Date = date, StartTime = new TimeOnly(20, 0), Movie = _movie });

            _viewModel.SelectedMovie = _movie;
            _viewModel.StartDate = start;
            _viewModel.EndDate = start;
            _viewModel.StartTimeInput = "18:30"; // different from the 20:00 screening above

            _viewModel.DeleteScreeningsCommand.Execute(null);

            Assert.AreEqual(1, _screen.Screenings.Count);
            StringAssert.Contains(_dialogService.InfoMessages[0], "Ingen");
        }

        [TestMethod]
        public void ClearForm_ResetsFieldsToDefaults()
        {
            _viewModel.SelectedMovie = _movie;
            _viewModel.StartTimeInput = "20:00";

            _viewModel.ClearFormCommand.Execute(null);

            Assert.IsNull(_viewModel.SelectedMovie);
            Assert.AreEqual(string.Empty, _viewModel.StartTimeInput);
            Assert.AreEqual(NextMonthFirstDay(), _viewModel.StartDate!.Value);
            Assert.AreEqual(NextMonthFirstDay(), _viewModel.EndDate!.Value);
        }

        [TestMethod]
        public void NextMonth_ChangesMonthName()
        {
            var initialMonthName = _viewModel.MonthName;

            _viewModel.NextMonthCommand.Execute(null);

            Assert.AreNotEqual(initialMonthName, _viewModel.MonthName);
        }

        [TestMethod]
        public void PreviousMonth_ThenNextMonth_ReturnsToOriginalMonthName()
        {
            var initialMonthName = _viewModel.MonthName;

            _viewModel.PreviousMonthCommand.Execute(null);
            _viewModel.NextMonthCommand.Execute(null);

            Assert.AreEqual(initialMonthName, _viewModel.MonthName);
        }
    }
}
