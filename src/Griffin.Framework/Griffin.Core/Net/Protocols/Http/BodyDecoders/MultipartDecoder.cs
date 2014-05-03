using System;
using System.Collections.Generic;
using System.IO;
using Griffin.Net.Protocols.Http.BodyDecoders.Mono;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http.BodyDecoders
{
    /// <summary>
    /// Decodes multipart files.
    /// </summary>
    public class MultipartDecoder : IBodyDecoder
    {
        /// <summary>
        /// form-data
        /// </summary>
        public const string FormData = "form-data";

        /// <summary>
        /// multipart/form-data
        /// </summary>
        public const string MimeType = "multipart/form-data";


        /// <summary>
        /// All content types that the decoder can parse.
        /// </summary>
        /// <returns>A collection of all content types that the decoder can handle.</returns>
        public IEnumerable<string> ContentTypes
        {
            get { return new[] { MimeType, FormData }; }
        }

        #region IBodyDecoder Members

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
                throw new FormatException("Missing boundary in content type.");

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
                    throw new FormatException("Error parsing request. Missing value name.\nElement: " + element);

                if (!string.IsNullOrEmpty(element.Filename))
                {
                    if (string.IsNullOrEmpty(element.ContentType))
                        throw new FormatException("Error parsing request. Value '" + element.Name +
                                                  "' lacks a content type.");

                    // Read the file data
                    var buffer = new byte[element.Length];
                    message.Body.Seek(element.Start, SeekOrigin.Begin);
                    message.Body.Read(buffer, 0, (int)element.Length);

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
                    message.Body.Read(buffer, 0, (int)element.Length);

                    form.Add(Uri.UnescapeDataString(element.Name), message.ContentCharset.GetString(buffer));
                }
            }


            return true;
        }

        #endregion
    }
}