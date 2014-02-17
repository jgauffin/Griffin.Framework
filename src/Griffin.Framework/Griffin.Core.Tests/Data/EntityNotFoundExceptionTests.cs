using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Griffin.Data;
using Griffin.Data.Mapper;
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

        [Fact]
        public void serialization_works_with_BinaryFormatter()
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "SELECT";
            cmd.AddParameter("a", "b");
            var serializer = new BinaryFormatter();
            var ms = new MemoryStream();

            var sut = new EntityNotFoundException("Failed", cmd);
            serializer.Serialize(ms, sut);
            ms.Position = 0;
            var actual = (EntityNotFoundException) serializer.Deserialize(ms);

            actual.CommandText.Should().Be(cmd.CommandText);
            actual.CommandParameters.Should().Be("a=b");
        }

        [Fact]
        public void serialization_works_with_datacontract()
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "SELECT";
            cmd.AddParameter("a", "b");
            var serializer = new DataContractSerializer(typeof (EntityNotFoundException));
            var ms = new MemoryStream();

            var sut = new EntityNotFoundException("Failed", cmd);
            serializer.WriteObject(ms, sut);
            ms.Position = 0;
            var actual = (EntityNotFoundException) serializer.ReadObject(ms);

            actual.CommandText.Should().Be(cmd.CommandText);
            actual.CommandParameters.Should().Be("a=b");
        }
    }
}