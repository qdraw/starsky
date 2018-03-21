using System;
using System.Collections.Generic;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface IUpdate
    {

        List<FileIndexItem> GetAllFiles(string subPath = "");

        IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/");

        ObjectItem SingleItem(string singleItemDbPath);

        FileIndexItem GetObjectByFilePath(string filePath);

        FileIndexItem RemoveItem(FileIndexItem updateStatusContent);

        string GetItemByHash(string fileHash);


        FileIndexItem AddItem(FileIndexItem updateStatusContent);
        FileIndexItem UpdateItem(FileIndexItem updateStatusContent);

        string SubPathSlashRemove(string subPath = "/");


    }
}
