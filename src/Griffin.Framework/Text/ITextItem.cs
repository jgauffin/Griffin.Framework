namespace Griffin.Framework.Text
{
    /// <summary>
    /// Represents a single text.
    /// </summary>
    public interface ITextItem
    {
        /// <summary>
        /// Gets identifier. Might be long. You have to hash yourself (and manage hash collisions).
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets metadata name (if the item that the identifier represents have additional metadata)
        /// </summary>
        string MetadataName { get; }

        /// <summary>
        /// Gets localized string.
        /// </summary>
        string Text { get; }

    }
}