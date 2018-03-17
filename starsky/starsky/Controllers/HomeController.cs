﻿using System;
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

        //[HttpPost]
        public IActionResult Update(string f = "path", string t = "")
        {
            var singleItem = _updateStatusContent.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");

            var oldHashCode = _updateStatusContent.SingleItem(f).FileIndexItem.FileHash;

            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var exifToolResult = ExifTool.SetExifToolKeywords(t, FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            if (exifToolResult == null) return BadRequest();

            var item = _updateStatusContent.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            item.FileHash = FileHash.CalcHashCode(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            item.AddToDatabase = DateTime.Now;
            item.Tags = exifToolResult;
            _updateStatusContent.UpdateItem(item);

            new Thumbnail().RenameThumb(oldHashCode, item.FileHash);

            return RedirectToAction("Info", new { f = f, t = exifToolResult });
        }

        public IActionResult Info(string f = "uniqueid", string t = "")
        {
            var singleItem = _updateStatusContent.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var item = _updateStatusContent.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;
            return Json(item);
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
