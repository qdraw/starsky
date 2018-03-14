﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            model.ObjectItems = _updateStatusContent.GetObjectItems(f);
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

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult SyncFiles()
        {
            return Json(_updateStatusContent.SyncFiles());
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
