using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core.Models
{
    internal class Screen : IHasId
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public int Capacity { get; set; }
    }
}
