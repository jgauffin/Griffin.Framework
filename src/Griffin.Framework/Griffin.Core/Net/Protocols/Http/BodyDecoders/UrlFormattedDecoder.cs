using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http.BodyDecoders
{
    /// <summary>
    /// Decodes URL encoded values.
    /// </summary>
    public class UrlFormattedDecoder : IBodyDecoder
    {
        /// <summary>
        /// The mimetype that this decoder is for.
        /// </summary>
        /// <value>application/x-www-form-urlencoded</value>
        public const string MimeType = "application/x-www-form-urlencoded";

        /// <summary>
        /// All content types that the decoder can parse.
        /// </summary>
        /// <returns>A collection of all content types that the decoder can handle.</returns>
        public IEnumerable<string> ContentTypes
        {
            get { return new[] { MimeType }; }
        }

        #region IBodyDecoder Members

        /// <summary>
        /// Decode body stream
        /// </summary>
        /// <param name="message">Contains the body to decode.</param>
        /// <exception cref="FormatException">Body format is invalid for the specified content type.</exception>
        public bool Decode(IHttpRequest message)
        {
            if (message == null) throw new ArgumentNullException("message");
            var msg = message as HttpRequest;
            if (msg == null)
                throw new NotSupportedException("Body decoders expect messages of type 'HttpRequest'.");

            if (!message.ContentType.StartsWith(MimeType, StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var decoder = new UrlDecoder();
                decoder.Parse(new StreamReader(message.Body), msg.Form);
                message.Body.Position = 0;
                return true;
            }
            catch (ArgumentException err)
            {
                throw new FormatException(err.Message, err);
            }
        }

        #endregion
    }
}
