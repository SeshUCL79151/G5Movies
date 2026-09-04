using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MoviesG5.UI
{
    public class ProgramViewModel : ViewModelBase
    {
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IDialogService _dialogService;
        public ObservableCollection<Movie> Movies { get; }
        public ObservableCollection<Cinema> Cinemas { get; }
        public ObservableCollection<ProgramListDayItem> PlannedScreenings { get; }

        public RelayCommand AddScreeningsCommand { get; }
        public RelayCommand ClearFormCommand { get; }
        public RelayCommand PreviousMonthCommand { get; }
        public RelayCommand NextMonthCommand { get; }

        private Movie _selectedMovie;
        private Cinema _selectedCinema;
        private Screen _selectedScreen;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string? _startTimeInput;
        private string _monthName;

        private int MonthNumber { get; set; }
        private int Year {  get; set; }
        

        public string MonthName
        {
            get { return _monthName; }
            set { _monthName = value; OnPropertyChanged(); }
        }

        public Movie SelectedMovie
        {
            get { return _selectedMovie; }
            set { _selectedMovie = value; OnPropertyChanged(); }
        }
        public Cinema SelectedCinema {
            get { return _selectedCinema; }
            set { _selectedCinema = value; OnPropertyChanged(); SelectedScreen = null; }
        }
        public Screen SelectedScreen {
            get { return _selectedScreen; }
            set { _selectedScreen = value; OnPropertyChanged(); RefreshPlannedScreenings(); }
        }
        public DateTime? StartDate {
            get { return _startDate; }
            set { _startDate = value; OnPropertyChanged(); }
        }
        public DateTime? EndDate {
            get { return _endDate; }
            set { _endDate = value; OnPropertyChanged(); }
        }
        public string StartTimeInput {
            get { return _startTimeInput; }
            set { _startTimeInput = value; OnPropertyChanged(); }
        }

        public ProgramViewModel(IRepository<Cinema> cinemaRepo, ObservableCollection<Movie> movies, IDialogService dialogService)
        {
            _cinemaRepo = cinemaRepo;
            _dialogService = dialogService;
            Movies = movies;
            Cinemas = new ObservableCollection<Cinema>(_cinemaRepo.GetAll());
            PlannedScreenings = new ObservableCollection<ProgramListDayItem>();
            DateTime date = DateTime.Now;
            MonthNumber = DateOnly.FromDateTime(date).Month + 1;// Nummer på næste måned
            Year = date.Year;
            CheckYearChange();
            SetMonthName(MonthNumber); // Programmet starter med næste måned valgt
            StartDate = new DateTime(Year, MonthNumber, 1);
            EndDate = StartDate;
            AddScreeningsCommand = new RelayCommand(execute => AddScreenings(), canexecute => { return true; }); // Gem-knap
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; }); // Ryd-knap
            PreviousMonthCommand = new RelayCommand(execute => PreviousMonth(), canexecute => { return true; }); // Forrige måned
            NextMonthCommand = new RelayCommand(execute => NextMonth(), canexecute => { return true; }); // Næste måned
        }
        
        private void AddScreenings()
        {
            string movieError = SelectedMovie == null ? "Du skal vælge en film\n" : "";
            string screenError = SelectedScreen == null ? "Du skal vælge en biografsal\n" : "";
            string dateError = (StartDate == null || EndDate == null || StartDate > EndDate) ? "Du skal vælge start- og slutdato, og startdato må ikke være efter slutdato\n" : "";
            bool isTimeValid = TimeOnly.TryParse(StartTimeInput, out TimeOnly startTime);
            string timeError = isTimeValid ? "" : "Du skal indtaste et gyldigt starttidspunkt (t:mm)\n";

            if (movieError != "" || screenError != "" || dateError != "" || timeError != "")
            {
                _dialogService.ShowError(movieError + screenError + dateError + timeError, "Fejl i indtastning");
                return;
            }

            TimeOnly endTime = startTime.Add(SelectedMovie.Duration + TimeSpan.FromMinutes(30));

            var conflictDates = new List<DateOnly>();
            bool anyOverlap = false;
            bool anyFull = false;
            for (DateTime date = StartDate.Value; date <= EndDate.Value; date = date.AddDays(1))
            {
                DateOnly screeningDate = DateOnly.FromDateTime(date);
                var screeningsOnDate = SelectedScreen.Screenings.Where(screening => screening.Date == screeningDate).ToList();

                bool overlaps = screeningsOnDate.Any(screening =>
                    screening.StartTime < endTime && startTime < screening.StartTime.Add(screening.Movie.Duration + TimeSpan.FromMinutes(30))); // Overlapper de forevisninger der skal tilføjes eksisterende forevisninger
                bool full = screeningsOnDate.Count >= 3; // Er der allerede 3 forevisninger på dagen?

                if (overlaps || full)
                    conflictDates.Add(screeningDate);

                if (overlaps)
                    anyOverlap = true;
                if (full)
                    anyFull = true;
            }

            if (conflictDates.Any())
            {
                string reason = anyOverlap && anyFull ? "overlappende visninger og fyldte dage"
                              : anyOverlap ? "overlappende visninger"
                              : "fyldte dage";
                string dateList = string.Join(", ", conflictDates.Select(d => d.ToString("dd/MM")));
                _dialogService.ShowError($"Kunne ikke oprette visninger pga. {reason} følgende dage: {dateList}", "Konflikt");
                return;
            }

            for (DateTime date = StartDate.Value; date <= EndDate.Value; date = date.AddDays(1))
            {
                SelectedScreen.Screenings.Add(new Screening
                {
                    Date = DateOnly.FromDateTime(date),
                    StartTime = startTime,
                    Movie = SelectedMovie,
                    Capacity = SelectedScreen.Capacity
                });
            }

            _cinemaRepo.Save();
            RefreshPlannedScreenings();
            ClearForm();
        }
        private void ClearForm()
        {
            SelectedMovie = null;
            StartDate = new DateTime(Year, MonthNumber, 1);
            EndDate = StartDate;
            StartTimeInput = "";
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
        private void NextMonth() {
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
            foreach (var screening in GetScreeningsByMonth()) // Observable Collection PlannedScreenings er get only, så listen der er genereret i GetScreeningsByMonth() lægges ind i PlannedScreenings ét dayItem af gangen, da den ikke kan assignes direkte.
            {
                {
                PlannedScreenings.Add(screening);
            }
        }
        private List<ProgramListDayItem> GetScreeningsByMonth() {
            List<ProgramListDayItem> monthList = new List<ProgramListDayItem>();
            if (SelectedScreen == null) return monthList;

            var plannedScreenings = from screening in SelectedScreen.Screenings
                                    where (screening.Date.Month == MonthNumber &&
                                    screening.Date.Year == Year)
                                    select new
                                    {
                                        day = screening.Date.Day,
                                        startTime = screening.StartTime,
                                        movie = screening.Movie
                                    };
            int daysInMonth = DateTime.DaysInMonth(Year, MonthNumber);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var screeningsOnDay = (from screening in plannedScreenings
                                       where day == screening.day
                                       orderby screening.startTime
                                       select screening.startTime.ToString("HH:mm") + "-" +
                                              screening.startTime.Add(screening.movie.Duration + TimeSpan.FromMinutes(30)).ToString("HH:mm") + " " + screening.movie.Title)
                                              .ToList();

                var dayItem = new ProgramListDayItem
                {
                    DateText = day.ToString(),
                    ScreeningText = screeningsOnDay
                };
                while (dayItem.ScreeningText.Count < 3)
                {
                    dayItem.ScreeningText.Add("Ledig");
                }
                monthList.Add(dayItem);
            }
            return monthList;
        }
    }
}
