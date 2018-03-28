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
        public IActionResult Index(string f = "/", string colorClass = null)
        {
            // Used in Detail and Index View => does not hide this single item
            var colorClassFilterList = new FileIndexItem().GetColorClassList(colorClass);

            var model = new IndexViewModel {FileIndexItems = _query.DisplayFileFolders(f,colorClassFilterList)};
            var singleItem = _query.SingleItem(f,colorClassFilterList);
            
            if (!model.FileIndexItems.Any())
            {
                // is directory is emthy 
                var queryIfFolder = _query.GetObjectByFilePath(f);
                
                if (singleItem?.FileIndexItem.FilePath == null && queryIfFolder == null)
                {
                    Response.StatusCode = 404;
                    return View("Error");
                }
            }

            if (singleItem?.FileIndexItem.FilePath != null)
            {
                singleItem.GetAllColor = FileIndexItem.GetAllColorUserInterface();
                singleItem.ColorClassFilterList = colorClassFilterList;
                singleItem.Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItem.FileIndexItem.FilePath);
                return View("SingleItem", singleItem);
            }

            model.Breadcrumb = Breadcrumbs.BreadcrumbHelper(f);
            model.SearchQuery = f.Split("/").LastOrDefault();                
            model.RelativeObjects = _query.GetNextPrevInFolder(f);
            return View(model);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
