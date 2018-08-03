﻿using System;
using System.Collections.Generic;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface IQuery
    {

        List<FileIndexItem> GetAllFiles(string subPath = "");
        
        List<FileIndexItem> GetAllRecursive(string subPath = "");

        IEnumerable<FileIndexItem> DisplayFileFolders(
            string subPath = "/", 
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        DetailView SingleItem(string singleItemDbPath, 
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true);

        FileIndexItem GetObjectByFilePath(string filePath);

        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);

        string GetItemByHash(string fileHash);


        FileIndexItem AddItem(FileIndexItem updateStatusContent);
        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);

        string SubPathSlashRemove(string subPath = "/");

        RelativeObjects GetNextPrevInFolder(string currentFolder);

        List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList);
    }
}
