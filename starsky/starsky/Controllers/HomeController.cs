using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        
        private readonly IQuery _query;

        public HomeController(IQuery query)
        {
            _query = query;
        }

        [HttpGet]
        [HttpHead]
        public IActionResult Index(
            string f = "/", 
            string colorClass = null,
            bool json = false,
            bool collections = true
            )
        {

            // Trick for avoiding spaces for behind proxy
            f = f.Replace("$20", " ");
            
            // Used in Detail and Index View => does not hide this single item
            var colorClassFilterList = new FileIndexItem().GetColorClassList(colorClass);
            var subpath = _query.SubPathSlashRemove(f);

            var model = new ArchiveViewModel
            {
                FileIndexItems = _query.DisplayFileFolders(subpath, colorClassFilterList),
                RelativeObjects = new RelativeObjects(),
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(subpath)
            };

            var singleItem = _query.SingleItem(subpath,colorClassFilterList);
            
            if (!model.FileIndexItems.Any())
            {
                // is directory is emthy 
                var queryIfFolder = _query.GetObjectByFilePath(subpath);

                // For showing a new database
                switch (f)
                {
                    case "/" when !json && queryIfFolder == null:
                        return View(model);
                    case "/" when queryIfFolder == null:
                        return Json(model);
                }

                if (singleItem?.FileIndexItem.FilePath == null && queryIfFolder == null)
                {
                    Response.StatusCode = 404;
                    if (json) return Json("not found");
                    return View("Error");
                }
            }

            if (singleItem?.FileIndexItem.FilePath != null)
            {
                if (json) return Json(singleItem);
                return View("SingleItem", singleItem);
            }
            
            model.SearchQuery = subpath.Split("/").LastOrDefault();                
            model.RelativeObjects = _query.GetNextPrevInFolder(subpath);

            if (collections) // in prev next collections is required
            {
                model.FileIndexItems = _query.Collections(model.FileIndexItems.ToList());
            }
            
            if (json) return Json(model);
            return View(model);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
