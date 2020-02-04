using FluentAssertions;
using Griffin.Net.Buffers;
using Xunit;

namespace Griffin.Core.Tests.Net.Buffers
{
    public class StandAloneBufferTests
    {
        [Fact]
        public void initialize_own_buffer()
        {
            var buffer = new byte[10];

            var sut = new StandAloneBuffer(buffer, 0, 10);

            Assert.Same(buffer, sut.Buffer);
            sut.Offset.Should().Be(0);
            sut.Capacity.Should().Be(10);
            sut.Buffer.Should().BeEquivalentTo(buffer);
        }

        //[Fact]
        //public void initialize_shared_buffer()
        //{
        //    var buffer = new byte[10];

        //    var sut = new StandAloneBuffer(buffer, 5, 5);

        //    Assert.Same(buffer, sut.Buffer);
        //    sut.Capacity.Should().Be(5);
        //    sut.Count.Should().Be(0);
        //    sut.BufferOffset.Should().Be(5);
        //}

        //[Fact]
        //public void initialize_throw_on_overflow()
        //{
        //    var buffer = new byte[10];

        //    Action x = () => new StandAloneBuffer(buffer, 5, 10);

        //    x.Throws<ArgumentOutOfRangeException>();
        //}

        //[Fact]
        //public void initialize_with_null_buffer()
        //{

        //    Action x = () => new StandAloneBuffer(null, 5, 10);

        //    x.Throws<ArgumentNullException>();
        //}

        //[Fact]
        //public void write_5_bytes_and_set_length()
        //{
        //    var buffer = new byte[10];

        //    var sut = new StandAloneBuffer(buffer, 5, 5);
        //    sut.SetLength(5);

        //    sut.Count.Should().Be(5);
        //    sut.BufferOffset.Should().Be(5, "Do not move forward when setting length");
        //}

        //[Fact]
        //public void write_5_bytes_and_increase_length()
        //{
        //    var buffer = new byte[10];

        //    var sut = new StandAloneBuffer(buffer, 5, 5);
        //    sut.IncreaseLength(2);
        //    sut.IncreaseLength(3);

        //    sut.Count.Should().Be(5);
        //}

        //[Fact]
        //public void copy_to_stream()
        //{
        //    var buffer = Encoding.ASCII.GetBytes("Hello world");
        //    var destination = new StandAloneBuffer(new byte[20], 0, 20);

        //    var sut = new StandAloneBuffer(buffer, 0, 11);
        //    sut.SetLength(buffer.Length);
        //    var result = sut.CopyTo(destination, destination.Capacity);

        //    destination.Buffer.Should().Contain(buffer);
        //    destination.Count.Should().Be(buffer.Length);
        //    result.Should().Be(buffer.Length);
        //}

        //[Fact]
        //public void copy_to_stream__requested_length_is_larger_than_the_buffer()
        //{
        //    var buffer = Encoding.ASCII.GetBytes("Hello world");
        //    var slice = new StandAloneBuffer(new byte[20], 0, 20);

        //    var sut = new StandAloneBuffer(buffer, 0, 11);
        //    Action x = () => sut.CopyTo(slice, 200);

        //    x.Throws<ArgumentOutOfRangeException>();
        //}

        //[Fact]
        //public void copy_to_stream__no_bytes_to_copy()
        //{
        //    var buffer = Encoding.ASCII.GetBytes("Hello world");
        //    var destination = new StandAloneBuffer(new byte[20], 0, 0);

        //    var sut = new StandAloneBuffer(buffer, 0, 11);
        //    var result = sut.CopyTo(destination, destination.Capacity);

        //    destination.Buffer.Should().OnlyContain(x => x == 0);
        //    destination.Count.Should().Be(0);
        //    result.Should().Be(0);
        //}
    }
}
