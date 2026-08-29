using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Screen : IHasId
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public int Capacity { get; set; }
        public List<Screening> Screenings { get; set; } = new List<Screening>();
    }
}
