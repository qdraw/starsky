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
        private readonly IQuery _query;

        public HomeController(IQuery query)
        {
            _query = query;
        }

        [HttpGet]
        public IActionResult Index(string f = "/")
        {
            var model = new IndexViewModel {FileIndexItems = _query.DisplayFileFolders(f)};
            var singleItem = _query.SingleItem(f);
            
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



        //[HttpGet]
        //public IActionResult Trash(int p = 0)
        //{
        //    if (p <= 0) p = p * -1;

        //    // t = tag name | p == pagenr.

        //    var model = new SearchViewModel();
        //    model.PageNumber = p;
        //    model.SearchQuery = "<delete>";
        //    model.Breadcrumb = new List<string>();
        //    model.Breadcrumb.Add("/");
        //    model.Breadcrumb.Add("/Home/Trash");

        //    model.LastPageNumber = _updateStatusContent.SearchLastPageNumber("<delete>");
        //    model.FileIndexItems = _updateStatusContent.SearchObjectItem(model.SearchQuery, model.PageNumber);
        //    return View("Trash", model);
        //}






        public IActionResult GetFolder(string p = "/")
        {
            var i = _query.DisplayFileFolders(p);

            return Json(i);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
