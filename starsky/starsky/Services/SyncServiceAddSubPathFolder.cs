﻿using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.Services
{
    public partial class SyncService
    {
        // Add Sub Path Folder - Parent Folders
        //  root(/)
        //      /2017  <= index only this folder
        //      /2018
        // If you use the cmd: $ starskycli -s "/2017"
        // the folder '2017' it self is not added 
        // and all parrent paths are not included
        // this class does add those parent folders

        public List<string> GetListOfSubpaths(string subPath)
        {
            subPath = _query.SubPathSlashRemove(subPath);
            var listOfSubpaths = Breadcrumbs.BreadcrumbHelper(subPath);
            listOfSubpaths.Add(subPath);
            return listOfSubpaths;
        }

        public void AddSubPathFolder(string subPath)
        {
            foreach (var itemSubpath in GetListOfSubpaths(subPath))
            {
                if(AppSettingsProvider.Verbose) Console.WriteLine("AddSubPathFolder: " + itemSubpath);

                var countFolder = _context.FileIndex.Count(p => p.FilePath == itemSubpath);
                if (countFolder == 0)
                {
                    var newItem = new FileIndexItem
                    {
                        // FilePath = itemSubpath,
                        AddToDatabase = DateTime.UtcNow,
                        IsDirectory = true
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
}
