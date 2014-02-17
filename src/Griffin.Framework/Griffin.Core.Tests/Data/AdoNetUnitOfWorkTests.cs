using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Data;
using Xunit;

namespace Griffin.Core.Tests.Data
{
    public class AdoNetUnitOfWorkTests
    {
        [Fact]
        public void commit_successfully_if_no_commit_have_been_previously_made()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection);
            sut.SaveChanges();

            connection.Transactions[0].IsCommitted.Should().BeTrue();
        }

        [Fact]
        public void throw_if_commit_have_been_previously_made()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection);
            sut.SaveChanges();
            Action actual = sut.SaveChanges;

            actual.ShouldThrow<TransactionAlreadyClosedException>();
        }

        [Fact]
        public void dont_close_connection_per_default()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection);
            sut.Dispose();

            connection.State.Should().Be(ConnectionState.Open);
        }

        [Fact]
        public void close_connection_if_owned_is_set()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection, true);
            sut.Dispose();

            connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public void rollback_transaction_if_savechanges_have_not_been_made()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection);
            sut.Dispose();

            connection.Transactions[0].IsRolledBack.Should().BeTrue();
        }

        [Fact]
        public void close_connection_on_dispose_even_if_savechanges_have_been_called()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection, true);
            sut.SaveChanges();
            sut.Dispose();


            connection.Transactions[0].IsCommitted.Should().BeTrue();
            connection.State.Should().Be(ConnectionState.Closed);
        }

        [Fact]
        public void use_the_specified_isolation_level()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection, true, IsolationLevel.Chaos);


            connection.Transactions[0].IsolationLevel.Should().Be(IsolationLevel.Chaos);
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Fact]
        public void attach_transaction_to_created_command()
        {
            var connection = new Griffin.AdoNetFakes.FakeConnection();
            connection.CurrentState = ConnectionState.Open;

            var sut = new AdoNetUnitOfWork(connection);
            var actual = sut.CreateCommand();

            actual.Transaction.Should().BeSameAs(connection.Transactions[0]);
            actual.Connection.Should().BeSameAs(connection);
        }


    }
}
