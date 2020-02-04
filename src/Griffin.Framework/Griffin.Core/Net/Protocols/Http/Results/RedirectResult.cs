namespace Griffin.Net.Protocols.Http.Results
{
    public class RedirectResult : HttpResult
    {
        /// <summary>
        /// Can be a complete uri, an absolute path or a relative path.
        /// </summary>
        public string Location { get; set; }
    }
}