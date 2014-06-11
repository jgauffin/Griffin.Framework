namespace Griffin.Net.Protocols.Http
{
    /// <summary>
    /// A message where the form have been decoded (or can be encoded for outbound messages)
    /// </summary>
    public interface IHttpMessageWithForm : IHttpMessage
    {
        /// <summary>
        /// Decoded HTTP body (along with <see cref="Files"/>)
        /// </summary>
        IParameterCollection Form { get; }

        /// <summary>
        /// Files from a multipart body
        /// </summary>
        IHttpFileCollection Files { get; }
    }
}