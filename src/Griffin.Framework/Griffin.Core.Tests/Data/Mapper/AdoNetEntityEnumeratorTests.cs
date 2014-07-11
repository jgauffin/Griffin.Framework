using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data.Mapper;
using NSubstitute;
using Xunit;

namespace Griffin.Core.Tests.Data.Mapper
{
    public class AdoNetEntityEnumeratorTests
    {
        [Fact]
        public void make_sure_that_the_constructor_initializes_everything_ok()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);

            cmd.DidNotReceiveWithAnyArgs().ExecuteReader();
        }


        [Fact]
        public void movenext_should_NOT_execute_the_command_when_called_for_the_First_time_if_it_was_passed_in_the_constructor()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            sut.MoveNext();

            cmd.DidNotReceive().ExecuteReader();
        }

        [Fact]
        public void movenext_should_execute_reader_Read()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            var actual = sut.MoveNext();

            reader.Received().Read();
            actual.Should().BeFalse();
        }

        [Fact]
        public void movenext_should_return_Value_from_Read()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();
            reader.Read().Returns(true);

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            var actual = sut.MoveNext();

            actual.Should().BeTrue();
        }

        [Fact]
        public void Reset_is_not_allowed_as_we_cant_move_backwards_with_a_reader()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();
            reader.Read();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            Action actual = sut.Reset;

            actual.ShouldThrow<InvalidOperationException>();
        }


        [Fact]
        public void dispose_reader_if_it_has_been_specified()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            sut.Dispose();

            reader.Received().Dispose();
        }

        [Fact]
        public void should_dispose_command()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            sut.Dispose();

            cmd.Received().Dispose();
        }

        [Fact]
        public void should_dispose_connection_if_its_owned()
        {
            var connection = Substitute.For<IDbConnection>();
            var cmd = Substitute.For<IDbCommand>();
            cmd.Connection.Returns(connection);
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, true);
            sut.Dispose();

            connection.Received().Dispose();
        }

        [Fact]
        public void should_NOT_dispose_connection_if_its_nots_owned()
        {
            var connection = Substitute.For<IDbConnection>();
            var cmd = Substitute.For<IDbCommand>();
            cmd.Connection.Returns(connection);
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, false);
            sut.Dispose();

            connection.DidNotReceive().Dispose();
        }

        [Fact]
        public void may_not_call_Current_before_MoveNext_as_we_need_to_move_to_the_first_record_first()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<string>>();
            object tmp = null;

            var sut = new AdoNetEntityEnumerator<string>(cmd, reader, mapper, false);
            Action actual = () => tmp = sut.Current;

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void only_populate_Current_once_even_if_called_multiple_times_for_the_same_entity()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<Wrapper>>();
            var expected = "Hello";
            mapper.Map(reader, Arg.Do<object>(x => ((Wrapper)x).Value = expected));
            mapper.Create(reader).Returns(new Wrapper());

            var sut = new AdoNetEntityEnumerator<Wrapper>(cmd, reader, mapper, false);
            sut.MoveNext();
            sut.Current.Should().NotBeNull();
            mapper.ClearReceivedCalls();
            var actual = sut.Current;

            mapper.DidNotReceiveWithAnyArgs().Map(null, null);
        }

        [Fact]
        public void map_a_record_first_time_Current_is_invoked()
        {
            var cmd = Substitute.For<IDbCommand>();
            var reader = Substitute.For<IDataReader>();
            var mapper = Substitute.For<ICrudEntityMapper<Wrapper>>();
            var wrapper = new Wrapper();
            var expected = "Hello";
            mapper.Create(reader).Returns(wrapper);
            mapper.Map(reader, Arg.Do<object>(x => wrapper.Value = expected));

            var sut = new AdoNetEntityEnumerator<Wrapper>(cmd, reader, mapper, false);
            sut.MoveNext();
            var actual = sut.Current;

            actual.Value.Should().Be(expected);
        }

        public class Wrapper
        {
            public string Value { get; set; }
        }
    }
}
