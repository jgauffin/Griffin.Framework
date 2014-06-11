using System;
using System.Collections.Generic;
using System.IO;
using Griffin.Net.Protocols.Http.BodyDecoders.Mono;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http.BodyDecoders
{
    /// <summary>
    ///     Decodes multipart files.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Uses <c>Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)</c> as temporary storage. The folder
    ///         <c>/var/tmp/</c> is used if the special folder is not found.
    ///     </para>
    /// </remarks>
    public class MultipartDecoder : IBodyDecoder
    {
        /// <summary>
        ///     form-data
        /// </summary>
        public const string FormData = "form-data";

        /// <summary>
        ///     Returns <c>multipart/form-data</c>
        /// </summary>
        public const string MimeType = "multipart/form-data";


        /// <summary>
        ///     All content types that the decoder can parse.
        /// </summary>
        /// <returns>A collection of all content types that the decoder can handle.</returns>
        public IEnumerable<string> ContentTypes
        {
            get { return new[] {MimeType, FormData}; }
        }

        #region IBodyDecoder Members

        /// <summary>
        /// Decode body stream
        /// </summary>
        /// <param name="message">Contains the body to decode. Expectes the body to be in format <c>multipart/form-data</c></param>
        /// <returns>
        ///   <c>true</c> if the body was decoded; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">MultipartDecoder expects requests of type 'HttpRequest'.</exception>
        /// <exception cref="DecoderFailureException">
        /// Missing boundary in content type:  + contentType
        /// or
        /// or
        /// </exception>
        /// <exception cref="System.FormatException">Missing boundary in content type.</exception>
        public bool Decode(IHttpRequest message)
        {
            if (!message.ContentType.StartsWith(MimeType))
                return false;
            var msg = message as HttpRequest;
            if (msg == null)
                throw new NotSupportedException("MultipartDecoder expects requests of type 'HttpRequest'.");

            var contentType = new HttpHeaderValue(message.Headers["Content-Type"]);

            //multipart/form-data, boundary=AaB03x
            var boundry = contentType.Parameters.Get("boundary");
            if (boundry == null)
                throw new DecoderFailureException("Missing boundary in content type: " + contentType);

            var multipart = new HttpMultipart(message.Body, boundry.Value, message.ContentCharset);

            var form = msg.Form;
            /*
            FileStream stream1 = new FileStream("C:\\temp\\mimebody.tmp", FileMode.Create);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream1.Write(bytes, 0, bytes.Length);
            stream1.Flush();
            stream1.Close();
            */

            HttpMultipart.Element element;
            while ((element = multipart.ReadNextElement()) != null)
            {
                if (string.IsNullOrEmpty(element.Name))
                    throw new DecoderFailureException(string.Format("Missing value name.\nElement: {0}", element));

                if (!string.IsNullOrEmpty(element.Filename))
                {
                    if (string.IsNullOrEmpty(element.ContentType))
                        throw new DecoderFailureException(string.Format("Value '{0}' lacks a content type.",
                            element.Name));

                    // Read the file data
                    var buffer = new byte[element.Length];
                    message.Body.Seek(element.Start, SeekOrigin.Begin);
                    message.Body.Read(buffer, 0, (int) element.Length);

                    // Generate a filename
                    var originalFileName = element.Filename;
                    var internetCache = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);

                    // if the internet path doesn't exist, assume mono and /var/tmp
                    var path = string.IsNullOrEmpty(internetCache)
                        ? Path.Combine("var", "tmp")
                        : Path.Combine(internetCache.Replace("\\\\", "\\"), "tmp");

                    element.Filename = Path.Combine(path, Math.Abs(element.Filename.GetHashCode()) + ".tmp");

                    // If the file exists generate a new filename
                    while (File.Exists(element.Filename))
                        element.Filename = Path.Combine(path, Math.Abs(element.Filename.GetHashCode() + 1) + ".tmp");

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    File.WriteAllBytes(element.Filename, buffer);

                    var file = new HttpFile
                    {
                        Name = element.Name,
                        OriginalFileName = originalFileName,
                        ContentType = element.ContentType,
                        TempFileName = element.Filename
                    };
                    msg.Files.Add(file);
                }
                else
                {
                    var buffer = new byte[element.Length];
                    message.Body.Seek(element.Start, SeekOrigin.Begin);
                    message.Body.Read(buffer, 0, (int) element.Length);

                    form.Add(Uri.UnescapeDataString(element.Name), message.ContentCharset.GetString(buffer));
                }
            }


            return true;
        }

        #endregion
    }
}