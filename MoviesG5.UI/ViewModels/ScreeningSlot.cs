using MoviesG5.Core;

namespace MoviesG5.UI
{
    public class ScreeningSlot
    {
        public Screening? Screening { get; set; }
        public bool HasScreening => Screening != null;
        public string DisplayText => Screening?.DisplayText ?? "Ledig";
    }
}
