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
            var screeningRepo = new RepositoryJson<Screening>("screenings.json");
            
            var dialogService = new MessageBoxDialogService();

            var mainViewModel = new MainViewModel(
                
                new MovieViewModel(movieRepo, dialogService),
                new ProgramViewModel(movieRepo, screeningRepo, dialogService));
                

            var mainWindow = new MainWindow { DataContext = mainViewModel };
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
