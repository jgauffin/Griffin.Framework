using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Core.External.SimpleJson;
using Xunit;

namespace Griffin.Core.Tests.External
{
    public class SimpleJsonTests
    {
        [Fact]
        public void serialize_private_fields()
        {
            var expected = new PrivateClass("hej", 1337);

            var json = SimpleJson.SerializeObject(expected);
            var actual = SimpleJson.DeserializeObject<PrivateClass>(json);

            actual.A.Should().Be(expected.A);
        }

        [Fact]
        public void serialize_private_constructor()
        {
            var expected = new PrivateClass("hej", 1337);

            var json = SimpleJson.SerializeObject(expected);
            var actual = SimpleJson.DeserializeObject<PrivateClass>(json);

            actual.A.Should().Be(expected.A);
        }

        [Fact]
        public void serialize_enum()
        {
            var expected = new PocoWithEnum();
            expected.StatusCode = HttpStatusCode.NotFound;

            var json = SimpleJson.SerializeObject(expected);
            var actual = SimpleJson.DeserializeObject<PocoWithEnum>(json);

            actual.StatusCode.Should().Be(expected.StatusCode);
        }

        [Fact]
        public void Serialize_enum_from_int()
        {
            var json = @"{""StatusCode"":404}";


            var actual = SimpleJson.DeserializeObject<PocoWithEnum>(json);

            actual.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
