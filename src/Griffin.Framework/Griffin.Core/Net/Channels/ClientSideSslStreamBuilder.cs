using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Builder used to create SslStreams for client side applications.
    /// </summary>
    public class ClientSideSslStreamBuilder : ISslStreamBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonName">the domain name of the server that you are connecting to</param>
        public ClientSideSslStreamBuilder(string commonName)
        {
            CommonName = commonName;
            Protocols = SslProtocols.Default;
        }

        /// <summary>
        /// Typically the domain name of the server that you are connecting to.
        /// </summary>
        public string CommonName { get; private set; }

        /// <summary>
        /// Leave empty to use the server certificate
        /// </summary>
        public X509Certificate Certificate { get; private set; }

        /// <summary>
        /// Allowed SSL protocols
        /// </summary>
        public SslProtocols Protocols { get; set; }

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
                X509CertificateCollection certificates = null;
                if (Certificate != null)
                {
                    certificates = new X509CertificateCollection(new[] { Certificate });
                }

                stream.AuthenticateAsClient(CommonName, certificates, Protocols, false);
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

        protected virtual bool OnRemoteCertifiateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return (Certificate != null && certificate == null) || (Certificate == null && certificate != null);
        }
    }
}