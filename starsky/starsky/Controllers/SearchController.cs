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
        public IActionResult Index(string t, int p = 0)
        {
            if (p <= 0) p = p * -1;
            Stopwatch stopWatch = Stopwatch.StartNew();


            // t = tag name | p == pagenr.

            var model = new SearchViewModel();
            model.PageNumber = p;
            model.SearchQuery = t;
            model.SearchCount = _search.SearchCount(t);
            model.Breadcrumb = new List<string>();
            model.Breadcrumb.Add("/");
            if (!string.IsNullOrEmpty(t))
            {
                model.Breadcrumb.Add(t);
            }

            if (string.IsNullOrWhiteSpace(t))
            {
                model.FileIndexItems = new List<FileIndexItem>();
                return View("Index", model);
            }

            model.LastPageNumber = _search.SearchLastPageNumber(t);
            model.FileIndexItems = _search.SearchObjectItem(model.SearchQuery, model.PageNumber);
            stopWatch.Stop();
            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;

            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Trash(int p = 0)
        {
            if (p <= 0) p = p * -1;

            // t = tag name | p == pagenr.

            var model = new SearchViewModel();
            model.PageNumber = p;
            model.SearchQuery = "!delete!";
            model.Breadcrumb = new List<string>();
            model.Breadcrumb.Add("/");
            model.Breadcrumb.Add("/Search/Trash");

            model.LastPageNumber = _search.SearchLastPageNumber("!delete!");
            model.FileIndexItems = _search.SearchObjectItem(model.SearchQuery, model.PageNumber);
            return View("Trash", model);
        }


    }
}