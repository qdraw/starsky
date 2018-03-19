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
    public class ApiController : Controller
    {
        private readonly IUpdate _updateStatusContent;

        public ApiController(IUpdate updateStatusContent)
        {
            _updateStatusContent = updateStatusContent;
        }

        public IActionResult Env()
        {
            var model = new EnvViewModel
            {
                DatabaseType = AppSettingsProvider.DatabaseType,
                BasePath = AppSettingsProvider.BasePath,
                ExifToolPath = AppSettingsProvider.ExifToolPath,
                ThumbnailTempFolder = AppSettingsProvider.ThumbnailTempFolder,
            };
            if (AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                model.DbConnectionString = AppSettingsProvider.DbConnectionString;
            }
            return Json(model);
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
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            var singleItem = _updateStatusContent.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _updateStatusContent.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            var getExiftool = ExifTool.ReadExifToolKeywords(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            if (item.Tags != getExiftool)
            {
                Response.StatusCode = 205;
            }
            item.Tags = getExiftool;
            return Json(item);

        }



        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
