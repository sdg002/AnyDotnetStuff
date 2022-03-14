using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebsiteCrawler.Infrastructure.interfaces
{
    public interface IWebSiteCrawler
    {
        Task<List<entity.SearchResult>> Run(string url);
    }
}