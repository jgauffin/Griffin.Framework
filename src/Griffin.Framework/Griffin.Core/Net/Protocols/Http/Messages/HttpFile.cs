using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// A HTTP file in a request.
    /// </summary>
    /// <remarks>The temporary file will be deleted when the request/response ends.</remarks>
    public class HttpFile : IHttpFile, IDisposable
    {
        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            File.Delete(TempFileName);
        }

        #endregion

        #region IHttpFile Members

        /// <summary>
        /// Gets or sets form element name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets client side file name
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets mime content type
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets full path to local file
        /// </summary>
        public string TempFileName { get; set; }

        /// <summary>
        /// Move the file
        /// </summary>
        /// <param name="destination">Destination (path and file name)</param>
        public void Move(string destination)
        {
            File.Move(TempFileName, destination);
        }

        /// <summary>
        /// Delete file
        /// </summary>
        public void Delete()
        {
            File.Delete(TempFileName);
        }

        #endregion
    }
}