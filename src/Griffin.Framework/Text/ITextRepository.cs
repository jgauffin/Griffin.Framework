using Griffin.Framework.Data;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// A more complete repository
    /// </summary>
    /// <remarks></remarks>
    public interface ITextRepository : ITextDataSource
    {
        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="text">The text.</param>
        void Create(string identifier, string text);

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="metadataName">Name of the metadata.</param>
        /// <param name="text">The text.</param>
        void Create(string identifier, string metadataName, string text);

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="text">New text.</param>
        void Edit(string identifier, string text);

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="metadataName">Name of the metadata.</param>
        /// <param name="text">New text.</param>
        void Edit(string identifier, string metadataName, string text);

        /// <summary>
        /// Remove text (and all metadata)
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        void Remove(string identifier);

        /// <summary>
        /// Finds all matching texts
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Paged items</returns>
        PagedResult<ITextItem> Find(string filter, int pageNumber, int pageSize);

        /// <summary>
        /// Get items which has not been translated
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Paged items</returns>
        /// <remarks>
        /// the <c>Text</c> property contains the default language string (determined by <see cref="Localize.SourceCulture" />)
        /// </remarks>
        PagedResult<ITextItem> FindNotTranslated(int pageNumber, int pageSize);

    }
}