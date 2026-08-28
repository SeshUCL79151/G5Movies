using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Screening : IHasId
    {
        public int Id { get; set; }
        public TimeOnly StartTime { get; set; }
        public DateOnly Date { get; set; }
    }
}
