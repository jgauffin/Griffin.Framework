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
    public class HttpHeaderValue
    {
        private readonly ParameterCollection _parameters;

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
                _parameters= ParameterCollection.Parse(value.Substring(pos + 1));
            }
                
        }

        public string Value { get; set; }

        public IParameterCollection Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (_parameters.Count > 0)
                return Value + ";" + _parameters;
            return Value;

        }
    }
}