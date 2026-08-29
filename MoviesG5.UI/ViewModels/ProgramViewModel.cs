using MoviesG5.Core;
using System.Collections.ObjectModel;
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

        private Movie _selectedMovie;
        private Cinema _selectedCinema;
        private Screen _selectedScreen;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private TimeOnly? _startTime;
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
        public TimeOnly? StartTime {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(); }
        }

        public ICommand SaveScreeningsCommand { get; }
        public ICommand ClearFormCommand { get; }

        public ProgramViewModel(IRepository<Movie> movieRepo, IRepository<Cinema> cinemaRepo, IDialogService dialogService)
        {
            _movieRepo = movieRepo;
            _cinemaRepo = cinemaRepo;
            _dialogService = dialogService;
            Movies = new ObservableCollection<Movie>(Enumerable.Reverse(_movieRepo.GetAll()));
            Cinemas = new ObservableCollection<Cinema>(_cinemaRepo.GetAll());
        }
    }
}
