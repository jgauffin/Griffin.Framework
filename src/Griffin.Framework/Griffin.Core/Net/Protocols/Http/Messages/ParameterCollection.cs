using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    ///     Collection of parameters in a HTTP header.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Most of the HTTP headers can have parameters in the header value. typically they are stored as
    ///         <code>TheHeaderValue;and=some,parameters=true</code>
    ///     </para>
    /// </remarks>
    public class ParameterCollection : IParameterCollection
    {
        private readonly Dictionary<string, IParameter> _items =
            new Dictionary<string, IParameter>(StringComparer.OrdinalIgnoreCase);

        #region IParameterCollection Members

        /// <summary>
        ///     Gets number of parameters.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        ///     Gets last value of an parameter.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>String if found; otherwise <c>null</c>.</returns>
        public string this[string name]
        {
            get
            {
                IParameter parameter;
                return _items.TryGetValue(name, out parameter) ? parameter.Last() : null;
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<IParameter> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        /// <summary>
        /// Parse string
        /// </summary>
        /// <param name="value">contains "a=b,c=d" etc</param>
        public static ParameterCollection Parse(string value)
        {
            var collection = new ParameterCollection();
            Parse(value, collection);
            return collection;
        }

        /// <summary>
        /// Parse string
        /// </summary>
        /// <param name="value">contains "a=b,c=d" etc</param>
        /// <param name="target">Collection to fill with the values</param>
        public static void Parse(string value, ParameterCollection target)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (target == null) throw new ArgumentNullException("target");

            var index = 0;
            var lastCh = char.MinValue;

            var name = "";
            var oldPos = 0;
            bool gotEquals = false;
            while (index < value.Length)
            {
                var ch = value[index];
                switch (ch)
                {
                    case '=':
                        if (gotEquals)
                            break;

                        gotEquals = true;
                        if (lastCh != '\\')
                        {
                            name = value.Substring(oldPos, index - oldPos).Trim(' ');
                            oldPos = index + 1;
                        }
                        break;
                    case ';':
                        gotEquals = false;
                        if (lastCh != '\\')
                        {
                            target.Add(name, value.Substring(oldPos, index - oldPos).Trim(' ', '"'));
                            name = "";
                            oldPos = index + 1;
                        }
                        break;
                }
                lastCh = value[index];
                ++index;
            }

            if (name != "")
            {
                target.Add(name, value.Substring(oldPos).Trim(' ', '"'));
            }
        }


        /// <summary>
        ///     Add a query string parameter.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Value</param>
        public void Add(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            IParameter parameter;
            if (!_items.TryGetValue(name, out parameter))
            {
                parameter = new Parameter(name, value);
                _items.Add(name, parameter);
            }
            else
                parameter.Add(value);
        }

        /// <summary>
        ///     Checks if the specified parameter exists
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>;</returns>
        public bool Exists(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return _items.ContainsKey(name);
        }

        /// <summary>
        ///     Get a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IParameter Get(string name)
        {
            IParameter value;
            return _items.TryGetValue(name, out value) ? value : null;
        }

        #endregion

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

        /// <summary>
        ///     Remove all item
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Join(",", _items.Select(x => string.Format("{0}={1}", x.Key, x.Value)));
        }
    }
}