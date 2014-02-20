using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.AdoNetFakes;
using Griffin.Data;
using Xunit;
using FluentAssertions;
using CommandExtensions = Griffin.Data.CommandExtensions;

namespace Griffin.Core.Tests.Data
{
    public class CommandExtensionsTests
    {
        [Fact]
        public void Do_not_allow_command_to_be_null()
        {

            IDbCommand sut = null;
            Action actual =  () => sut.AddParameter("name", "arne");

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void do_not_allow_name_to_be_null()
        {

            var sut = new FakeCommand();
            Action actual = () => CommandExtensions.AddParameter(sut, null, "arne");

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void convert_null_value_to_dbnull()
        {

            var sut = new FakeCommand();
            CommandExtensions.AddParameter(sut, "name", null);

            sut.Parameters[0].Value.Should().Be(DBNull.Value);
        }

        [Fact]
        public void add_created_parameter_to_command()
        {

            var sut = new FakeCommand();
            CommandExtensions.AddParameter(sut, "name", "arne");

            sut.Parameters[0].Value.Should().Be("arne");
            sut.Parameters[0].ParameterName.Should().Be("name");
        }
    }
}
