using System;
using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker.Services
{
    public class MemoryQueueRepositoryTests
    {
        [Fact]
        public void add_queue_requires_a_name()
        {
            var queue = new StompQueue();

            var sut = new MemoryQueueRepository();
            Action actual = () => sut.Add(queue);

            actual.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void add_queue_using_minimal_requirements()
        {
            var queue = new StompQueue();
            queue.Name = "mama";

            var sut = new MemoryQueueRepository();
            sut.Add(queue);

        }

        [Fact]
        public void fetch_existing_queue()
        {
            var queue = new StompQueue();
            queue.Name = "mama";
            var sut = new MemoryQueueRepository();
            sut.Add(queue);

            var actual = sut.Get("mama");

            actual.Should().BeSameAs(queue);
        }

        [Fact]
        public void fetch_missing_queue()
        {
            var sut = new MemoryQueueRepository();

            Action actual = () => sut.Get("mama");

            actual.ShouldThrow<NotFoundException>();
        }

        [Fact]
        public void try_to_Fetch_a_queue_Without_specifying_a_name()
        {
            var sut = new MemoryQueueRepository();

            Action actual = () => sut.Get(null);

            actual.ShouldThrow<ArgumentNullException>();
        }
    }
}
