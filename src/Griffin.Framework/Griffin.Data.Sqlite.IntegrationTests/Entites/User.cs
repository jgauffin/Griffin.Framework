using System;

namespace Griffin.Data.Sqlite.IntegrationTests.Entites
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int MessageCount { get; set; }

    }
}
