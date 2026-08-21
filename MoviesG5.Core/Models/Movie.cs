using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Movie : IHasId
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
