using FluentAssertions;
using Griffin.Net.Protocols.Stomp.Broker;
using Griffin.Net.Protocols.Stomp.Frames;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Stomp.Broker
{
    public class FrameExtensionsTests
    {
        [Fact]
        public void do_not_create_receipt_if_the_header_is_missing()
        {
            var frame = new BasicFrame("SEND");

            var response = frame.CreateReceiptIfRequired();

            response.Should().BeNull();
        }

        [Fact]
        public void create_receipt_with_correct_id()
        {
            var frame = new BasicFrame("SEND");
            frame.Headers["receipt"] = "1";

            var response = frame.CreateReceiptIfRequired();

            response.Should().NotBeNull();
            response.Headers["receipt-id"] = "1";
        }

    }
}
