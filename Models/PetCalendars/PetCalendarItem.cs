namespace backend.Models.PetCalendars
{
    public class PetCalendarItem
    {
        public Guid Id { get; set; }
        public Guid CalendarId { get; set; }

        public TimeOnly Time { get; set; }
        public string Name { get; set; }
        public bool isActive { get; set; }
        public ActionType ActionType { get; set; }
    }
}
