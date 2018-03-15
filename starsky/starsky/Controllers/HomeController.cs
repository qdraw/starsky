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
            var model = new IndexViewModel();
            // No check for 404's

            model.ObjectItems = _updateStatusContent.GetObjectItems(f).ToList();

            if (!model.ObjectItems.Any())
            {
                Response.StatusCode = 404;
                return View("Error");
            }

            var firstItem = model?.ObjectItems?.FirstOrDefault();

            model.Breadcrumb = BreadcrumbHelper(firstItem);

            if (firstItem != null && !firstItem.IsFolder && model.ObjectItems.Count() == 1)
            {
                model.SingleItem = firstItem;

                return View("SingleItem", model);
            }

            return View(model);
        }

        public List<string> BreadcrumbHelper(ObjectItem firstItem)
        {
            var filePath = firstItem?.FilePath;
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

        [HttpGet]
        [HttpPost]
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

            model.ObjectItems = _updateStatusContent.SearchObjectItem(model.SearchQuery, model.PageNumber);
            return View("Search", model);
        }




        public IActionResult Thumbnail(string f)
        {

            var sourcePath = _updateStatusContent.GetItemByHash(f);

            if (sourcePath == null) return NotFound("not in index");

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + f + ".jpg";

            if (!System.IO.File.Exists(thumbPath) && System.IO.File.Exists(Files.PathToFull(sourcePath)))
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
            return Json(_updateStatusContent.GetAll(f).Count);
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
            var i = _updateStatusContent.GetChildFolders(p);

            return Json(i);
        }

        public IActionResult GetFilesInFolder(string p = "/")
        {
            var i = _updateStatusContent.GetFilesInFolder(p);

            return Json(i);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
