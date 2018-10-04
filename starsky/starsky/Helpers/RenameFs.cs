using System.Collections.Generic;
using System.IO;
using System.Linq;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starsky.Helpers
{
    public class RenameFs
    {
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;

        public RenameFs(AppSettings appSettings, IQuery query)
        {
            _query = query;
            _appSettings = appSettings;
        }

        public void Rename(string f, string to, bool collections = true)
        {
            var inputFileSubPaths = ConfigRead.SplitInputFilePaths(f);
            var toFileSubPaths = ConfigRead.SplitInputFilePaths(to);
            
            // the result list
            //var fileIndexResultsList = new List<FileIndexItem>();
            
            // To check if the file has a unique name (in database)
            for (var i = 0; i < toFileSubPaths.Length; i++)
            {
                toFileSubPaths[i] = ConfigRead.RemoveLatestSlash(toFileSubPaths[i]);
                var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
                if (detailView != null) toFileSubPaths[i] = null;
            }

            for (var i = 0; i < inputFileSubPaths.Length; i++)
            {
                var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
                if (detailView != null) inputFileSubPaths[i] = null;
            }

            // Remove null from list
            toFileSubPaths = toFileSubPaths.Where(p => p != null).ToArray();
            inputFileSubPaths = inputFileSubPaths.Where(p => p != null).ToArray();

            // Check if two list are the same lenght - Change this in the future BadRequest("f != to")
            if (toFileSubPaths.Length != inputFileSubPaths.Length) return;

            for (var i = 0; i < toFileSubPaths.Length; i++)
            {
                var inputFileSubPath = inputFileSubPaths[i];
                var toFileSubPath = toFileSubPaths[i];

                var detailView = _query.SingleItem(inputFileSubPath, null, collections, false);
                // files that not exist
                if(detailView == null) continue;
                if (detailView.IsDirectory)
                {
                    var toFileFullPath = _appSettings.DatabasePathToFilePath(toFileSubPath);
                    var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputFileSubPath);

                    Directory.Move(inputFileFullPath,toFileFullPath);
                    // move also in the database

                    var t = _query.GetAllRecursive(inputFileSubPath);
                }
            }
        }
    }
}