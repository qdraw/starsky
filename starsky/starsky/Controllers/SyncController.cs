using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starsky.Controllers
{
    [Authorize]
    public class SyncController : Controller
    {
        private readonly ISync _sync;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;

        public SyncController(ISync sync, IBackgroundTaskQueue queue, IQuery query, AppSettings appSettings)
        {
            _sync = sync;
            _bgTaskQueue = queue;
            _query = query;
            _appSettings = appSettings;
        }
        
        /// <summary>
        /// Do a file sync in a background process
        /// </summary>
        /// <param name="f">subpaths split by dot comma</param>
        /// <returns></returns>
        [ActionName("Index")]
        public IActionResult SyncIndex(string f)
        {
            var inputFilePaths = ConfigRead.SplitInputFilePaths(f).ToList();
            // the result list
            var fileIndexResultsList = new List<FileIndexItem>();

            for (int i = 0; i < inputFilePaths.Count; i++)
            {
                var subPath = inputFilePaths[i];
                var detailView = _query.SingleItem(subPath,null,false,false);
                var statusResults = new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);

                var statusModel = new FileIndexItem();
                statusModel.SetFilePath(subPath);
                statusModel.IsDirectory = false;

                // if one item fails, the status will added
                if (!new StatusCodesHelper(null).ReturnExifStatusError(
                    statusModel, statusResults, fileIndexResultsList)) continue;
                
                inputFilePaths[i] = null;
                if (statusResults == FileIndexItem.ExifStatus.NotFoundIsDir)
                {
                    inputFilePaths.AddRange(_query.DisplayFileFolders(subPath, null, false)
                        .Where(p => p.IsDirectory == false).Select(p => p.FilePath));
                }
            }

            foreach (var subPath in inputFilePaths)
            {
                if (subPath != null)
                {
                    // Update >
                    _bgTaskQueue.QueueBackgroundWorkItem(async token =>
                    {
                        _sync.SyncFiles(subPath);
                    });
                }
            }
            
            return Json(inputFilePaths);
        }

    }
}