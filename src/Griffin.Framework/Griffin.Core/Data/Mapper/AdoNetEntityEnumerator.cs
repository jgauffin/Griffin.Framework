using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Griffin.Data.Mapper
{
    /// <summary>
    ///     Traverses a recordset returned from a <c>IDbCommand</c> and converts each row to an entity.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to convert to.</typeparam>
    /// <remarks>
    ///     <para>
    ///         Will not do any conversions until the <see cref="Current" /> property is being called. Hence you can call the
    ///         <see cref="MoveNext" />
    ///         method any amount of times that you need without the mapping being used.
    ///     </para>
    ///     <para></para>
    /// </remarks>
    public class AdoNetEntityEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private readonly IEntityMapperBase _mapper;
        private readonly bool _ownsConnection;
        private IDbCommand _command;
        private TEntity _current;
        private IDataReader _reader;
        private bool _hasRead = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNetEntityEnumerator{TEntity}"/> class.
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
        public AdoNetEntityEnumerator(IDbCommand command, IDataReader reader, IEntityMapperBase mapper, bool ownsConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (reader == null) throw new ArgumentNullException("reader");
            if (mapper == null) throw new ArgumentNullException("mapper");

            _command = command;
            _reader = reader;
            _mapper = mapper;
            _ownsConnection = ownsConnection;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
            var connection = _command.Connection;
            if (_command != null)
            {
                _command.Dispose();
                _command = null;
            }

            if (connection != null && _ownsConnection)
            {
                connection.Dispose();
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
        ///     the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            _current = default(TEntity);
            _hasRead = true;
            return _reader.Read();
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            throw new InvalidOperationException("The collection was modified after the enumerator was created");
        }

        /// <summary>
        ///     Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        ///     The element in the collection at the current position of the enumerator.
        /// </returns>
        public TEntity Current
        {
            get
            {
                if (!_hasRead)
                    throw new InvalidOperationException("You have to invoke MoveNext() first.");
                _hasRead = true;

                if (!EqualityComparer<TEntity>.Default.Equals(_current, default(TEntity)))
                    return _current;

                _current = (TEntity) _mapper.Create(_reader);
                _mapper.Map(_reader, _current);
                return _current;
            }
        }

        /// <summary>
        ///     Gets the current element in the collection.
        /// </summary>
        /// <returns>
        ///     The current element in the collection.
        /// </returns>
        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}