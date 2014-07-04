using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    /// Wrapper around the <see cref="AdoNetEntityEnumerator{T}"/> which does the actual work.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to convert the rows to</typeparam>
    public class AdoNetEntityEnumerable<TEntity> : IEnumerable<TEntity>
    {
        private bool _handedOut;
        private readonly AdoNetEntityEnumerator<TEntity> _enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetEntityEnumerable{TEntity}"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="ownsConnection">if set to <c>true</c> [owns connection].</param>
        /// <exception cref="System.ArgumentNullException">
        /// command
        /// or
        /// reader
        /// or
        /// mapper
        /// </exception>
        public AdoNetEntityEnumerable(IDbCommand command, IDataReader reader, IEntityMapperBase<TEntity> mapper, bool ownsConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (reader == null) throw new ArgumentNullException("reader");
            if (mapper == null) throw new ArgumentNullException("mapper");

            _enumerator = new AdoNetEntityEnumerator<TEntity>(command, reader, mapper, ownsConnection);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="InvalidOperationException">May only traverse AdoNetEntityEnumerable once</exception>
        public IEnumerator<TEntity> GetEnumerator()
        {
            //InvalidOperation is defined in base class
            if (_handedOut)
                throw new InvalidOperationException("May only traverse AdoNetEntityEnumerable once.");
            _handedOut = true;

            return _enumerator;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}