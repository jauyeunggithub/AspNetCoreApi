namespace AspNetCoreApi.Models
{
    public abstract class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string OwnerId { get; set; } // This is your 'UserId' from previous errors
        public string Location { get; set; } // This needs to be set up correctly
        public string Url { get; set; }      // This is your 'Link' from previous errors
    }

    public class ConferenceEvent : Event
    {

        public new string Location { get; set; }
    }

    public class WebinarEvent : Event
    {
        public new string Url { get; set; }
    }
}
