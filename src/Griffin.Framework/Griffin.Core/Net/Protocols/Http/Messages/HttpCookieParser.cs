using System;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Parses a request cookie header value.
    /// </summary>
    /// <remarks>This class is not thread safe.</remarks>
    public class HttpCookieParser
    {
        private string _headerValue;
        private HttpCookieCollection<HttpCookie> _cookies;
        private int _index;
        private string _cookieName = "";
        private Action _parserMethod;
        private string _cookieValue = "";

        private char Current
        {
            get
            {
                if (_index >= _headerValue.Length)
                    return char.MinValue;

                return _headerValue[_index];
            }
        }

        /// <summary>
        /// end of cookie string?
        /// </summary>
        protected bool IsEOF
        {
            get { return _index >= _headerValue.Length; }
        }

        /// <summary>
        /// Parse state method, remove all white spaces before the cookie name
        /// </summary>
        protected void Name_Before()
        {
            while (char.IsWhiteSpace(Current))
            {
                MoveNext();
            }

            _parserMethod = Name;
        }

        /// <summary>
        /// Read cookie name until white space or equals are found
        /// </summary>
        protected virtual void Name()
        {
            while (!char.IsWhiteSpace(Current) && Current != '=')
            {
                _cookieName += Current;
                MoveNext();
            }

            _parserMethod = Name_After;
        }

        /// <summary>
        /// Remove all white spaces until colon is found
        /// </summary>
        protected virtual void Name_After()
        {
            while (char.IsWhiteSpace(Current) || Current == ':')
            {
                MoveNext();
            }

            _parserMethod = Value_Before;
        }

        /// <summary>
        /// Determine if the cookie value is quoted or regular.
        /// </summary>
        protected virtual void Value_Before()
        {
            if (Current == '"')
                _parserMethod = Value_Quoted;
            else
                _parserMethod = Value;

            MoveNext();
        }

        /// <summary>
        /// Read cookie value
        /// </summary>
        private void Value()
        {
            while (Current != ';' && !IsEOF)
            {
                _cookieValue += Current;
                MoveNext();
            }

            _parserMethod = Value_After;
        }

        /// <summary>
        /// Read cookie value quoted
        /// </summary>
        private void Value_Quoted()
        {
            MoveNext(); // skip '"'

            var last = char.MinValue;
            while (Current != '"' && !IsEOF)
            {
                if (Current == '"' && last == '\\')
                {
                    _cookieValue += '#';
                    MoveNext();
                }
                else
                {
                    _cookieValue += Current;
                }

                last = Current;
                MoveNext();
            }

            _parserMethod = Value_After;
        }


        private void Value_After()
        {
            OnCookie(_cookieName, _cookieValue);
            _cookieName = "";
            _cookieValue = "";
            while (char.IsWhiteSpace(Current) || Current == ';')
            {
                MoveNext();
            }

            _parserMethod = Name_Before;
        }

        private void OnCookie(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (name == "") return; // ignore empty cookie names as defined in rfc 6265 http://tools.ietf.org/html/rfc6265

            _cookies.Add(new HttpCookie(name, value));
        }

        private void MoveNext()
        {
            if (!IsEOF)
                ++_index;
        }

        /// <summary>
        /// Parse cookie string
        /// </summary>
        /// <returns>A generated cookie collection.</returns>
        public HttpCookieCollection<HttpCookie> Parse(string value)
        {
            if (value == null) throw new ArgumentNullException("value");

            _index = 0;
            _cookieValue = "";
            _cookieName = "";
            _headerValue = value;
            _cookies = new HttpCookieCollection<HttpCookie>();
            _parserMethod = Name_Before;

            while (!IsEOF)
            {
                _parserMethod();
            }

            OnCookie(_cookieName, _cookieValue);
            return _cookies;
        }
    }
}