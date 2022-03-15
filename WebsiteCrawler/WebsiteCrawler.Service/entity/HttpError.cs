using System.Net;

namespace WebsiteCrawler.Service.entity
{
    /// <summary>
    /// Records an error while trying to download the content of a link
    /// </summary>
    internal class HttpError
    {
        /// <summary>
        /// Gets/sets the Http status code
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// Gets/sets the URL of the web page when the error was encountered
        /// </summary>
        public string Url { get; set; }
    }
}