using MoviesG5.Core;
using MoviesG5.Tests.Fakes;
using MoviesG5.UI;

namespace MoviesG5.Tests
{
    [TestClass]
    public class ReservationViewModelTests
    {
        private InMemoryRepository<Cinema> _cinemaRepo = new();
        private FakeDialogService _dialogService = new();
        private ReservationViewModel _viewModel = null!;
        private Movie _movie = null!;
        private Cinema _cinema = null!;
        private Screen _screen = null!;

        [TestInitialize]
        public void Setup()
        {
            _cinemaRepo = new InMemoryRepository<Cinema>();
            _dialogService = new FakeDialogService();

            _movie = new Movie { Title = "Inception", Director = "Christopher Nolan", Duration = new TimeSpan(2, 28, 0), Genre = Genre.SciFi };

            _screen = new Screen { Id = 1, Name = 1, Capacity = 100 };
            _cinema = new Cinema { Name = "Hjerm biograf", Screens = new List<Screen> { _screen } };
            _cinemaRepo.Add(_cinema);

            _viewModel = new ReservationViewModel(_cinemaRepo, _dialogService);
        }

        private static DateTime NextMonthFirstDay()
        {
            var next = DateTime.Now.AddMonths(1);
            return new DateTime(next.Year, next.Month, 1);
        }

        private Screening AddScreening(DateOnly date, TimeOnly time)
        {
            var screening = new Screening { Date = date, StartTime = time, Movie = _movie, Capacity = _screen.Capacity };
            _screen.Screenings.Add(screening);
            return screening;
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
            Assert.IsTrue(_viewModel.PlannedScreenings.All(item => !item.Slot1.HasScreening && !item.Slot2.HasScreening && !item.Slot3.HasScreening));
            Assert.IsTrue(_viewModel.PlannedScreenings.All(item => item.Slot1.DisplayText == "Ledig"));
        }

        [TestMethod]
        public void SelectedScreen_Selected_PlacesScreeningInCorrectDaySlot()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));

            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;

            var dayItem = _viewModel.PlannedScreenings.First(d => d.DateText == start.Day.ToString());
            Assert.IsTrue(dayItem.Slot1.HasScreening);
            Assert.AreSame(screening, dayItem.Slot1.Screening);
            StringAssert.Contains(dayItem.Slot1.DisplayText, "Inception");
        }

        [TestMethod]
        public void ScreeningButtonClickCommand_ValidScreening_SetsSelectedScreeningAndText()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;

            _viewModel.ScreeningButtonClickCommand.Execute(screening);

            Assert.AreSame(screening, _viewModel.SelectedScreening);
            StringAssert.Contains(_viewModel.SelectedScreeningText, "Inception");
            StringAssert.Contains(_viewModel.SelectedScreeningText, date.ToString("dd/MM/yyyy"));
        }

        [TestMethod]
        public void ScreeningButtonClickCommand_NullParameter_DoesNothing()
        {
            _viewModel.ScreeningButtonClickCommand.Execute(null);

            Assert.IsNull(_viewModel.SelectedScreening);
        }

        [TestMethod]
        public void ReserveBooking_NoFieldsSet_ShowsAllErrorsAndDoesNotAdd()
        {
            _viewModel.ReserveCommand.Execute(null);

            var message = _dialogService.ErrorMessages[0];
            StringAssert.Contains(message, "forestilling");
            StringAssert.Contains(message, "email");
            StringAssert.Contains(message, "telefonnummer");
            StringAssert.Contains(message, "billetter");
        }

        [TestMethod]
        public void ReserveBooking_ValidInput_AddsBookingAndSaves()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "2";

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(1, screening.Bookings.Count);
            Assert.AreEqual(2, screening.Bookings[0].TicketCount);
            Assert.AreEqual("test@test.dk", screening.Bookings[0].Customer.Email);
            Assert.AreEqual(1, _cinemaRepo.SaveCallCount);
            Assert.AreEqual(1, _dialogService.InfoMessages.Count);
        }

        [TestMethod]
        public void ReserveBooking_ValidInput_ClearsFormAndRefreshesPlannedScreenings()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "2";

            _viewModel.ReserveCommand.Execute(null);

            Assert.IsNull(_viewModel.SelectedScreening);
            Assert.AreEqual(string.Empty, _viewModel.SelectedScreeningText);
            Assert.AreEqual(string.Empty, _viewModel.CustomerEmail);
            Assert.AreEqual(string.Empty, _viewModel.CustomerPhone);
            Assert.AreEqual(string.Empty, _viewModel.TicketCount);

            var dayItem = _viewModel.PlannedScreenings.First(d => d.DateText == start.Day.ToString());
            StringAssert.Contains(dayItem.Slot1.DisplayText, "2/100");
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("-1")]
        [DataRow("abc")]
        [DataRow("")]
        public void ReserveBooking_InvalidTicketCount_ShowsErrorAndDoesNotAdd(string ticketCount)
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = ticketCount;

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(0, screening.Bookings.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "billetter");
        }

        [TestMethod]
        [DataRow("not-an-email")]
        [DataRow("test@test")]
        [DataRow("@test.dk")]
        [DataRow("test.dk")]
        public void ReserveBooking_InvalidEmail_ShowsErrorAndDoesNotAdd(string email)
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = email;
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "2";

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(0, screening.Bookings.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "email");
        }

        [TestMethod]
        [DataRow("1234")]
        [DataRow("123456789")]
        [DataRow("1234567a")]
        [DataRow("")]
        public void ReserveBooking_InvalidPhone_ShowsErrorAndDoesNotAdd(string phone)
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = phone;
            _viewModel.TicketCount = "2";

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(0, screening.Bookings.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "telefonnummer");
        }

        [TestMethod]
        public void ReserveBooking_TicketCountExceedsCapacity_ShowsErrorAndDoesNotAdd()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            _screen.Capacity = 5;
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "6";

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(0, screening.Bookings.Count);
            StringAssert.Contains(_dialogService.ErrorMessages[0], "ledige pladser");
        }

        [TestMethod]
        public void ReserveBooking_ExactRemainingCapacity_Succeeds()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            _screen.Capacity = 5;
            var screening = AddScreening(date, new TimeOnly(18, 30));
            screening.Capacity = 5;
            screening.Bookings.Add(new Booking { TicketCount = 3, Customer = new Customer { Email = "a@a.dk", Phone = "1" } });
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "2"; // exactly the 2 remaining seats

            _viewModel.ReserveCommand.Execute(null);

            Assert.AreEqual(2, screening.Bookings.Count);
        }

        [TestMethod]
        public void ClearForm_ResetsAllFields()
        {
            var start = NextMonthFirstDay();
            var date = DateOnly.FromDateTime(start);
            var screening = AddScreening(date, new TimeOnly(18, 30));
            _viewModel.ScreeningButtonClickCommand.Execute(screening);
            _viewModel.CustomerEmail = "test@test.dk";
            _viewModel.CustomerPhone = "12345678";
            _viewModel.TicketCount = "2";

            _viewModel.ClearFormCommand.Execute(null);

            Assert.IsNull(_viewModel.SelectedScreening);
            Assert.AreEqual(string.Empty, _viewModel.SelectedScreeningText);
            Assert.AreEqual(string.Empty, _viewModel.CustomerEmail);
            Assert.AreEqual(string.Empty, _viewModel.CustomerPhone);
            Assert.AreEqual(string.Empty, _viewModel.TicketCount);
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

        [TestMethod]
        public void NextMonth_ScreenSelected_RefreshesPlannedScreeningsForNewMonth()
        {
            _viewModel.SelectedCinema = _cinema;
            _viewModel.SelectedScreen = _screen;

            _viewModel.NextMonthCommand.Execute(null);

            var followingMonth = NextMonthFirstDay().AddMonths(1);
            int daysInFollowingMonth = DateTime.DaysInMonth(followingMonth.Year, followingMonth.Month);
            Assert.AreEqual(daysInFollowingMonth, _viewModel.PlannedScreenings.Count);
        }
    }
}
