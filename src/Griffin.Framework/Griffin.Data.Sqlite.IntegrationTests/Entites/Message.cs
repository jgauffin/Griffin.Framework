using System;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}