using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUpdate _updateStatusContent;

        public HomeController(IUpdate updateStatusContent)
        {
            _updateStatusContent = updateStatusContent;
        }

        [HttpGet]
        public IActionResult Index(string f = "/")
        {
            var model = new IndexViewModel {FileIndexItems = _updateStatusContent.DisplayFileFolders(f)};
            var singleItem = _updateStatusContent.SingleItem(f);
            
            if (!model.FileIndexItems.Any())
            {
                if (singleItem?.FileIndexItem.FilePath == null)
                {
                    Response.StatusCode = 404;
                    return View("Error");
                }
            }

            if (singleItem?.FileIndexItem.FilePath != null)
            {
                singleItem.Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItem.FileIndexItem.FilePath);
                return View("SingleItem", singleItem);
            }

            model.Breadcrumb = Breadcrumbs.BreadcrumbHelper(model.FileIndexItems?.FirstOrDefault().FilePath);
            model.SearchQuery = model.FileIndexItems?.FirstOrDefault().ParentDirectory.Split("/").LastOrDefault();
            return View(model);
        }


        [HttpPost]
        public IActionResult Search(string t)
        {
            return RedirectToAction("Search", new { t = t, p = 0  });
        }

        [HttpGet]
        public IActionResult Search(string t, int p = 0)
        {
            if (p <= 0) p = p *-1;

            // t = tag name | p == pagenr.

            var model = new SearchViewModel();
            model.PageNumber =  p;
            model.SearchQuery = t;
            model.Breadcrumb = new List<string>();
            model.Breadcrumb.Add("/");
            if (!string.IsNullOrEmpty(t))
            {
                model.Breadcrumb.Add(t);
            }

            if (string.IsNullOrWhiteSpace(t))
            {
                model.FileIndexItems = new List<FileIndexItem>();
                return View("Search", model);
            }

            model.LastPageNumber = _updateStatusContent.SearchLastPageNumber(t);
            model.FileIndexItems = _updateStatusContent.SearchObjectItem(model.SearchQuery, model.PageNumber);
            return View("Search", model);
        }




        

        public IActionResult GetFolder(string p = "/")
        {
            var i = _updateStatusContent.DisplayFileFolders(p);

            return Json(i);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
