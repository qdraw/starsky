using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query // For folder displays only
    {
        public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", 
            IEnumerable<FileIndexItem.Color> colorClassFilterList = null)
        {
            subPath = SubPathSlashRemove(subPath);
            List<FileIndexItem> queryItems;
            
            if (colorClassFilterList == null)
            {
                queryItems = _context.FileIndex
                    .Where(p => p.ParentDirectory == subPath)
                    .OrderBy(p => p.FileName).ToList();     
            }
            else
            {
                queryItems = _context.FileIndex
                    .Where(p => p.ParentDirectory == subPath &&
                                colorClassFilterList.Contains(p.ColorClass))
                    .OrderBy(p => p.FileName).ToList();  
            }
            
            if (!queryItems.Any())
            {
                return new List<FileIndexItem>();
            }
            return _hideDeletedFileFolderList(queryItems);
        }

        private static IEnumerable<FileIndexItem> _hideDeletedFileFolderList(List<FileIndexItem> queryItems){
            // temp feature to hide deleted items
            var displayItems = new List<FileIndexItem>();
                foreach (var item in queryItems)
            {
                if (item.Tags == null)
                {
                    item.Tags = string.Empty;
                }
    
                if (!item.Tags.Contains("!delete!"))
                {
                    displayItems.Add(item);
                }
            }
            return displayItems;
            // temp feature to hide deleted items
        }
        
        
        public RelativeObjects GetNextPrevInFolder(string currentFolder)
        {
            currentFolder = SubPathSlashRemove(currentFolder);

            var parrentFolderPathArray = currentFolder.Split("/");
            var parrentFolderPath = String.Empty;
            for (int i = 1; i < parrentFolderPathArray.Length; i++)
            {
                if (i <= parrentFolderPathArray.Length-2)
                {
                    parrentFolderPath = parrentFolderPath + "/" + parrentFolderPathArray[i];
                }
            }

            var itemsInSubFolder = _context.FileIndex
                .Where(p => p.ParentDirectory == parrentFolderPath)
                .OrderBy(p => p.FileName).ToList();
            
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == currentFolder);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1 && currentFolder != "/")
            {
                // on the home folder you will automaticly go to a subfolder
                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
            }

            if (photoIndexOfSubFolder >= 1)
            {
                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
            }

            return relativeObject;
        }
    }
}