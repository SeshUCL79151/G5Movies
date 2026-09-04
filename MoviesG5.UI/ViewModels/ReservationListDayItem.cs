using MoviesG5.Core;

namespace MoviesG5.UI
{
    public class ScreeningSlot
    {
        public Screening? Screening { get; set; }
        public bool HasScreening => Screening != null;
        public string DisplayText => Screening?.DisplayText ?? "Ledig";
    }

    public class ReservationListDayItem
    {
        public string DateText { get; set; }
        public ScreeningSlot Slot1 { get; set; } = new();
        public ScreeningSlot Slot2 { get; set; } = new();
        public ScreeningSlot Slot3 { get; set; } = new();
    }
}
