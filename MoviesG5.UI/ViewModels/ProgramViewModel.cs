using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;
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
        public RelayCommand SaveScreeningsCommand { get; }
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
            set { _selectedCinema = value; OnPropertyChanged(); }
        }
        public Screen SelectedScreen {
            get { return _selectedScreen; }
            set { _selectedScreen = value; OnPropertyChanged(); }
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

        public ProgramViewModel(IRepository<Movie> movieRepo, IRepository<Cinema> cinemaRepo, IDialogService dialogService)
        {
            _movieRepo = movieRepo;
            _cinemaRepo = cinemaRepo;
            _dialogService = dialogService;
            Movies = new ObservableCollection<Movie>(Enumerable.Reverse(_movieRepo.GetAll()));
            Cinemas = new ObservableCollection<Cinema>(_cinemaRepo.GetAll());
            MonthNumber = DateOnly.FromDateTime(DateTime.Now).Month + 1; // Nummer på næste måned
            SetMonthName(MonthNumber);
            SaveScreeningsCommand = new RelayCommand(execute => SaveScreenings(), canexecute => { return true; }); // Kommando til Gem-knap
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; }); // Kommando til Ryd-knap
            PreviousMonthCommand = new RelayCommand(execute => PreviousMonth(), canexecute => { return true; });
            NextMonthCommand = new RelayCommand(execute => NextMonth(), canexecute => { return true; });
        }
        private void SetMonthName(int monthNumber) {
            MonthName = new CultureInfo("da-DK").DateTimeFormat.GetMonthName(monthNumber); // Navn på næste måned
        }
        private void SaveScreenings()
        {

        }
        private void ClearForm()
        {

        }
        private void PreviousMonth() // Årstal skal med
        {
            MonthNumber--;
            SetMonthName(MonthNumber);
        }
        private void NextMonth() {
            MonthNumber++;
            SetMonthName(MonthNumber);
        }
    }
}
