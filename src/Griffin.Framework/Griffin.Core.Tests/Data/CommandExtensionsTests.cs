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
        public void cant_operate_when_command_is_null()
        {

            IDbCommand sut = null;
            Action actual =  () => sut.AddParameter("name", "arne");

            actual.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void cant_operate_if_supplied_argument_do_not_specify_a_name()
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
