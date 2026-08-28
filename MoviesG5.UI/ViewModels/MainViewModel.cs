using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.UI
{
    public class MainViewModel : ViewModelBase
    {
        public MovieViewModel MovieViewModel { get; }
        public ProgramViewModel ProgramViewModel { get; }

        public MainViewModel(MovieViewModel movieVm, ProgramViewModel programVm)
        {
            MovieViewModel = movieVm;
            ProgramViewModel = programVm;
        }
    }
}
