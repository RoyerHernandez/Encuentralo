using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WServicesEncuentralo.BO;
using WServicesEncuentralo.Models;

namespace WServicesEncuentralo.Controllers
{
    public class SearchProductController : ApiController
    {
               // POST: api/SearchProduct
        public async Task<HttpResponseMessage> Post(Search search)
        {
            SearchScraping2 scraping = new SearchScraping2();
            return Request.CreateResponse(HttpStatusCode.OK, await scraping.GetBooks2(search), Configuration.Formatters.JsonFormatter);
        }
    }
}
