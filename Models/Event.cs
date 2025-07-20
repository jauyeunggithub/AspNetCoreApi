// Models/Event.cs
public abstract class Event
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Date { get; set; }
}

public class ConferenceEvent : Event
{
    public string Location { get; set; }
}

public class WebinarEvent : Event
{
    public string Url { get; set; }
}
