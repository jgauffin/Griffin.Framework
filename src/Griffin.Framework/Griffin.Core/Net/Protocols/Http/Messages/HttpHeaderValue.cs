using System;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    ///     Value for a http header
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Created to take care of optional value parameters.
    ///     </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var str =  "application/x-www-form-urlencoded;charset=windows-1250;type=Your.App.Namespace.SomeType-YourApp";
    /// var header = new HttpHeaderValue(str);
    /// var contentType = header.Value;                 // application/x-www-form-urlencoded
    /// var charset = header.Parameters["charset"];     // windows-1250
    /// var type = header.Parameters["type"];           // Your.App.Namespace.SomeType-YourApp
    /// </code>
    /// </example>
    public class HttpHeaderValue
    {
        private readonly ParameterCollection _parameters;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpHeaderValue" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public HttpHeaderValue(string value)
        {
            if (value == null) throw new ArgumentNullException("value");

            var pos = value.IndexOf(';');
            if (pos == -1)
            {
                Value = value;
                _parameters = new ParameterCollection();
            }
            else
            {
                Value = value.Substring(0, pos);
                _parameters = ParameterCollection.Parse(value.Substring(pos + 1));
            }
        }

        /// <summary>
        ///     Gets value (parameters after semicolon is excluded)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     All parameters after the semi colon (if any).
        /// </summary>
        public ParameterCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (_parameters.Count > 0)
                return Value + ";" + _parameters;
            return Value;
        }
    }
}