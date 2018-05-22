﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearch _search;

        public SearchController(ISearch search)
        {
            _search = search;
        }
        
        [HttpPost]
        public IActionResult Index(string t)
        {
            return RedirectToAction("Index", new { t = t, p = 0 });
        }

        [HttpGet]
        public IActionResult Index(string t, int p = 0, bool json = false)
        {
            // Json api && View()            
            var model = _search.Search(t, p);
            if (json) return Json(model);
            return View("Index", model);
        }

        [HttpGet]
        public IActionResult Trash(int p = 0, bool json = false)
        {
            var model = _search.Search("!delete!", p);
            if (json) return Json(model);
            return View("Trash", model);
        }

        public IActionResult Error()
        {
            // copy to controller, this one below is only for copying
            Response.StatusCode = 404;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}