
using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;
namespace MoviesG5.UI
{
    public class MovieViewModel : ViewModelBase
    {
        private readonly IRepository<Movie> _movieRepo;
        private readonly IDialogService _dialogService;
        public ObservableCollection<Movie> Movies { get; }
        public RelayCommand AddMovieCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        private string _movieTitle = string.Empty;
        public string MovieTitle
        {
            get { return _movieTitle; }
            set { _movieTitle = value; OnPropertyChanged(); }
        }
        private string _movieDirector;
        public string MovieDirector 
        { 
            get { return _movieDirector; }
            set { _movieDirector = value; OnPropertyChanged(); }
        }
        private string _durationInput;
        public string DurationInput
        {
            get { return _durationInput; }
            set { _durationInput = value; OnPropertyChanged(); }
        }
        public IEnumerable<Genre> Genres { get; } = Enum.GetValues<Genre>();

        private Genre? _selectedGenre; // Nullable, da vi gerne vil have vist en tom combobox, indtil brugeren vælger en genre
        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set { _selectedGenre = value; OnPropertyChanged(); }
        }

        public MovieViewModel(IRepository<Movie> repo, IDialogService dialogService)
        {
            _movieRepo = repo;
            _dialogService = dialogService;
            Movies = new ObservableCollection<Movie>(Enumerable.Reverse(_movieRepo.GetAll())); // Vi vil gerne have vist den sidst tilføjede film først i listen og vender derfor repo om
            AddMovieCommand = new RelayCommand(execute => AddMovie(), canexecute => { return true; }); // Kommando til Gem-knap
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; }); // Kommando til Ryd-knap
        }
        
        private void AddMovie() 
        {
            string titleErrorMessage = "";
            string directorErrorMessage = "";
            string durationErrorMessage = "";
            string genreErrorMessage = "";
            bool isFormValid = true;
            if (string.IsNullOrWhiteSpace(_movieTitle)) 
            {
                titleErrorMessage = "Du skal indtaste en titel på filmen\n";
                isFormValid = false;
            }
            if (string.IsNullOrWhiteSpace(_movieDirector))
            {
                directorErrorMessage = "Du skal indtaste en instruktør for filmen\n";
                isFormValid = false;
            }
            bool isDurationValid = TimeSpan.TryParseExact(_durationInput, @"h\:mm", CultureInfo.InvariantCulture, out TimeSpan duration); // Accepterer tider fra 0:00 - 23:59
            if (!isDurationValid)
            {
                durationErrorMessage = "Varighed skal indtastes i formatet t:mm\n";
                isFormValid = false;
            }
            if (_selectedGenre == null)
            {
                genreErrorMessage = "Du skal vælge en genre";
                isFormValid = false;
            }
            if (!isFormValid)
            {
                _dialogService.ShowError(titleErrorMessage + directorErrorMessage + durationErrorMessage + genreErrorMessage, "Fejl i indtastning");
            } else
            {
                var newMovie = new Movie();
                newMovie.Title = _movieTitle;
                newMovie.Director = _movieDirector;
                newMovie.Duration = duration;
                newMovie.Genre = _selectedGenre.Value; // Value tvinger compileren til at tildele en nullable type til en ikke nullable type
                Movies.Insert(0, newMovie); // Indsæt filmen først i listen
                _movieRepo.Add(newMovie);
                _movieRepo.Save();
                _dialogService.ShowInfo("Filmen er gemt", "Succes");
                ClearForm();
            }
        }
        private void ClearForm() 
        {
            MovieTitle = "";
            MovieDirector = "";
            DurationInput = "";
            SelectedGenre = null;
        }



    }
}
