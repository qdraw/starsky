﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly AppSettings _appsettings;

        public HomeController(IQuery query, AppSettings appsettings = null)
        {
            _query = query;
            _appsettings = appsettings;
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

            // First check if it is a single Item
            var singleItem = _query.SingleItem(subpath, colorClassFilterList,collections);
            // returns no object when it a directory
            
            if (singleItem?.IsDirectory == false)
            {
                if (_appsettings != null && _appsettings.AddHttp2Optimizations)
                {
                    // HTTP2 push
                    Response.Headers["Link"] =
                        "<" + Url.Action("Thumbnail", "Api", new {f = singleItem.FileIndexItem.FileHash}) +
                        "?issingleitem=True>; rel=preload; as=image"; 
                    Response.Headers["Link"] += ",";
                    Response.Headers["Link"] += "<"
                                                + Url.Action("Info", "Api", new {f = singleItem.FileIndexItem.FilePath}) +
                                                ">; rel=preload; as=fetch";
                }
                
                if (json) return Json(singleItem);
                return View("SingleItem", singleItem);
            }
            
            // (singleItem.IsDirectory) or not found
            var directoryModel = new ArchiveViewModel
            {
                FileIndexItems = _query.DisplayFileFolders(subpath),
                RelativeObjects = _query.GetNextPrevInFolder(subpath),
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(subpath),
                SearchQuery = subpath.Split("/").LastOrDefault()
            };

            if (singleItem == null)
            {
                // For showing a new database
                var queryIfFolder = _query.GetObjectByFilePath(subpath);

                // For showing a new database
                switch (f)
                {
                    case "/" when !json && queryIfFolder == null:
                        return View(directoryModel);
                    case "/" when queryIfFolder == null:
                        return Json(directoryModel);
                }

                if (singleItem?.FileIndexItem.FilePath == null && queryIfFolder == null)
                {
                    Response.StatusCode = 404;
                    if (json) return Json("not found");
                    return View("Error");
                }
            }
            
            
            if (json) return Json(directoryModel);
            return View(directoryModel);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
