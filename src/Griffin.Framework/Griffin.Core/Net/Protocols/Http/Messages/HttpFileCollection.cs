using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Collection of files in a HTTP request.
    /// </summary>
    public class HttpFileCollection : IEnumerable<HttpFile>
    {
        private readonly List<HttpFile> _files = new List<HttpFile>();

        #region IHttpFileCollection Members

        /// <summary>
        /// Get a file
        /// </summary>
        /// <param name="name">Name in form</param>
        /// <returns>File if found; otherwise <c>null</c>.</returns>
        public HttpFile this[string name]
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
        public void Add(HttpFile file)
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

		  /// <summary>
		  ///     Returns an enumerator that iterates through the collection.
		  /// </summary>
		  /// <returns>
		  ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		  /// </returns>
		  /// <filterpriority>1</filterpriority>
		  public IEnumerator<HttpFile> GetEnumerator()
		  {
			  return _files.GetEnumerator();
		  }

		  /// <summary>
		  ///     Returns an enumerator that iterates through a collection.
		  /// </summary>
		  /// <returns>
		  ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		  /// </returns>
		  IEnumerator IEnumerable.GetEnumerator()
		  {
			  return GetEnumerator();
		  }

        #endregion
    }
}