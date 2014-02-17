using System.IO;
using Griffin.Net.Protocols.Stomp.Frames;
using Griffin.Net.Protocols.Stomp.Frames.Server;

namespace Griffin.Net.Protocols.Stomp
{
    public static class FrameExtensions
    {
        public static IFrame CreateAck(this IFrame request)
        {
            var frame = new BasicFrame("ACK");

            var trans = request.Headers["transaction"];
            if (trans != null)
                frame.Headers["transaction"] = trans;

            var id = request.Headers["ack"];
            if (id != null)
                frame.Headers["id"] = id;


            var receipt = request.Headers["receipt"];
            if (receipt != null)
                frame.Headers["receipt"] = receipt;

            return frame;
        }

        public static IFrame CreateError(this IFrame request, string errorDescription)
        {
            var frame = new StompError(errorDescription);

            var id = request.Headers["message-id"] ?? request.Headers["id"];
            if (id != null)
                frame.Headers["org-message-id"] = id;

            var receipt = request.Headers["receipt"];
            if (receipt != null)
                frame.Headers["receipt"] = receipt;

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.WriteLine("Message headers:");
            sw.WriteLine();
            foreach (var header in request.Headers)
            {
                sw.Write("{0}:{1}\n", header.Key, header.Value);
            }

            sw.WriteLine();

            if (request.Body != null)
            {
                sw.WriteLine("Body:");
                sw.Flush();
                request.Body.CopyTo(ms);
            }

            frame.Headers["diagnostics-information"] = "Original message is included as body";
            frame.Body = ms;
            frame.Body.Position = 0;
            return frame;
        }

        public static IFrame CreateNack(this IFrame request, string errorDescription)
        {
            var frame = new BasicFrame("NACK");

            var trans = request.Headers["transaction"];
            if (trans != null)
                frame.Headers["transaction"] = trans;

            var id = request.Headers["ack"];
            if (id != null)
                frame.Headers["id"] = id;


            var receipt = request.Headers["receipt"];
            if (receipt != null)
                frame.Headers["receipt"] = receipt;

            return frame;
        }
    }
}