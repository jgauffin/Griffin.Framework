using System;
using System.IO;

namespace Griffin.Net.Protocols.Stomp.Frames
{
    public class BasicFrame : IFrame
    {
        private HeaderCollection _headers = new HeaderCollection();
        private string _name;
        private Stream _body;

        public BasicFrame(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">name</exception>
        /// <exception cref="System.FormatException">
        ///     Header name may not contain colon, CR or LF.
        ///     or
        ///     Header value may not contain colon, CR or LF.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         If a client or a server receives repeated frame header entries, only the first header entry SHOULD be used as
        ///         the value of header entry. Subsequent values are only used to maintain a history of state changes of the header
        ///         and MAY be ignored. This implementation will IGNORE all subsequent headers
        ///     </para>
        /// </remarks>
        public void AddHeader(string name, string value)
        {
            _headers.Add(name, value);
        }

        public IHeaderCollection Headers
        {
            get { return _headers; }
        }

        public Stream Body
        {
            get { return _body; }
            set
            {
                _body = value;
                if (!_headers.Contains("content-length"))
                    _headers["content-length"] = _body.Length.ToString();
            }
        }

        public int ContentLength
        {
            get
            {
                if (_headers.Contains("content-length"))
                    return int.Parse(_headers["content-length"]);

                if (Body != null)
                    return (int) Body.Length;

                return 0;
            }
            set { _headers["content-length"] = value.ToString(); }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _name = value;
            }
        }
    }
}