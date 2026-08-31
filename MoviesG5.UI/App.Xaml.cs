using System.Collections.ObjectModel;
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
            var cinemaRepo = new RepositoryJson<Cinema>("cinemas.json");
            SeedCinemas(cinemaRepo);

            var movies = new ObservableCollection<Movie>(Enumerable.Reverse(movieRepo.GetAll()));

            var dialogService = new MessageBoxDialogService();

            var mainViewModel = new MainViewModel(

                new MovieViewModel(movieRepo, movies, dialogService),
                new ProgramViewModel(movieRepo, cinemaRepo, movies, dialogService));


            var mainWindow = new MainWindow { DataContext = mainViewModel };
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static void SeedCinemas(IRepository<Cinema> cinemaRepo) // Da brugertilføjelse af biografer ikke implementeres i denne prototype, lægger vi biograferne ind manuelt når programmet køres 1. gang
        {
            if (cinemaRepo.GetAll().Count > 0) return;

            cinemaRepo.Add(new Cinema
            {
                Name = "Hjerm biograf",
                Screens = new List<Screen>
                {
                    new Screen { Id = 1, Name = 1, Capacity = 50 },
                    new Screen { Id = 2, Name = 2, Capacity = 25 }
                }
            });

            cinemaRepo.Add(new Cinema
            {
                Name = "Videbæk biograf",
                Screens = new List<Screen>
                {
                    new Screen { Id = 1, Name = 1, Capacity = 70 },
                    new Screen { Id = 2, Name = 2, Capacity = 25 }
                }
            });
            cinemaRepo.Add(new Cinema
            {
                Name = "Thorsminde biograf",
                Screens = new List<Screen>
                {
                    new Screen { Id = 1, Name = 1, Capacity = 50 }
                }
            });
            cinemaRepo.Add(new Cinema
            {
                Name = "Ræhr biograf",
                Screens = new List<Screen>
                {
                    new Screen { Id = 1, Name = 1, Capacity = 75 }
                }
            });

            cinemaRepo.Save();
        }
    }
}
