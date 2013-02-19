using System.Threading;

namespace Griffin.Framework.Text
{
    /// <summary>
    /// Used to provide translations for <see cref="Localize"/>.
    /// </summary>
    /// <remarks>Since this repository is assigned as a singleton it has to be able to manage for instance database connections in some way
    /// <para><see cref="Thread.CurrentCulture"/> determines language used to return string.</para>
    /// </remarks>
    public interface ITextDataSource
    {
        /// <summary>
        /// Get translation
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <returns>null = do not exist; "" = not translated; any other value should be the correct text.</returns>
        string Get(string identifier);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="identifier">Identifier. Might be long. You have to hash yourself (and manage hash collisions)</param>
        /// <param name="metadataName">Name of the meta.</param>
        /// <returns>null = do not exist; "" = not translated; any other value should be the correct text.</returns>
        string Get(string identifier, string metadataName);
    }
}