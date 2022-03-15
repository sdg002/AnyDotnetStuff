using Dawn;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebsiteCrawler.Infrastructure.entity;
using WebsiteCrawler.Infrastructure.extensions;
using WebsiteCrawler.Infrastructure.interfaces;
using WebsiteCrawler.Service.entity;

namespace WebsiteCrawler.Service
{
    public class SingleThreadedWebSiteCrawler : IWebSiteCrawler
    {
        public const int RetryAttempts = 2;
        private readonly List<HttpError> _errors = new List<HttpError>();
        private readonly IHtmlParser _htmlParser;
        private readonly HttpClient _httpClient;
        private readonly Queue<SearchJob> _jobQueue;
        private readonly ILogger<SingleThreadedWebSiteCrawler> _logger;
        private readonly SortedDictionary<string, WebsiteCrawler.Infrastructure.entity.SearchResult> _searchResults;

        public SingleThreadedWebSiteCrawler(
            ILogger<SingleThreadedWebSiteCrawler> logger,
            IHtmlParser htmlParser,
            HttpClient httpClient)
        {
            this._logger = logger;
            this._htmlParser = htmlParser;
            this._httpClient = httpClient;

            _searchResults = new SortedDictionary<string, WebsiteCrawler.Infrastructure.entity.SearchResult>();
            _jobQueue = new Queue<SearchJob>();
        }

        public async Task<List<SearchResult>> Run(string url, int maxPagesToSearch)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Other");
            _jobQueue.Enqueue(new entity.SearchJob(url, 0));

            for (int pageCount = 0; pageCount < maxPagesToSearch; pageCount++)
            {
                if (_jobQueue.Count == 0)
                {
                    _logger.LogInformation("No more pages to search");
                    break;
                }
                await DiscoverLinks(url);
                _logger.LogInformation($"----Pages searched={pageCount}, Job Queue={_jobQueue.Count}, results={_searchResults.Count}, Errors={_errors.Count}");
            }

            return _searchResults.Values.ToList();
        }

        private static AsyncRetryPolicy<HttpResponseMessage> CreateExponentialBackoffPolicy()
        {
            var unAcceptableResponses = new HttpStatusCode[] { HttpStatusCode.GatewayTimeout, HttpStatusCode.GatewayTimeout };
            return Policy
                .HandleResult<HttpResponseMessage>(resp => unAcceptableResponses.Contains(resp.StatusCode))
                .WaitAndRetryAsync(
                RetryAttempts,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }

        private async Task DiscoverLinks(string startingSite)
        {
            var searchJob = _jobQueue.Dequeue();
            string pageContents = await DownloadPage(searchJob);
            if (pageContents == null)
            {
                return;
            }
            _logger.LogInformation($"Downloaded page:{searchJob.Url}, Content is {pageContents.Length} characters long");
            var links = FindLinksWithinHtml(pageContents);
            _logger.LogInformation($"Found {links.Count} hyperlinks in the page {searchJob.Url}");
            links.ForEach(link =>
            {
                _logger.LogInformation($"Found a link '{link}'");
                var searchResult = new SearchResult
                {
                    ParentPageUrl = searchJob.Url,
                    OriginalLink = link,
                    Level = searchJob.Level + 1
                };
                bool isLinkAcceptable = IsLinkAcceptable(searchJob, searchResult);
                if (!isLinkAcceptable)
                {
                    _logger.LogInformation($"Ignoring link {link}");
                    return;
                }

                if (link.StartsWith("/"))
                {
                    searchResult.AbsoluteLink = UrlExtensions.Combine(startingSite, link);
                    _logger.LogInformation($"Found a child link:'{link}' which is relateive to top level page:{searchResult.AbsoluteLink} under root:{startingSite}");
                }
                else
                {
                    if (searchResult.IsLinkFullyQualified)
                    {
                        searchResult.AbsoluteLink = new Uri(searchResult.OriginalLink);
                    }
                    else
                    {
                        var parentLink = searchJob.Uri.GetParentUriString();
                        var absoluteUri = UrlExtensions.Combine(parentLink, link);
                        _logger.LogInformation($"Found a child link:'{link}' which is relative to container page:{absoluteUri} under parent:{parentLink}");
                        searchResult.AbsoluteLink = absoluteUri;
                    }
                }

                if (
                (searchResult.AbsoluteLink.Host.ToLower() == searchJob.Uri.Host.ToLower()) &&
                (searchResult.AbsoluteLink.PathAndQuery.ToLower() == searchJob.Uri.PathAndQuery.ToLower())
                )
                {
                    _logger.LogInformation($"Not adding child link:{searchResult.AbsoluteLink.ToString()} because it is the same as parent page");
                    return;
                }
                if (_searchResults.ContainsKey(searchResult.AbsoluteLink.ToString()))
                {
                    _logger.LogInformation($"Child link:{searchResult.AbsoluteLink.ToString()} already added to results");
                    return;
                }
                _searchResults.Add(searchResult.AbsoluteLink.ToString(), searchResult);
                _jobQueue.Enqueue(new SearchJob(searchResult.AbsoluteLink.ToString(), searchResult.Level));
                _logger.LogInformation($"Child link:{searchResult.AbsoluteLink} was added to results");
                _logger.LogInformation($"Queue={_jobQueue.Count} Search results={_searchResults.Count}");
            });
        }

        private async Task<string> DownloadPage(SearchJob searchJob)
        {
            if ((searchJob.Uri.Scheme.ToLower() != "http") && (searchJob.Uri.Scheme.ToLower() != "https"))
            {
                return null;
            }

            var retryPolicy = CreateExponentialBackoffPolicy();

            var htmlResponse = await retryPolicy
                .ExecuteAsync(() => _httpClient.GetAsync(searchJob.Url));

            if (!htmlResponse.IsSuccessStatusCode)
            {
                //throw new NotImplementedException("How do we handle errors? Think"); //TODO handle non-sucess response, Polly retry
                _logger.LogError($"Error while downloading page {searchJob}");
                _errors.Add(new HttpError { Url = searchJob.Url, HttpStatusCode = htmlResponse.StatusCode });
                return null;
            }

            var ctype = htmlResponse.Content.Headers.ContentType;
            if (!ctype.MediaType.Contains("text/html"))
            {
                _logger.LogInformation($"Content in url:{searchJob} has content type:{ctype}. This is non-html Ignoring!");
                return null;
            }

            var htmlContent = await htmlResponse.Content.ReadAsStringAsync();
            return htmlContent;
        }

        private List<string> FindLinksWithinHtml(string htmlContent)
        {
            return _htmlParser.GetLinks(htmlContent);
        }

        private bool IsLinkAcceptable(SearchJob searchJob, SearchResult searchResult)
        {
            var childLink = searchResult.OriginalLink;
            if (string.IsNullOrEmpty(childLink))
            {
                return false;
            }
            var frags = childLink.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (!frags.Any())
            {
                return false;
            }

            if (childLink.StartsWith("#"))
            {
                //Book marks are not wanted
                return false;
            }

            if (childLink.StartsWith("mailto:"))
            {
                //Email links are not wanted
                return false;
            }

            if (childLink.StartsWith("tel:"))
            {
                //Phone links are not wanted
                return false;
            }

            if (childLink.StartsWith("sms:"))
            {
                //sms links are not wanted
                return false;
            }

            if (childLink.ToLower().StartsWith("http:") || childLink.ToLower().StartsWith("https:"))
            {
                searchResult.IsLinkFullyQualified = true;
                var uri = new Uri(childLink);
                if (uri.Host != searchJob.Uri.Host)
                {
                    searchResult.IsLinkExternalDomain = true;
                    return false;
                }
                searchResult.IsLinkExternalDomain = false;
            }

            return true;
        }
    }
}