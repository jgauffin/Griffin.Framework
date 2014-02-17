using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used to build SSL streams for server side applications.
    /// </summary>
    public class ServerSideSslStreamBuilder : ISslStreamBuilder
    {
        public ServerSideSslStreamBuilder(X509Certificate certificate)
        {
            if (certificate == null) throw new ArgumentNullException("certificate");
            Certificate = certificate;
            Protocols = SslProtocols.Default;
        }

        /// <summary>
        /// Build a new SSL steam.
        /// </summary>
        /// <param name="channel">Channel which will use the stream</param>
        /// <param name="socket">Socket to wrap</param>
        /// <returns>Stream which is ready to be used (must have been validated)</returns>
        public SslStream Build(ITcpChannel channel, Socket socket)
        {
            var ns = new NetworkStream(socket);
            var stream = new SslStream(ns, true, OnRemoteCertifiateValidation);

            try
            {
                stream.AuthenticateAsServer(Certificate, UseClientCertificate, Protocols, CheckCertificateRevocation);
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