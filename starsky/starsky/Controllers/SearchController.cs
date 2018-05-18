using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearch _search;

        public SearchController(ISearch search)
        {
            _search = search;
        }
        [HttpPost]
        public IActionResult Index(string t)
        {
            return RedirectToAction("Index", new { t = t, p = 0 });
        }

        [HttpGet]
        public IActionResult Index(string t, int p = 0, bool json = false)
        {
            var model = _search.Search(t, p);
            if (json) return Json(model);
            return View("Index", model);
            
//            // Json api && View()
//            
//            if (p <= 0) p = p * -1;
//            Stopwatch stopWatch = Stopwatch.StartNew();
//            
//            // t = tag name | p == pagenr.
//
//            var model = new SearchViewModel
//            {
//                PageNumber = p,
//                SearchQuery = t,
//                Breadcrumb = new List<string> {"/"}
//            };
//            if (!string.IsNullOrEmpty(t))
//            {
//                model.Breadcrumb.Add(t);
//            }
//
//            if (string.IsNullOrWhiteSpace(t))
//            {
//                model.FileIndexItems = new List<FileIndexItem>();
//                if (json) return Json(model);
//                return View("Index", model);
//            }
//
//            model.LastPageNumber = _search.SearchLastPageNumber(t);
//            model.SearchCount = _search.SearchCount(t);
//
//            if (p > model.SearchCount)
//            {
//                Response.StatusCode = 404;
//                if (json) return Json("not found");
//                return View("Error", model);
//            }
//            
//            model.FileIndexItems = _search.SearchObjectItem(model.SearchQuery, model.PageNumber);
//            stopWatch.Stop();
//            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
//
//            if (json) return Json(model);
//            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Trash(int p = 0)
        {
            if (p <= 0) p = p * -1;

            // t = tag name | p == pagenr.

            var model = new SearchViewModel();
            model.PageNumber = p;
//            model.SearchQuery = "!delete!";
//            model.Breadcrumb = new List<string>();
//            model.Breadcrumb.Add("/");
//            model.Breadcrumb.Add("/Search/Trash");
//
//            model.LastPageNumber = _search.SearchLastPageNumber("!delete!");
//            model.FileIndexItems = _search.SearchObjectItem(model.SearchQuery, model.PageNumber);
            return View("Trash", model);
        }

        public IActionResult Error()
        {
            // copy to controller, this one below is only for copying
            Response.StatusCode = 404;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}