using Dawn;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebsiteCrawler.Infrastructure.entity;
using WebsiteCrawler.Infrastructure.extensions;
using WebsiteCrawler.Infrastructure.interfaces;
using WebsiteCrawler.Service.entity;

namespace WebsiteCrawler.Service
{
    public class SingleThreadedWebSiteCrawler : IWebSiteCrawler
    {
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

            var htmlResponse = await _httpClient.GetAsync(searchJob.Url);
            if (!htmlResponse.IsSuccessStatusCode)
            {
                //throw new NotImplementedException("How do we handle errors? Think"); //TODO handle non-sucess response, Polly retry
                _logger.LogError($"Error while downloading page {searchJob}");
                _errors.Add(new HttpError { Url = searchJob.Url, HttpStatusCode = htmlResponse.StatusCode });
            }

            var ctype = htmlResponse.Content.Headers.ContentType;
            if (!ctype.MediaType.Contains("text/html"))
            {
                _logger.LogInformation($"Content in url:{searchJob} has content type:{ctype}. This is non-html Ignoring!");
                return null;
            }
            //TODO Handle non-text content type gracefully

            var htmlContent = await htmlResponse.Content.ReadAsStringAsync();
            return htmlContent;
        }

        private List<string> FindLinksWithinHtml(string htmlContent)
        {
            //TODO put this in the wrapper service for link parsing
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogInformation("Found empty HTML page");
                return new List<string>();
            }
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes == null)
            {
                return new List<string>();
            }
            var links = linkNodes.Where(n => n.Attributes.Contains("href")).Select(n => n.Attributes["href"]).ToList();
            //return links.Where(l=>l.hr)
            return links.Select(l => l.Value).ToList();
        }

        private bool IsLinkAcceptable(SearchJob searchJob, SearchResult searchResult)
        {
            var childLink = searchResult.OriginalLink;
            //TODO  skip if mailto or email
            /*
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Found a link 'mailto:ask@reedexpo.com.au'
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Found a child link:'mailto:ask@reedexpo.com.au' which is relative to container page:mailto:ask@reedexpo.com.au under parent:https://rxglobal.com/
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Child link:mailto:ask@reedexpo.com.au was added to results
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Queue=52 Search results=53
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Found a link 'tel:61%202%209422%202500'
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Found a child link:'tel:61%202%209422%202500' which is relative to container page:tel:61 2 9422 2500 under parent:https://rxglobal.com/
WebsiteCrawler.Service.SingleThreadedWebSiteCrawler: Information: Child link:tel:61 2 9422 2500 was added to results

             */
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
                //TODO handle child links which are fully qualified
            }

            return true;
        }
    }
}

/*
 * skip /
 * skip ""
 * You will need to figure out how to handle relative domain rx-italy as opposed to fully qualified domain name
 *

    [0]: Name: "href", Value: "/"
    [1]: Name: "href", Value: "rx-australia"
    [2]: Name: "href", Value: "rx-austria-germany"
    [3]: Name: "href", Value: "rx-brazil"
    [4]: Name: "href", Value: "rx-china"
    [5]: Name: "href", Value: "rx-france"
    [6]: Name: "href", Value: "rx-india"
    [7]: Name: "href", Value: "rx-indonesia"
    [8]: Name: "href", Value: "rx-italy"
    [9]: Name: "href", Value: "rx-japan"
    [10]: Name: "href", Value: "rx-korea"
    [11]: Name: "href", Value: "rx-mexico"
    [12]: Name: "href", Value: "rx-middle-east"
    [13]: Name: "href", Value: "rx-russia"
    [14]: Name: "href", Value: "rx-singapore"
    [15]: Name: "href", Value: "rx-south-africa"
    [16]: Name: "href", Value: "rx-tradex-thailand"
    [17]: Name: "href", Value: "rx-tradex-vietnam"
    [18]: Name: "href", Value: "rx-tuyap"
    [19]: Name: "href", Value: "rx-uk"
    [20]: Name: "href", Value: "rx-usa"
    [21]: Name: "href", Value: "rx-mack-brooks"
    [22]: Name: "href", Value: "/"
    [23]: Name: "href", Value: "/about-rx"
    [24]: Name: "href", Value: "/leadership-team"
    [25]: Name: "href", Value: "/why-rx"
    [26]: Name: "href", Value: "/events"
    [27]: Name: "href", Value: "/life-at-rx"
    [28]: Name: "href", Value: "/our-values"
    [29]: Name: "href", Value: "/inclusion-diversity"
    [30]: Name: "href", Value: "/corporate-responsibility"
    [31]: Name: "href", Value: "/join-us"
    [32]: Name: "href", Value: "/our-stories"
    [33]: Name: "href", Value: "/our-stories"
    [34]: Name: "href", Value: "/our-stories?created=&amp;field_story_type_target_id%5B0%5D=10&amp;field_story_type_target_id%5B1%5D=2"
    [35]: Name: "href", Value: "/our-stories?created=&amp;field_story_type_target_id%5B0%5D=19&amp;field_story_type_target_id%5B1%5D=3"
    [36]: Name: "href", Value: "/our-stories?created=&amp;field_story_type_target_id%5B0%5D=18&amp;field_story_type_target_id%5B1%5D=1"
    [37]: Name: "href", Value: "/press-kit"
    [38]: Name: "href", Value: "/contact-us"
    [39]: Name: "href", Value: "/events"
    [40]: Name: "href", Value: "/events?map=true"
    [41]: Name: "href", Value: "https://fastenerfair-connect.com/"
    [42]: Name: "href", Value: "https://www.fastenerfairfrance.com/"
    [43]: Name: "href", Value: "http://www.brasiloffshore.com"
    [44]: Name: "href", Value: ""

 *
 */