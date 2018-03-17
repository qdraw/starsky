using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Internal;
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
                singleItem.Breadcrumb = BreadcrumbHelper(singleItem.FileIndexItem.FilePath);
                return View("SingleItem", singleItem);
            }

            model.Breadcrumb = BreadcrumbHelper(model.FileIndexItems?.FirstOrDefault().FilePath);
            model.SearchQuery = model.FileIndexItems?.FirstOrDefault().ParentDirectory.Split("/").LastOrDefault();
            return View(model);
        }

        public List<string> BreadcrumbHelper(string filePath)
        {
            if (filePath == null) return null;

            var breadcrumb = new List<string>();
            if (filePath[0].ToString() != "/")
            {
                filePath = "/" + filePath;
            }
            var filePathArray = filePath.Split("/");

            var dir = 0;
            while (dir < filePathArray.Length - 1)
            {
                if (string.IsNullOrEmpty(filePathArray[dir]))
                {
                    breadcrumb.Add("/");
                }
                else
                {

                    var item = "";
                    for (int i = 0; i <= dir; i++)
                    {
                        if (!string.IsNullOrEmpty(filePathArray[i]))
                        {
                            item += "/" + filePathArray[i];
                        }
                        //else
                        //{
                        //    item += "/" +filePathArray[i];
                        //}
                    }
                    breadcrumb.Add(item);
                }
                dir++;

            }

            return breadcrumb;
        }

        [HttpPost]
        public IActionResult Search(string t)
        {
            return RedirectToAction("Search", new { t = t, p = 0  });
        }

        [HttpGet]
        public IActionResult Search(string t, int p = 0)
        {
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




        public IActionResult Thumbnail(string f)
        {

            var sourcePath = _updateStatusContent.GetItemByHash(f);

            if (sourcePath == null) return NotFound("not in index");

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + f + ".jpg";

            if (!System.IO.File.Exists(thumbPath) && System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                return NotFound("could regenerate thumb");
            };

            if (!System.IO.File.Exists(thumbPath))
            {
                return NotFound("in cache but not in thumbdb");
            }


            FileStream fs = System.IO.File.OpenRead(thumbPath);
            return File(fs, "image/jpeg");

            //using (FileStream fs = System.IO.File.OpenRead(path))
            //{
            //    var result = File(fs, "image/jpeg");
            //    //fs.Dispose();
            //    return result;
            //}

        }

        public IActionResult Count(string f)
        {
            return Json(_updateStatusContent.GetAllFiles(f).Count);
        }

        public IActionResult SyncFiles()
        {
            //_updateStatusContent.SyncFiles()
            return Json("");
        }

        //public IActionResult Update()
        //{
        //    var item = new FileIndexItem();
        //    item.FileName = "item";
        //    item.FilePath = "i";
        //    _updateStatusContent.AddItem(item);
        //    return Json(item);
        //}

        public IActionResult GetFolder(string p = "/")
        {
            var i = _updateStatusContent.DisplayFileFolders(p);

            return Json(i);
        }

        //public IActionResult GetFilesInFolder(string p = "/")
        //{
        //    var i = _updateStatusContent.GetFilesInFolder(p);

        //    return Json(i);
        //}


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
