using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Griffin.Data;
using Xunit;

namespace Griffin.Core.Tests.Data
{
    public class EntityNotFoundExceptionTests
    {
        [Fact]
        public void properties_Are_assigned()
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "SELECT";
            cmd.AddParameter("a", "b");

            var sut = new EntityNotFoundException("Failed", cmd);

            sut.CommandText.Should().Be(cmd.CommandText);
            sut.CommandParameters.Should().Be("a=b");
        }

    }
}