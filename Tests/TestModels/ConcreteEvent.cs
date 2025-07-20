// File: AspNetCoreApi.Tests/TestModels/ConcreteEvent.cs

namespace AspNetCoreApi.Tests.TestModels
{
    using AspNetCoreApi.Models;

    public class ConcreteEvent : Event
    {
        public ConcreteEvent(string title, string location, string url, string ownerId)
        {
            Title = title;
            Location = location;
            Url = url;
            OwnerId = ownerId;
        }
    }
}
