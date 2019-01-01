﻿using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {


        public List<string> GetListOfSubpaths(string subPath)
        {
            subPath = _query.SubPathSlashRemove(subPath);
            var listOfSubpaths = Breadcrumbs.BreadcrumbHelper(subPath);
            listOfSubpaths.Add(subPath);
            return listOfSubpaths;
        }

	    /// <summary>
	    /// Add Sub Path Folder - Parent Folders
	    ///  root(/)
	    ///      /2017  <= index only this folder
	    ///      /2018
	    /// If you use the cmd: $ starskycli -s "/2017"
	    /// the folder '2017' it self is not added 
	    /// and all parent paths are not included
	    /// this class does add those parent folders
	    /// </summary>
	    /// <param name="subPath">the relative urls</param>
        public void AddSubPathFolder(string subPath)
        {
            foreach (var itemSubpath in GetListOfSubpaths(subPath))
            {
                if(_appSettings.Verbose) Console.WriteLine("AddSubPathFolder: " + itemSubpath);

                if (_query.GetObjectByFilePath(itemSubpath) != null) continue;
                
                var newItem = new FileIndexItem
                {
                    AddToDatabase = DateTime.UtcNow,
                    IsDirectory = true,
                    ColorClass = FileIndexItem.Color.None
                };
                if (itemSubpath != "/")
                {
                    newItem.ParentDirectory = Breadcrumbs.BreadcrumbHelper(itemSubpath).LastOrDefault();
                }
                newItem.FileName = itemSubpath.Split("/").LastOrDefault();
                _query.AddItem(newItem);

            }
        }
    }
}
