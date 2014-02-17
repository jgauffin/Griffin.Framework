using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Services
{
    public class TransactionManagerTests
    {
        [Fact]
        public void pending_is_false_with_no_transactions()
        {
            var sut = new TransactionManager();

            var actual = sut.HasActiveTransactions;

            actual.Should().BeFalse();
        }

        [Fact]
        public void pending_is_true_when_we_got_an_Active_transaction()
        {
            var sut = new TransactionManager();
            sut.Begin("abc");

            var actual = sut.HasActiveTransactions;

            actual.Should().BeTrue();
        }


        [Fact]
        public void transaction_id_must_be_unique_for_Every_new_transaction()
        {
            var sut = new TransactionManager();
            sut.Begin("abc");

            Action actual = () => sut.Begin("abc");

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void transaction_id_should_be_able_to_Be_reused_after_rollback()
        {
            var sut = new TransactionManager();
            sut.Begin("abc");
            sut.Rollback("abc");

            sut.Begin("abc");

        }

        [Fact]
        public void transaction_id_should_be_able_to_Be_reused_after_commit()
        {
            var sut = new TransactionManager();
            sut.Begin("abc");
            sut.Commit("abc");

            sut.Begin("abc");

        }

        [Fact]
        public void cannot_rollback_missing_transactions()
        {
            var sut = new TransactionManager();

            Action actual = () => sut.Rollback("abc");

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void cannot_commit_missing_transactions()
        {
            var sut = new TransactionManager();

            Action actual = () => sut.Commit("abc");

            actual.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void can_commit_existing_transaction()
        {
            var sut = new TransactionManager();
            sut.Begin("abd");

            sut.Commit("abd");

        }

        [Fact]
        public void cleanup_cancels_all_transactions()
        {
            var firstIsRolledback = false;
            var secondIsRolledback = false;
            var sut = new TransactionManager();
            sut.Begin("abd");
            sut.Enqueue("abd", () => { }, delegate { firstIsRolledback = true; });
            sut.Begin("abc");
            sut.Enqueue("abc", () => { }, delegate { secondIsRolledback = true; });

            sut.Cleanup();

            firstIsRolledback.Should().BeTrue();
            secondIsRolledback.Should().BeTrue();
        }

        [Fact]
        public void commit_invokes_all_transaction_actions()
        {
            var first = false;
            var second = false;
            var sut = new TransactionManager();
            sut.Begin("abd");
            sut.Enqueue("abd", delegate { first = true; },() => { });
            sut.Enqueue("abd", delegate { second = true; }, () => { });

            sut.Commit("abd");

            first.Should().BeTrue();
            second.Should().BeTrue();
        }

        [Fact]
        public void cant_enqueue_in_nonexistant_transaction()
        {
            var sut = new TransactionManager();

            Action actual = () => sut.Enqueue("abd", delegate { }, () => { });

            actual.ShouldThrow<InvalidOperationException>();
        }
    }
}
