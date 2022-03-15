using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteCrawler.Infrastructure.extensions
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Combines a relative path to an existing Uri
        /// </summary>
        /// <param name="parentUrl"></param>
        /// <param name="childUrl"></param>
        /// <returns></returns>
        public static Uri Combine(this string parentUrl, string childUrl)
        {
            var parentUri = new Uri(parentUrl);
            var frags = new List<string>();

            if (!string.IsNullOrEmpty(parentUri.PathAndQuery))
            {
                frags.AddRange(parentUri.PathAndQuery.Split('/'));
            }
            frags.AddRange(childUrl.Split('/'));

            var nonEmptyFrags = frags
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var newChildUrl = string.Join("/", nonEmptyFrags);

            var builder = new UriBuilder();
            builder.Scheme = parentUri.Scheme;
            builder.Port = parentUri.Port;
            builder.Host = parentUri.Host;

            return new Uri(builder.Uri, newChildUrl);
        }

        /// <summary>
        /// Returns the container page of a link
        /// E.g. http://www.hello/foo/bar.html produces http://www.hello/foo/
        /// Inspired by https://stackoverflow.com/a/510243/2989655
        /// </summary>
        /// <param name="parentUri">Absolute path to a web resource</param>
        /// <returns>The path to the container page</returns>
        public static string GetParentUriString(this Uri parentUri)
        {
            var builder = new UriBuilder();
            builder.Scheme = parentUri.Scheme;
            builder.Port = parentUri.Port;
            builder.Host = parentUri.Host;
            builder.Path = parentUri.AbsolutePath;
            var parentUriWithoutQuery = builder.Uri;
            return parentUriWithoutQuery
                .AbsoluteUri
                .Remove(parentUriWithoutQuery.AbsoluteUri.Length - parentUriWithoutQuery.Segments.Last().Length);
        }
    }
}