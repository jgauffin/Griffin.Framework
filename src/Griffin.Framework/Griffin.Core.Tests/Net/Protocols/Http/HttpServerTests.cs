using System;
using System.CodeDom;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;
using Xunit;

namespace Griffin.Core.Tests.Net.Protocols.Http
{
    public class HttpServerTests: HttpMiddleware
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
            var config = new HttpConfiguration {Port = 0};
            config.ContentSerializers.Clear();
            config.Pipeline.Register(this);

            var sut = new HttpServer(config);
            sut.RunAsync(IPAddress.Loopback,CancellationToken.None);

            var request = WebRequest.CreateHttp("http://localhost:" + sut.LocalPort);
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            request.Headers.Add("SOAPAction", "urn:BVTAFService:PostMessages");
            request.ContentType = "text/xml; charset=utf-8";
            request.Method = "POST";
            var formattedMessage = string.Format(Wrapper, "<fake><soap /></fake>");
            var buf = Encoding.UTF8.GetBytes(formattedMessage);
            request.GetRequestStream().Write(buf, 0, buf.Length);

            HttpRequest msg;
            var response = request.GetResponse();

            response.Should().NotBeNull();
        }

        public override Task Process(HttpContext context, Func<Task> next)
        {
            context.Response.StatusCode = 204;
            return Task.CompletedTask;
        }
    }
}
