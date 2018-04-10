﻿using System;
using System.Collections.Generic;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface IQuery
    {

        List<FileIndexItem> GetAllFiles(string subPath = "");
        
        List<FileIndexItem> GetAllFilesRecursive(string subPath = "");

        IEnumerable<FileIndexItem> DisplayFileFolders(
            string subPath = "/", 
            List<FileIndexItem.Color> colorFilterClass = null);

        DetailView SingleItem(string singleItemDbPath, 
            List<FileIndexItem.Color> colorFilterClass = null);

        FileIndexItem GetObjectByFilePath(string filePath);

        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);

        string GetItemByHash(string fileHash);


        FileIndexItem AddItem(FileIndexItem updateStatusContent);
        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);

        string SubPathSlashRemove(string subPath = "/");

        RelativeObjects GetNextPrevInFolder(string currentFolder);
    }
}
