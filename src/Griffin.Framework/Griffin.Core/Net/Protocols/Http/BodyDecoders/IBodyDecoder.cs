namespace Griffin.Net.Protocols.Http.BodyDecoders
{
    /// <summary>
    /// Decodes body stream into the Form/Files properties.
    /// </summary>
    public interface IBodyDecoder
    {
        /// <summary>
        /// Decode body stream
        /// </summary>
        /// <param name="message">Contains the body to decode.</param>
        /// <exception cref="BadRequestException">Body format is invalid for the specified content type.</exception>
        /// <returns><c>true</c> if the body was decoded; otherwise <c>false</c>.</returns>
        bool Decode(IHttpRequest message);
    }
}