namespace Griffin.Net.Protocols.Http.Serializers
{
    /// <summary>
    /// Result from decoders.
    /// </summary>
    // this seems like a hack that the HTTP namespace leaks to here.
    public class FormAndFilesResult
    {
        /// <summary>
        /// Collection of files (or an empty collection)
        /// </summary>
        public IHttpFileCollection Files { get; set; }

        /// <summary>
        /// HTTP form items (or an empty form)
        /// </summary>
        public IParameterCollection Form { get; set; }
    }
}