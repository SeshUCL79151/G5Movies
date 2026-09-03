using System;
using System.Collections.Generic;
using System.Text;

namespace MoviesG5.Core
{
    public class Screening
    {
        public TimeOnly StartTime { get; set; }
        public DateOnly Date { get; set; }
        public Movie Movie { get; set; }
        public int Capacity { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public int ReservedSeats => Bookings.Sum(b => b.TicketCount);

        public string DisplayText =>
            $"{StartTime:HH:mm}-{StartTime.Add(Movie.Duration + TimeSpan.FromMinutes(30)):HH:mm} {Movie.Title} ({ReservedSeats}/{Capacity})";
    }
}
