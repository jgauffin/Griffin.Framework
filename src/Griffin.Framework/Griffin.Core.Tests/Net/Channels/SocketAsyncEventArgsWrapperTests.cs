using System.Net.Sockets;
using FluentAssertions;
using Griffin.Net.Channels;
using Xunit;

namespace Griffin.Core.Tests.Net.Channels
{
    public class SocketAsyncEventArgsWrapperTests
    {
        [Fact]
        public void SetBuffer_should_also_be_made_in_our_args()
        {
            var e = new SocketAsyncEventArgs();
            var buf = new byte[65535];

            var sut = new SocketAsyncEventArgsWrapper(e);
            sut.SetBuffer(buf, 1, 100);

            e.Offset.Should().Be(1);
            e.Count.Should().Be(100);
            e.Buffer.Length.Should().Be(buf.Length);
            sut.BaseOffset.Should().Be(1);
            sut.Count.Should().Be(e.Count);
        }


        [Fact]
        public void SetBuffer_without_buffer_should_also_be_made_in_our_args()
        {
            var e = new SocketAsyncEventArgs();
            var buf = new byte[65535];

            var sut = new SocketAsyncEventArgsWrapper(e);
            sut.SetBuffer(buf, 1, 100);
            sut.SetBuffer(2, 101);

            e.Offset.Should().Be(2);
            e.Count.Should().Be(101);
            e.Buffer.Length.Should().Be(buf.Length);
            sut.BaseOffset.Should().Be(1);
            sut.Offset.Should().Be(2);
        }

    }
}
