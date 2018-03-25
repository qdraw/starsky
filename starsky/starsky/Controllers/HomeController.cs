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
                singleItem.GetAllColor = FileIndexItem.GetAllColorUserInterface();
                
                singleItem.Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItem.FileIndexItem.FilePath);
                return View("SingleItem", singleItem);
            }

            model.Breadcrumb = Breadcrumbs.BreadcrumbHelper(model.FileIndexItems?.FirstOrDefault().FilePath);
            model.SearchQuery = model.FileIndexItems?.FirstOrDefault().ParentDirectory.Split("/").LastOrDefault();
            
                
            model.RelativeObjects = _query.GetNextPrevInFolder(model.FileIndexItems?.FirstOrDefault().ParentDirectory);
            return View(model);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
