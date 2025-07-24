using PraiseCMS.Web.Controllers.Base;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class SearchController : BaseController
    {
        public ActionResult SearchResultList(string query)
        {
            ViewBag.query = query;

            var resultList = work.Search.GetSearchResults(query);

            return PartialView("_SearchResultsPartial", resultList);
        }

        [HttpPost]
        public ActionResult SearchResult(string category, string query, bool partial = false)
        {
            ViewBag.query = query;
            ViewBag.category = category;

            var resultList = work.Search.GetSearchResults(category, query);

            return partial ? (ActionResult)PartialView("_AllSearchResults", resultList) : View(resultList);
        }
    }
}