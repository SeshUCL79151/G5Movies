using MoviesG5.Core;

namespace MoviesG5.UI
{
    public class ProgramViewModel : ViewModelBase
    {
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<Screening> _screeningRepo;
        private readonly IDialogService _dialogService;

        public ProgramViewModel(IRepository<Movie> movieRepo, IRepository<Screening> screeningRepo, IDialogService dialogService)
        {
            _movieRepo = movieRepo;
            _screeningRepo = screeningRepo;
            _dialogService = dialogService;
        }
    }
}
