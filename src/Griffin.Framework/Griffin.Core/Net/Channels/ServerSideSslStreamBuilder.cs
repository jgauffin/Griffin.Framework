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
    /// Used to build SSL streams for server side applications.
    /// </summary>
    public class ServerSideSslStreamBuilder : ISslStreamBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSideSslStreamBuilder"/> class.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <param name="allowedProtocols">Which protocols to support for HTTPS. By default only Tls 1.2 is allowed (most secure)</param>
        /// <exception cref="System.ArgumentNullException">certificate</exception>
        public ServerSideSslStreamBuilder(X509Certificate certificate, SslProtocols allowedProtocols = SslProtocols.Tls12)
        {
            if (certificate == null) throw new ArgumentNullException("certificate");
            Certificate = certificate;
            Protocols = allowedProtocols;
            HandshakeTimeout = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        /// Build a new SSL steam.
        /// </summary>
        /// <param name="channel">Channel which will use the stream</param>
        /// <param name="socket">Socket to wrap</param>
        /// <returns>Stream which is ready to be used (must have been validated)</returns>
        public SslStream Build(IBinaryChannel channel, Socket socket)
        {
            var ns = new NetworkStream(socket);
            var stream = new SslStream(ns, true, OnRemoteCertifiateValidation);

            try
            {
                var t = stream.AuthenticateAsServerAsync(Certificate, UseClientCertificate, Protocols, CheckCertificateRevocation);
                t.Wait(HandshakeTimeout);
                if (t.Status == TaskStatus.WaitingForActivation)
                    throw new InvalidOperationException("Handshake was not completed within the given interval '" + HandshakeTimeout + "'.");
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

        /// <summary>
        /// check if the certificate have been revoked.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }

        /// <summary>
        /// Amount of time to wait for the TSL handshake to complete.
        /// </summary>
        public TimeSpan HandshakeTimeout { get; set; }

        /// <summary>
        /// Allowed protocols
        /// </summary>
        public SslProtocols Protocols { get; set; }

        /// <summary>
        /// The client must supply a certificate
        /// </summary>
        public bool UseClientCertificate { get; set; }

        /// <summary>
        /// Certificate to use in this server.
        /// </summary>
        public X509Certificate Certificate { get; private set; }

        /// <summary>
        /// Works just like the <a href="http://msdn.microsoft.com/en-us/library/system.net.security.remotecertificatevalidationcallback(v=vs.110).aspx">callback</a> for <c>SslStream</c>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslpolicyerrors"></param>
        /// <returns></returns>
        protected virtual bool OnRemoteCertifiateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return !(UseClientCertificate && certificate == null);
        }
    }
}