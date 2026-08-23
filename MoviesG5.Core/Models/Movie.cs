using MoviesG5.Core;

namespace MoviesG5.Core
{
    public class Movie : IHasId
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public Genre Genre { get; set; }
    }
}
