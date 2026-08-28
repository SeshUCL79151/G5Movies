using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Cinema : IHasId
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
