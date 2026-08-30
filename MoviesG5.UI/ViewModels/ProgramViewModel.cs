using Microsoft.VisualBasic;
using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Input;

namespace MoviesG5.UI
{
    public class ProgramViewModel : ViewModelBase
    {
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<Cinema> _cinemaRepo;
        private readonly IDialogService _dialogService;
        public ObservableCollection<Movie> Movies { get; }
        public ObservableCollection<Cinema> Cinemas { get; }
        public ObservableCollection<ScreeningListItem> PlannedScreenings { get; }

        public RelayCommand AddScreeningsCommand { get; }
        public RelayCommand DeleteScreeningsCommand { get; }
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

        public ProgramViewModel(IRepository<Movie> movieRepo, IRepository<Cinema> cinemaRepo, ObservableCollection<Movie> movies, IDialogService dialogService)
        {
            _movieRepo = movieRepo;
            _cinemaRepo = cinemaRepo;
            _dialogService = dialogService;
            Movies = movies;
            Cinemas = new ObservableCollection<Cinema>(_cinemaRepo.GetAll());
            PlannedScreenings = new ObservableCollection<ScreeningListItem>();
            DateTime date = DateTime.Now;
            MonthNumber = DateOnly.FromDateTime(date).Month + 1;// Nummer på næste måned
            Year = date.Year;
            CheckYearChange();
            SetMonthName(MonthNumber); // Programmet starter med næste måned valgt
            StartDate = new DateTime(Year, MonthNumber, 1);
            EndDate = StartDate;
            AddScreeningsCommand = new RelayCommand(execute => AddScreenings(), canexecute => { return true; }); // Kommando til Gem-knap
            DeleteScreeningsCommand = new RelayCommand(execute => DeleteScreenings(), canexecute => { return true; }); // Kommando til Slet-knap
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; }); // Kommando til Ryd-knap
            PreviousMonthCommand = new RelayCommand(execute => PreviousMonth(), canexecute => { return true; });
            NextMonthCommand = new RelayCommand(execute => NextMonth(), canexecute => { return true; });
        }
        
        private void AddScreenings()
        {
            string movieError = SelectedMovie == null ? "Du skal vælge en film\n" : "";
            string screenError = SelectedScreen == null ? "Du skal vælge en biografsal\n" : "";
            string dateError = (StartDate == null || EndDate == null) ? "Du skal vælge start- og slutdato\n" : "";
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
                var screeningsOnDate = SelectedScreen.Screenings.Where(s => s.Date == screeningDate).ToList();

                bool overlaps = screeningsOnDate.Any(s =>
                    s.StartTime < endTime && startTime < s.StartTime.Add(s.Movie.Duration + TimeSpan.FromMinutes(30)));
                bool full = screeningsOnDate.Count >= 3;

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
                    Movie = SelectedMovie
                });
            }

            _cinemaRepo.Save();
            RefreshPlannedScreenings();
            ClearForm();
        }
        private void DeleteScreenings()
        {
            string movieError = SelectedMovie == null ? "Du skal vælge en film\n" : "";
            string screenError = SelectedScreen == null ? "Du skal vælge en biografsal\n" : "";
            string dateError = (StartDate == null || EndDate == null) ? "Du skal vælge start- og slutdato\n" : "";
            bool isTimeValid = TimeOnly.TryParse(StartTimeInput, out TimeOnly startTime);
            string timeError = isTimeValid ? "" : "Du skal indtaste et gyldigt starttidspunkt (t:mm)\n";

            if (movieError != "" || screenError != "" || dateError != "" || timeError != "")
            {
                _dialogService.ShowError(movieError + screenError + dateError + timeError, "Fejl i indtastning");
                return;
            }

            DateOnly startDate = DateOnly.FromDateTime(StartDate.Value);
            DateOnly endDate = DateOnly.FromDateTime(EndDate.Value);

            var matches = SelectedScreen.Screenings.Where(s =>
                s.Movie.Id == SelectedMovie.Id &&
                s.StartTime == startTime &&
                s.Date >= startDate && s.Date <= endDate).ToList();

            if (!matches.Any())
            {
                _dialogService.ShowInfo("Ingen visninger matchede søgningen", "Ingen match");
                return;
            }

            foreach (var screening in matches)
            {
                SelectedScreen.Screenings.Remove(screening);
            }

            _cinemaRepo.Save();
            RefreshPlannedScreenings();

            string dateList = string.Join(", ", matches.Select(s => $"{s.Date:dd/MM} {s.StartTime:HH:mm} {s.Movie.Title}"));
            _dialogService.ShowInfo($"Følgende visninger blev slettet: {dateList}", "Slettet");
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
            foreach (var item in GetScreeningsByMonth())
            {
                PlannedScreenings.Add(item);
            }
        }
        private List<ScreeningListItem> GetScreeningsByMonth() {
            List<ScreeningListItem> monthList = new List<ScreeningListItem>();
            if (SelectedScreen == null) return monthList;

            var plannedScreenings = from scr in SelectedScreen.Screenings
                                    where (scr.Date.Month == MonthNumber &&
                                    scr.Date.Year == Year)
                                    select new
                                    {
                                        day = scr.Date.Day,
                                        startTime = scr.StartTime,
                                        movie = scr.Movie
                                    };
            int daysInMonth = DateTime.DaysInMonth(Year, MonthNumber);
            for (int days = 1; days <= daysInMonth; days++)
            {
                var screeningsOnDay = (from scr in plannedScreenings
                                       where days == scr.day
                                       orderby scr.startTime
                                       select scr.startTime.ToString("HH:mm") + "-" +
                                              scr.startTime.Add(scr.movie.Duration + TimeSpan.FromMinutes(30)).ToString("HH:mm") + " " + scr.movie.Title)
                                       .ToList();

                var item = new ScreeningListItem
                {
                    DateText = days.ToString(),
                    ScreeningText = screeningsOnDay
                };
                while (item.ScreeningText.Count < 3)
                {
                    item.ScreeningText.Add("Ledig");
                }
                monthList.Add(item);
            }
            return monthList;
        }
    }
}
