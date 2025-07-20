namespace AspNetCoreApi.Models
{
    public abstract class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string OwnerId { get; set; }
        public string Location { get; set; }  // Add this property
        public string Url { get; set; }       // Add this property
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
