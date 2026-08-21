using System.Configuration;
using System.Data;
using System.Windows;
using MoviesG5.Core;
namespace MoviesG5.UI



{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var movieRepo = new RepositoryJson<Movie>("movies.json");
            var viewModel = new MovieViewModel(movieRepo);
            var view = new MovieWindow();
            view.DataContext = viewModel;
            this.MainWindow = view;
            view.Show();
        }
    }
}
