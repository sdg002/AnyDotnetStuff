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
        private readonly IHtmlParser _htmlParser;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SingleThreadedWebSiteCrawler> _logger;

        public SingleThreadedWebSiteCrawler(
            ILogger<SingleThreadedWebSiteCrawler> logger,
            IHtmlParser htmlParser,
            HttpClient httpClient)
        {
            this._logger = logger;
            this._htmlParser = htmlParser;
            this._httpClient = httpClient;
        }

        public async Task<List<SearchResult>> Run(string url)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Other");

            //TODO you were here, do the single threaded thing

            var results = new SortedDictionary<string, WebsiteCrawler.Infrastructure.entity.SearchResult>();
            var jobQueue = new Queue<SearchJob>(new SearchJob[] { new entity.SearchJob { Level = 0, Url = url } });
            while (jobQueue.Count > 0)
            {
                await DiscoverLinks(jobQueue, results);
            }

            //var allLinks = _htmlParser.GetLinks(htmlContent);

            //Old stuff

            throw new NotImplementedException();
        }

        private async Task DiscoverLinks(Queue<entity.SearchJob> jobQueue, IDictionary<string, SearchResult> results)
        {
            var urlToSearch = jobQueue.Dequeue();
            string urlContents = await DownloadPage(urlToSearch.Url);
            var links = FindLinksWithinHtml(urlContents);
            links.ForEach(link =>
            {
                bool isLinkAcceptable = IsLinkAcceptable(urlToSearch.Url, link);
                if (!isLinkAcceptable)
                {
                    return;
                }
                var newUrl = UrlExtensions.Combine(urlToSearch.Url, link);
                var searchResult = new SearchResult(urlToSearch.Url, newUrl.ToString(), urlToSearch.Level + 1);
                results.Add(searchResult.ChildPageUrl, searchResult);

                jobQueue.Enqueue(new SearchJob { Level = searchResult.Level, Url = searchResult.ChildPageUrl });
            });
        }

        private async Task<string> DownloadPage(string urlToSearch)
        {
            var htmlResponse = await _httpClient.GetAsync(urlToSearch);
            if (!htmlResponse.IsSuccessStatusCode)
            {
                throw new NotImplementedException("How do we handle errors? Think"); //TODO handle non-sucess response, Polly retry
            }

            var ctype = htmlResponse.Content.Headers.ContentType;
            //TODO Handle non-text content type gracefully

            var header = htmlResponse.Headers;
            var htmlContent = await htmlResponse.Content.ReadAsStringAsync();
            return htmlContent;
        }

        private List<string> FindLinksWithinHtml(string htmlContent)
        {
            //TODO put this in the wrapper service for link parsing
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
            var links = linkNodes.Select(n => n.Attributes["href"]).ToList();
            //return links.Where(l=>l.hr)
            return links.Select(l => l.Value).ToList();
        }

        private bool IsLinkAcceptable(string parentUrl, string childLink)
        {
            //TODO you were here, skip if empty, skip if host name does not match, bookmarks

            if (string.IsNullOrEmpty(childLink))
            {
                return false;
            }
            var frags = childLink.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (!frags.Any())
            {
                return false;
            }

            if (childLink.ToLower().StartsWith("http:") || childLink.ToLower().StartsWith("https:"))
            {
                //TODO handle child links which are fully qualified
                return false;
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