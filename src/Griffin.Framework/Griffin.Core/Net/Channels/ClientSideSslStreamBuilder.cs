using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     Builder used to create SslStreams for client side applications.
    /// </summary>
    public class ClientSideSslStreamBuilder : ISslStreamBuilder
    {
        /// <summary>
        /// </summary>
        /// <param name="commonName">the domain name of the server that you are connecting to</param>
        public ClientSideSslStreamBuilder(string commonName)
        {
            CommonName = commonName;
            Protocols = SslProtocols.Ssl3 | SslProtocols.Tls;
        }

        /// <summary>
        ///     Leave empty to use the server certificate
        /// </summary>
        public X509Certificate Certificate { get; set; }

        /// <summary>
        ///     Typically the domain name of the server that you are connecting to.
        /// </summary>
        public string CommonName { get; }

        /// <summary>
        ///     Allowed SSL protocols
        /// </summary>
        public SslProtocols Protocols { get; set; }


        public async Task<SslStream> BuildAsync(IBinaryChannel channel, Socket socket)
        {
            return await Build(socket);
        }

        /// <summary>
        ///     Used to validate the certificate that the server have provided.
        /// </summary>
        /// <param name="sender">Server.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="chain">The chain.</param>
        /// <param name="sslpolicyerrors">The sslpolicyerrors.</param>
        /// <returns><c>true</c> if the certificate will be allowed, otherwise <c>false</c>.</returns>
        protected virtual bool OnRemoteCertifiateValidation(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslpolicyerrors)
        {
            return Certificate != null && certificate == null || Certificate == null && certificate != null;
        }

        private async Task<SslStream> Build(Socket socket)
        {
            var ns = new NetworkStream(socket);
            var stream = new SslStream(ns, true, OnRemoteCertifiateValidation);

            try
            {
                X509CertificateCollection certificates = null;
                if (Certificate != null) certificates = new X509CertificateCollection(new[] {Certificate});

                await stream.AuthenticateAsClientAsync(CommonName, certificates, Protocols, false);
            }
            catch (IOException err)
            {
                throw new InvalidOperationException("Failed to authenticate " + socket.RemoteEndPoint, err);
            }
            catch (ObjectDisposedException err)
            {
                throw new InvalidOperationException("Failed to create stream, did client disconnect directly?", err);
            }
            catch (AuthenticationException err)
            {
                throw new InvalidOperationException("Failed to authenticate " + socket.RemoteEndPoint, err);
            }

            return stream;
        }
    }
}