using Griffin.Data.Queries;
using System;
using System.Linq;
using Xunit;

namespace Griffin.Core.Tests.Data.Queries
{
    public class QueryResultTests
    {
        [Fact]
        public void CorrectlyInitialized()
        {
            
            var qr = new QueryResult<User>(new[] {new User {FirstName = "Arne"}}, 100);

            Assert.Equal("Arne", qr.Items.First().FirstName);
            Assert.Equal(100, qr.TotalCount);
        }

        [Fact]
        public void InvalidTotalCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new QueryResult<User>(new[] {new User {FirstName = "Arne"}}, -1));
        }

        [Fact]
        public void NullList()
        {
            Assert.Throws<ArgumentNullException>(() => new QueryResult<User>(null, 0));
        }
    }
}