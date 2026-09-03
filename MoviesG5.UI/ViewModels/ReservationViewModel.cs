using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MoviesG5.UI
{
    public class ReservationViewModel : ViewModelBase
    {
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Cinema> Cinemas { get; }
        public ObservableCollection<Screening> PlannedScreenings { get; }

        public RelayCommand ReserveCommand { get; }
        public RelayCommand ClearFormCommand { get; }
        public RelayCommand PreviousMonthCommand { get; }
        public RelayCommand NextMonthCommand { get; }

        private Cinema _selectedCinema;
        private Screen _selectedScreen;
        private Screening? _selectedScreening;
        private string _customerEmail;
        private string _customerPhone;
        private string _ticketCount;
        private string _monthName;

        private int MonthNumber { get; set; }
        private int Year { get; set; }

        public string MonthName
        {
            get { return _monthName; }
            set { _monthName = value; OnPropertyChanged(); }
        }

        public Cinema SelectedCinema
        {
            get { return _selectedCinema; }
            set { _selectedCinema = value; OnPropertyChanged(); SelectedScreen = null; }
        }
        public Screen SelectedScreen
        {
            get { return _selectedScreen; }
            set { _selectedScreen = value; OnPropertyChanged(); SelectedScreening = null; RefreshPlannedScreenings(); }
        }
        public Screening? SelectedScreening
        {
            get { return _selectedScreening; }
            set { _selectedScreening = value; OnPropertyChanged(); }
        }
        public string CustomerEmail
        {
            get { return _customerEmail; }
            set { _customerEmail = value; OnPropertyChanged(); }
        }
        public string CustomerPhone
        {
            get { return _customerPhone; }
            set { _customerPhone = value; OnPropertyChanged(); }
        }
        public string TicketCount
        {
            get { return _ticketCount; }
            set { _ticketCount = value; OnPropertyChanged(); }
        }

        public ReservationViewModel(IRepository<Cinema> cinemaRepo, IDialogService dialogService)
        {
            _cinemaRepo = cinemaRepo;
            _dialogService = dialogService;

            Cinemas = new ObservableCollection<Cinema>(_cinemaRepo.GetAll());
            PlannedScreenings = new ObservableCollection<Screening>();

            DateTime date = DateTime.Now;
            MonthNumber = DateOnly.FromDateTime(date).Month + 1; // Nummer på næste måned
            Year = date.Year;
            CheckYearChange();
            SetMonthName(MonthNumber); // Programmet starter med næste måned valgt

            ReserveCommand = new RelayCommand(execute => ReserveBooking(), canexecute => { return true; });
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; });
            PreviousMonthCommand = new RelayCommand(execute => PreviousMonth(), canexecute => { return true; });
            NextMonthCommand = new RelayCommand(execute => NextMonth(), canexecute => { return true; });
        }

        private void ReserveBooking()
        {
            string screeningError = SelectedScreening == null ? "Du skal vælge en forestilling\n" : "";
            string emailError = string.IsNullOrWhiteSpace(CustomerEmail) ? "Du skal indtaste en email\n" : "";
            string phoneError = string.IsNullOrWhiteSpace(CustomerPhone) ? "Du skal indtaste et telefonnummer\n" : "";
            bool isTicketCountValid = int.TryParse(TicketCount, out int ticketCount) && ticketCount > 0;
            string ticketCountError = isTicketCountValid ? "" : "Du skal indtaste et gyldigt antal billetter\n";

            if (screeningError != "" || emailError != "" || phoneError != "" || ticketCountError != "")
            {
                _dialogService.ShowError(screeningError + emailError + phoneError + ticketCountError, "Fejl i indtastning");
                return;
            }

            int seatsLeft = SelectedScreening!.Capacity - SelectedScreening.ReservedSeats;
            if (ticketCount > seatsLeft)
            {
                _dialogService.ShowError($"Der er kun {seatsLeft} ledige pladser til denne forestilling", "For mange billetter");
                return;
            }

            var booking = new Booking
            {
                Customer = new Customer { Email = CustomerEmail, Phone = CustomerPhone },
                TicketCount = ticketCount
            };
            SelectedScreening.Bookings.Add(booking);
            _cinemaRepo.Save();

            RefreshPlannedScreenings();
            _dialogService.ShowInfo("Reservationen er gemt", "Succes");
            ClearForm();
        }
        private void ClearForm()
        {
            SelectedScreening = null;
            CustomerEmail = "";
            CustomerPhone = "";
            TicketCount = "";
        }
        private void SetMonthName(int monthNumber)
        {
            MonthName = new CultureInfo("da-DK").DateTimeFormat.GetMonthName(monthNumber) + " " + Year; // Navn og årstal på næste måned
        }
        private void PreviousMonth()
        {
            MonthNumber--;
            CheckYearChange();
            SetMonthName(MonthNumber);
            RefreshPlannedScreenings();
        }
        private void NextMonth()
        {
            MonthNumber++;
            CheckYearChange();
            SetMonthName(MonthNumber);
            RefreshPlannedScreenings();
        }
        private void CheckYearChange()
        {
            if (MonthNumber == 13)
            {
                MonthNumber = 1;
                Year++;
            }
            if (MonthNumber == 0)
            {
                MonthNumber = 12;
                Year--;
            }
        }
        private void RefreshPlannedScreenings()
        {
            PlannedScreenings.Clear();
            if (SelectedScreen == null) return;

            var screeningsThisMonth = SelectedScreen.Screenings
                .Where(screening => screening.Date.Month == MonthNumber && screening.Date.Year == Year)
                .OrderBy(screening => screening.Date)
                .ThenBy(screening => screening.StartTime);

            foreach (var screening in screeningsThisMonth)
            {
                PlannedScreenings.Add(screening);
            }
        }
    }
}
