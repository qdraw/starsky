﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IQuery _query;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

        public ApiController(IQuery query, IExiftool exiftool, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _query = query;
            _exiftool = exiftool;
        }
        

        // Used for end2end test
        [HttpGet]
        [HttpHead]
        [ResponseCache(Duration = 30 )]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] /// <=================================
        public IActionResult Env()
        {
            return Json(_appSettings);
        }

        private bool _isReadOnly(string f)
        {
            if (_appSettings.ReadOnlyFolders == null) return false;
            
            var result = _appSettings.ReadOnlyFolders.FirstOrDefault(f.Contains);
            return result != null;
        }

        [HttpPost]
        public IActionResult Update(string tags, string colorClass,
            string captionAbstract, string f, bool collections = true)
        {
            var detailView = _query.SingleItem(f);
            if (detailView == null)
            {
                return NotFound("not in index " + f);
            }

            if (_isReadOnly(detailView.FileIndexItem.ParentDirectory)) return StatusCode(203, "read only");

            foreach (var collectionPath in detailView.FileIndexItem.CollectionPaths)
            {
                var fullPathCollection = _appSettings.DatabasePathToFilePath(collectionPath);
                if (!System.IO.File.Exists(fullPathCollection))
                {
                    detailView.FileIndexItem.CollectionPaths.Remove(collectionPath);
                }
            }

            if (detailView.FileIndexItem.CollectionPaths.Count == 0)
            {
                return NotFound("source image missing");
            }

            // First create an update model
            var updateModel = new ExifToolModel();
            if (tags != null)
            {
                updateModel.Tags = tags;
            }

            if (captionAbstract != null)
            {
                updateModel.CaptionAbstract = captionAbstract;
            }
            detailView.FileIndexItem.SetColorClass(colorClass);
            updateModel.ColorClass = detailView.FileIndexItem.ColorClass;

            var collectionFullPaths = _appSettings.DatabasePathToFilePath(detailView.FileIndexItem.CollectionPaths);
            var oldHashCodes = FileHash.GetHashCode(collectionFullPaths.ToArray());
                
            var exifToolResults = _exiftool.Update(updateModel, collectionFullPaths);

            var listOfUpdateFileIndexItems = new List<FileIndexItem>();
            for (int i = 0; i < detailView.FileIndexItem.CollectionPaths.Count; i++)
            {
                var singleItem = _query.SingleItem(detailView.FileIndexItem.CollectionPaths[i]);
                singleItem.FileIndexItem.Tags = exifToolResults.Tags;
                singleItem.FileIndexItem.Description = exifToolResults.CaptionAbstract;
                singleItem.FileIndexItem.ColorClass = exifToolResults.ColorClass;
                singleItem.FileIndexItem.FileHash = FileHash.GetHashCode(collectionFullPaths[i]);
                // Rename Thumbnail
                new Thumbnail(_appSettings).RenameThumb(oldHashCodes[i], singleItem.FileIndexItem.FileHash);
                listOfUpdateFileIndexItems.Add(singleItem.FileIndexItem);
                _query.UpdateItem(singleItem.FileIndexItem);
            }
         
            return Json(exifToolResults);
        }   
        
        
//            var oldHashCodes = new List<string>();
//
//            var listOfSubPaths = new List<string> {f};
//            if (f.Contains(";"))
//            {
//                listOfSubPaths = ConfigRead.RemoveLatestDotComma(f).Split(";").ToList();
//            }
//
//            var singleItemList = new List<DetailView>();
//            foreach (var item in listOfSubPaths)
//            {
//                if (_isReadOnly(item)) return StatusCode(203,"read only");
//                var singleItem = _query.SingleItem(item);
//                if (singleItem == null) return NotFound("not in index " + item);
//                singleItemList.Add(singleItem);
//                
//                oldHashCodes.Add(singleItem.FileIndexItem.FileHash);
//                if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
//                    return NotFound("source image missing " +
//                                    _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//            }
//
//            var updateModel = new ExifToolModel();
//            if (tags != null)
//            {
//                updateModel.Tags = tags;
//            }
//
//            if (captionAbstract != null)
//            {
//                updateModel.CaptionAbstract = captionAbstract;
//            }
//
//            var exiftoolPathsBuilder = new StringBuilder();
//            foreach (var singleItem in singleItemList)
//            {
//                // Enum get always one value and no null
//                singleItem.FileIndexItem.SetColorClass(colorClass);
//                updateModel.ColorClass = singleItem.FileIndexItem.ColorClass;
//
//                // Update Database with results
//                singleItem.FileIndexItem.FileHash =
//                    FileHash.GetHashCode(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//                singleItem.FileIndexItem.AddToDatabase = DateTime.Now;
//
//                exiftoolPathsBuilder.Append($"\"");
//                exiftoolPathsBuilder.Append(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)); 
//                exiftoolPathsBuilder.Append($"\"");
//                exiftoolPathsBuilder.Append($" "); // space
//                _query.UpdateItem(singleItem.FileIndexItem);
//            }
//            
//            // Run ExifTool updater
//            var exifToolResults = _exiftool.Update(updateModel,exiftoolPathsBuilder.ToString() );
//
//            for (int i = 0; i < singleItemList.Count; i++)
//            {
//                singleItemList[i].FileIndexItem.Tags = exifToolResults.Tags;
//                singleItemList[i].FileIndexItem.Description = exifToolResults.CaptionAbstract;
//                singleItemList[i].FileIndexItem.ColorClass = exifToolResults.ColorClass;
//                // Rename Thumbnail
//                new Thumbnail(_appSettings).RenameThumb(oldHashCodes[i], singleItemList[i].FileIndexItem.FileHash);
//            }


        [ResponseCache(Duration = 30, VaryByQueryKeys = new [] { "f" } )]
        public IActionResult Info(string f = "dbStyleFilepath")
        {
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            
            if (_isReadOnly(f)) return StatusCode(203,"read only");
            
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var getExiftool = _exiftool.Info(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            return Json(getExiftool);
        }

        [HttpDelete]
        public IActionResult Delete(string f = "dbStyleFilepath")
        {
            if (_isReadOnly(f)) return NotFound("afbeelding is in lees-only mode en kan niet worden verwijderd");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            //  Remove Files if exist and RAW file
            var fullFilePath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            var toDeletePaths =
                new List<string>
                {
                    fullFilePath,
                    fullFilePath.Replace(".jpg", ".arw"), 
                    fullFilePath.Replace(".jpg", ".dng"),
                    fullFilePath.Replace(".jpg", ".ARW"), 
                    fullFilePath.Replace(".jpg", ".DNG"),
                    fullFilePath.Replace(".jpg", ".xmp"),
                    fullFilePath.Replace(".jpg", ".XMP")
                };

            foreach (var toDelPath in toDeletePaths)
            {
                if (System.IO.File.Exists(toDelPath))
                {
                    System.IO.File.Delete(toDelPath);
                }
            }
            // End Remove files
            
            _query.RemoveItem(item);

            return Json(item);
        }

        // DEBUG
//        [ResponseCache(Duration = 90000, VaryByQueryKeys = new [] { "f"} )]
        [HttpGet("/api/thumbnail/{f}")]
        [HttpHead("/api/thumbnail/{f}")]
        [IgnoreAntiforgeryToken]
        public IActionResult Thumbnail(
            string f, 
            bool isSingleitem = false, 
            bool json = false,
            bool retryThumbnail = false)
        {
            // f is Hash
            // isSingleItem => detailview
            // Retry thumbnail => is when you press reset thumbnail
            // json, => to don't waste the users bandwith.

            var sourcePath = _query.GetItemByHash(f);

            if (sourcePath == null) return NotFound("not in index");

            var thumbPath = _appSettings.ThumbnailTempFolder + f + ".jpg";

            // When a file is corrupt show error + Delete
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                if (!retryThumbnail)
                {
                    Console.WriteLine("image is corrupt");
                    SetExpiresResponseHeadersToZero();
                    return NoContent();
                }
                System.IO.File.Delete(thumbPath);
            }

            var sourceFullPath = _appSettings.DatabasePathToFilePath(sourcePath);

            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(sourceFullPath))
            {
                if (!isSingleitem)
                {
                    // "Photo exist in database but " + "isSingleItem flag is Missing"
                    SetExpiresResponseHeadersToZero();
                    return NoContent();
                }
                FileStream fs1 = System.IO.File.OpenRead(sourceFullPath);
                return File(fs1, "image/jpeg");
            }

            if (!System.IO.File.Exists(thumbPath) && 
                !System.IO.File.Exists(sourceFullPath))
            {
                return NotFound("There is no thumbnail image and no source image");
            }
            
            // When using the api to check using javascript
            if (json) return Json("OK");

            FileStream fs = System.IO.File.OpenRead(thumbPath);
            return File(fs, "image/jpeg");
        }

        public void SetExpiresResponseHeadersToZero()
        {
            Request.HttpContext.Response.Headers.Remove("Cache-Control");
            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            Request.HttpContext.Response.Headers.Remove("Pragma");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            Request.HttpContext.Response.Headers.Remove("Expires");
            Request.HttpContext.Response.Headers.Add("Expires", "0");
        }

        [HttpGet]
        [HttpHead]
        public IActionResult DownloadPhoto(string f, bool isThumbnail = true)
        {
            // f = subpath/filepath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail= instead of ?isthumbnail=");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);

            var sourceFullPath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(sourceFullPath))
                return NotFound("source image missing " + sourceFullPath );

            // Return full image
            if (!isThumbnail)
            {
                FileStream fs = System.IO.File.OpenRead(sourceFullPath);
                return File(fs, "image/jpeg");
            }

            // Return Thumbnail
            
            var thumbPath = _appSettings.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";

            // If File is corrupt delete it
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                System.IO.File.Delete(thumbPath);
            }

            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.notfound)
            {
                try
                {
                    var searchItem = new FileIndexItem
                    {
                        FileName = _appSettings.FullPathToDatabaseStyle(sourceFullPath).Split("/").LastOrDefault(),
                        ParentDirectory = Breadcrumbs.BreadcrumbHelper(_appSettings.FullPathToDatabaseStyle(sourceFullPath)).LastOrDefault(),
                        FileHash = FileHash.GetHashCode(sourceFullPath)
                    };
                    
                    new Thumbnail(_appSettings).CreateThumb(searchItem);

                    FileStream fs2 = System.IO.File.OpenRead(thumbPath);
                    return File(fs2, "image/jpeg");

                }
                catch (FileNotFoundException)
                {
                    return NotFound("Thumb base folder not found");
                }
            }

            var getExiftool = _exiftool.Info(sourceFullPath);
            _exiftool.Update(getExiftool, sourceFullPath);

            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
        }
    }
}
