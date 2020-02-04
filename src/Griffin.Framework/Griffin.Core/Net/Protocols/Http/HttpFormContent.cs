using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    public class HttpFormContent
    {
        private HttpFileCollection _files;
        private ParameterCollection _form;

        /// <summary>
        ///     Submitted files
        /// </summary>
        public HttpFileCollection Files
        {
            get => _files ?? (_files = new HttpFileCollection());
            set => _files = value;
        }

        /// <summary>
        ///     Submitted form items
        /// </summary>
        public ParameterCollection Form
        {
            get => _form ?? (_form = new ParameterCollection());
            set => _form = value;
        }
    }
}