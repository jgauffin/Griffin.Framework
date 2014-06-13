namespace Griffin.Net.Protocols.Http.Serializers
{
    /// <summary>
    /// Result from decoders.
    /// </summary>
    // this seems like a hack that the HTTP namespace leaks to here.
    public class FormAndFilesResult
    {
        public IHttpFileCollection Files { get; set; }
        public IParameterCollection Form { get; set; }
    }
}