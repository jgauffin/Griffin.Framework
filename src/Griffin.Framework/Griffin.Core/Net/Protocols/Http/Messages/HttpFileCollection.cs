using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Collection of files in a HTTP request.
    /// </summary>
    public class HttpFileCollection : IHttpFileCollection
    {
        private readonly List<IHttpFile> _files = new List<IHttpFile>();

        #region IHttpFileCollection Members

        /// <summary>
        /// Get a file
        /// </summary>
        /// <param name="name">Name in form</param>
        /// <returns>File if found; otherwise <c>null</c>.</returns>
        public IHttpFile this[string name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException("name");
                return _files.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Gets number of files
        /// </summary>
        public int Count
        {
            get { return _files.Count; }
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="name">Name of the file (form item name)</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return _files.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Add a new file.
        /// </summary>
        /// <param name="file">File to add.</param>
        public void Add(IHttpFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            _files.Add(file);
        }

        /// <summary>
        /// Remove all files from disk.
        /// </summary>
        public void Clear()
        {
            _files.Clear();
        }

        #endregion
    }
}