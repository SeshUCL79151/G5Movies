namespace MoviesG5.UI
{
    public class ReservationListDayItem
    {
        public string DateText { get; set; }
        public ScreeningSlot Slot1 { get; set; } = new(); // Undgår null fejl ved at sætte Slot1 til et tomt objekt. Slot1.Screening er stadig null (bruges som knap disablingscheck i View)
        public ScreeningSlot Slot2 { get; set; } = new();
        public ScreeningSlot Slot3 { get; set; } = new();
    }
}
