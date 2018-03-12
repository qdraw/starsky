﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUpdate _updateStatusContent;

        public HomeController(IUpdate updateStatusContent)
        {
            _updateStatusContent = updateStatusContent;
        }

        public IActionResult Index()
        {

            return View();
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

        public IActionResult Update()
        {
            var item = new FileIndexItem();
            item.FileName = "item";
            item.FilePath = "i";
            _updateStatusContent.AddItem(item);
            return Json(item);
        }

        public IActionResult GetFolder(string p = "/")
        {
            var i = _updateStatusContent.GetFolder(p);

            return Json(i);
        }

        public IActionResult GetFilesInFolder(string p = "/")
        {
            var i = _updateStatusContent.GetFilesInFolder(p);

            return Json(i);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
