namespace backend.Models.PetCalendars
{
    public class PetCalendar
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<PetCalendarItem> petCalendarItems { get; set; }
        public IEnumerable<PetCalendarNote> petCalendarNotes { get; set; } 
    }
}
