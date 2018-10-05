﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public List<FileIndexItem> Rename(string f, string to, bool collections = true)
        {
			var inputFileSubPaths = ConfigRead.SplitInputFilePaths(f);
			var toFileSubPaths = ConfigRead.SplitInputFilePaths(to);
			
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();
			
			// To check if the file has a unique name (in database)
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				toFileSubPaths[i] = ConfigRead.RemoveLatestSlash(toFileSubPaths[i]);
				toFileSubPaths[i] = ConfigRead.PrefixDbSlash(toFileSubPaths[i]);

				var detailView = _query.SingleItem(toFileSubPaths[i], null, collections, false);
				if (detailView != null) toFileSubPaths[i] = null;
			}
			
			for (var i = 0; i < inputFileSubPaths.Length; i++)
			{
				inputFileSubPaths[i] = ConfigRead.RemoveLatestSlash(inputFileSubPaths[i]);
				inputFileSubPaths[i] = ConfigRead.PrefixDbSlash(inputFileSubPaths[i]);

				var detailView = _query.SingleItem(inputFileSubPaths[i], null, collections, false);
				if (detailView == null) inputFileSubPaths[i] = null;
			}
			
			// Remove null from list
			toFileSubPaths = toFileSubPaths.Where(p => p != null).ToArray();
			inputFileSubPaths = inputFileSubPaths.Where(p => p != null).ToArray();
			
			// Check if two list are the same lenght - Change this in the future BadRequest("f != to")
			if (toFileSubPaths.Length != inputFileSubPaths.Length) return null;
			
			for (var i = 0; i < toFileSubPaths.Length; i++)
			{
				var inputFileSubPath = inputFileSubPaths[i];
				var toFileSubPath = toFileSubPaths[i];
				
				var detailView = _query.SingleItem(inputFileSubPath, null, collections, false);
				// files that not exist
				if(detailView == null) continue; // next
				var toFileFullPath = _appSettings.DatabasePathToFilePath(toFileSubPath,false);
				var inputFileFullPath = _appSettings.DatabasePathToFilePath(inputFileSubPath);
				
				if(Files.IsFolderOrFile(toFileFullPath) 
					!= FolderOrFileModel.FolderOrFileTypeList.Deleted 
					|| Files.IsFolderOrFile(inputFileFullPath) 
					!= FolderOrFileModel.FolderOrFileTypeList.Folder) continue; //next
				
				Directory.Move(inputFileFullPath,toFileFullPath);
				
				var fileIndexItems = _query.GetAllRecursive(inputFileSubPath);
				// Rename child items
				fileIndexItems.ForEach(p => 
					p.ParentDirectory = p.ParentDirectory.Replace(inputFileSubPath, toFileSubPath)
				);
				
				// Rename parent item >eg the folder
				detailView.FileIndexItem.SetFilePath(toFileSubPath);
				fileIndexItems.Add(detailView.FileIndexItem);

				// To update the results
				_query.UpdateItem(fileIndexItems);
				
				fileIndexResultsList.AddRange(fileIndexItems);
			}

	        return fileIndexResultsList;
        }
    }
}
