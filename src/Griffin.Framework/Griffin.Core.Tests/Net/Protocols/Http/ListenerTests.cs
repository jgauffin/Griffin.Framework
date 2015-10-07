using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Griffin.Core.Tests.Data.Mapper;
using Griffin.Net.Protocols.Http;
using Xunit;
using HttpListener = Griffin.Net.Protocols.Http.HttpListener;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class ListenerTests
    {
        private const string Wrapper = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
		<MessageArray xmlns=""urn:BVTAFMessageInbox"">
			{0}
		</MessageArray>
	</s:Body>
</s:Envelope>";

        [Fact]
        public void work_with_custom_header()
        {
            var sut = new HttpListener();
            sut.Start(IPAddress.Loopback, 0);
            var request = WebRequest.CreateHttp("http://localhost:" + sut.LocalPort);
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
            request.Headers.Add("SOAPAction", "urn:BVTAFService:PostMessages");
            request.ContentType = "text/xml; charset=utf-8";
            request.Method = "POST";
            var formattedMessage = string.Format(Wrapper, "<fake><soap /></fake>");
            var buf = Encoding.UTF8.GetBytes(formattedMessage);
            request.GetRequestStream().Write(buf, 0, buf.Length);

            HttpRequest msg;
            sut.MessageReceived += (channel, message) =>
            {
                msg = (HttpRequest) message;
                channel.Send(msg.CreateResponse());
            };
            var response = request.GetResponse();


        }
    }
}
