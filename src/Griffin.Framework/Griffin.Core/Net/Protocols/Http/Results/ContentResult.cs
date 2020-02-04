using System.IO;

namespace Griffin.Net.Protocols.Http.Results
{
    public class ContentResult : HttpResult
    {
        public ContentResult(string contentType, string stringContent)
        {
            ContentType = contentType;
            StringContent = stringContent;
        }
        public ContentResult(string contentType, Stream content)
        {
            ContentType = contentType;
            BinaryContent = content;
        }

        public string ContentType { get; private set; }
        public string StringContent { get; private set; }
        public Stream BinaryContent { get; private set; }
    }
}