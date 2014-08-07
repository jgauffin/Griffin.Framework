using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Griffin.Net.Protocols.Http.Messages;
using Griffin.Net.Protocols.Serializers;
using Griffin.Net.Protocols.Serializers.Mono;

namespace Griffin.Net.Protocols.Http.Serializers
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
    public class MultipartSerializer : IMessageSerializer
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
        /// Content types that this serializer supports.
        /// </summary>
        public string[] SupportedContentTypes { get { return new[] {FormData, MimeType}; }}

        /// <summary>
        /// Serialize an object to the stream.
        /// </summary>
        /// <param name="source">Object to serialize</param>
        /// <param name="destination">Stream that the serialized version will be written to</param>
        /// <param name="contentType">If you include the type name to it after the format name, for instance <c>json;YourApp.DTO.User,YourApp</c></param>
        /// <returns>Content name (will be passed to the <see cref="IMessageSerializer.Deserialize"/> method in the other end)</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public void Serialize(object source, Stream destination, out string contentType)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Deserialize the content from the stream.
        /// </summary>
        /// <param name="contentType">Used to identify the object which is about to be deserialized. Specified by the <c>Serialize()</c> method when invoked in the other end point.</param>
        /// <param name="source">Stream that contains the object to deserialize.</param>
        /// <returns>Created object</returns>
        /// <exception cref="SerializationException">Deserialization failed</exception>
        public object Deserialize(string contentType, Stream source)
        {
            if (!contentType.StartsWith(MimeType))
                return null;

            var result = new FormAndFilesResult()
            {
                Form = new ParameterCollection(),
                Files = new HttpFileCollection()
            };
            var contentTypeHeader = new HttpHeaderValue(contentType);
            var encodingStr = contentTypeHeader.Parameters["charset"];
				var encoding = Encoding.Default;
				if (!string.IsNullOrEmpty(encodingStr)) {
					encoding = Encoding.GetEncoding(encodingStr);
				}

            //multipart/form-data, boundary=AaB03x
            var boundry = contentTypeHeader.Parameters.Get("boundary");
            if (boundry == null)
                throw new DecoderFailureException("Missing boundary in content type: " + contentType);

            var multipart = new HttpMultipart(source, boundry.Value, encoding);
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
                    source.Seek(element.Start, SeekOrigin.Begin);
                    source.Read(buffer, 0, (int)element.Length);

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
                    result.Files.Add(file);
                }
                else
                {
                    var buffer = new byte[element.Length];
                    source.Seek(element.Start, SeekOrigin.Begin);
                    source.Read(buffer, 0, (int)element.Length);

                    result.Form.Add(Uri.UnescapeDataString(element.Name), encoding.GetString(buffer));
                }
            }

            return result;
        }
    }
}