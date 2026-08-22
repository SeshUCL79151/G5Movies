
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
        private string _movieTitle = string.Empty;
        public string MovieTitle
        {
            get { return _movieTitle; }
            set { _movieTitle = value; }
        }
        private Movie _movie;
        private string _movieDurationString;
        public string MovieDurationString
        {
            get { return _movieDurationString; }
            set { _movieDurationString = value; }
        }

        public MovieViewModel(IRepository<Movie> repo)
        {
            _movieRepo = repo;
            Movies = new ObservableCollection<Movie>(_movieRepo.GetAll());
            AddMovieCommand = new RelayCommand(execute => AddMovie(), canexecute => { return true; });
        }
        private void AddMovie() 
        {
            string titleErrorMsg = "";
            string durationErrorMsg = "";
            Boolean isInputOk = true;
            if (_movieTitle == "") 
            {
                titleErrorMsg = "Du skal indtaste en titel på filmen";
                isInputOk = false;
            }
            bool isDurationStringOk = TimeSpan.TryParseExact(_movieDurationString,@"h\:mm", CultureInfo.InvariantCulture, out TimeSpan duration);
            if (!isDurationStringOk)
            {
                durationErrorMsg = "Varighed skal indtastes i formatet t:mm";
                isInputOk = false;
            }
            if (!isInputOk)
            {
                MessageBox.Show(titleErrorMsg + "\n" + durationErrorMsg, "Fejl i indtastning", MessageBoxButton.OK, MessageBoxImage.Error);
               
            } else
            {
                _movie = new Movie();
                _movie.Title = _movieTitle;
                _movie.Duration = duration;

                Movies.Add(_movie);
                _movieTitle = "";
                _movieDurationString = "";
            }
        }

        
    }
}
