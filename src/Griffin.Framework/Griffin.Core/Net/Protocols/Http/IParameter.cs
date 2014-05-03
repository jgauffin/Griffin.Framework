using System.Collections.Generic;

namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// Parameter in <see cref="IParameterCollection"/>
    /// </summary>
    public interface IParameter : IEnumerable<string>
    {
        /// <summary>
        /// Gets *last* value.
        /// </summary>
        /// <remarks>
        /// Parameters can have multiple values. This property will always get the last value in the list.
        /// </remarks>
        /// <value>String if any value exist; otherwise <c>null</c>.</value>
        string Value { get; }

        /// <summary>
        /// Gets or sets name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get one of the values.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string this[int index] { get; }

        /// <summary>
        /// Get number of values
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Add a new parameter value
        /// </summary>
        /// <param name="value">Value to add</param>
        void Add(string value);
    }
}