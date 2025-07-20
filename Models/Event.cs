using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreApi.Models
{
    public abstract class Event
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string OwnerId { get; set; }

        [Required]
        public string Url { get; set; }
    }

    public class ConferenceEvent : Event
    {
        [Required]
        public string Location { get; set; }
    }

    public class WebinarEvent : Event
    {
    }
}
