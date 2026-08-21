using System;
using System.Collections.Generic;
using System.Text;
using MoviesG5.Core;
namespace MoviesG5.UI
{
    public class MovieViewModel : ViewModelBase
    {
        private readonly IRepository<Movie> _movieRepo;
        private List<Movie> _movies;
        public MovieViewModel(IRepository<Movie> repo)
        {
            _movieRepo = repo;
            LoadMovies();
        }
        private void LoadMovies()
        {
            _movies = _movieRepo.GetAll().ToList();
        }
    }
}
