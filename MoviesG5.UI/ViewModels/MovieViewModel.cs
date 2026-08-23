
using MoviesG5.Core;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
namespace MoviesG5.UI
{
    public class MovieViewModel : ViewModelBase
    {
        private readonly IRepository<Movie> _movieRepo;
        public ObservableCollection<Movie> Movies { get; }
        public RelayCommand AddMovieCommand { get; }
        public RelayCommand ClearFormCommand { get; }

        private string _movieTitle = string.Empty;
        public string MovieTitle
        {
            get { return _movieTitle; }
            set { _movieTitle = value; OnPropertyChanged(); }
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

        public MovieViewModel(IRepository<Movie> repo)
        {
            _movieRepo = repo;
            Movies = new ObservableCollection<Movie>(Enumerable.Reverse(_movieRepo.GetAll())); // Vi vil gerne have vist den sidst tilføjede film først i listen og vender derfor repo om
            AddMovieCommand = new RelayCommand(execute => AddMovie(), canexecute => { return true; }); // Kommando til Gem-knap
            ClearFormCommand = new RelayCommand(execute => ClearForm(), canexecute => { return true; }); // Kommando til Ryd-knap
        }
        
        private void AddMovie() 
        {
            string titleErrorMessage = "";
            string durationErrorMessage = "";
            string genreErrorMessage = "";
            bool isFormValid = true;
            if (_movieTitle == "") 
            {
                titleErrorMessage = "Du skal indtaste en titel på filmen";
                isFormValid = false;
            }
            bool isDurationValid = TimeSpan.TryParseExact(_durationInput, @"h\:mm", CultureInfo.InvariantCulture, out TimeSpan duration);
            if (!isDurationValid)
            {
                durationErrorMessage = "Varighed skal indtastes i formatet t:mm";
                isFormValid = false;
            }
            if (_selectedGenre == null)
            {
                genreErrorMessage = "Du skal vælge en genre";
                isFormValid = false;
            }
            if (!isFormValid)
            {
                MessageBox.Show(titleErrorMessage + "\n" + durationErrorMessage + "\n" + genreErrorMessage, "Fejl i indtastning", MessageBoxButton.OK, MessageBoxImage.Error);
               
            } else
            {
                var newMovie = new Movie();
                newMovie.Title = _movieTitle;
                newMovie.Duration = duration;
                newMovie.Genre = _selectedGenre.Value; // Value tvinger compileren til at tildele en nullable type til en ikke nullable type
                Movies.Insert(0, newMovie); // Indsæt filmen først i listen
                _movieRepo.Add(newMovie);
                _movieRepo.Save();
                MessageBox.Show("Filmen er gemt", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
        }
        private void ClearForm() 
        {
            MovieTitle = "";
            DurationInput = "";
            SelectedGenre = null;
        }



    }
}
