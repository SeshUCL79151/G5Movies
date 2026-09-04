namespace MoviesG5.UI
{
    public class ReservationListDayItem
    {
        public string DateText { get; set; }
        public ScreeningSlot Slot1 { get; set; } = new();
        public ScreeningSlot Slot2 { get; set; } = new();
        public ScreeningSlot Slot3 { get; set; } = new();
    }
}
