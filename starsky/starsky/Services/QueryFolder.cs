using System.Collections.Generic;
using System.Linq;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query // For folder displays only
    {
        // Class for displaying folder content
        
        // Display File folder displays content of the folder
        public IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/", 
            List<FileIndexItem.Color> colorClassFilterList = null)
        {
            if (colorClassFilterList == null) colorClassFilterList = new List<FileIndexItem.Color>();
            
            subPath = SubPathSlashRemove(subPath);
            List<FileIndexItem> queryItems;
            
            if (!colorClassFilterList.Any())
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

        // Hide Deleted items in folder
        private static IEnumerable<FileIndexItem> _hideDeletedFileFolderList(List<FileIndexItem> queryItems){
            // temp feature to hide deleted items
            var displayItems = new List<FileIndexItem>();
                foreach (var item in queryItems)
            {
                if (!item.Tags.Contains("!delete!"))
                {
                    displayItems.Add(item);
                }
            }
            return displayItems;
            // temp feature to hide deleted items
        }
        
        // Show previous en next items in the folder view.
        // There is equivalent class for prev next in the display view
        public RelativeObjects GetNextPrevInFolder(string currentFolder)
        {
            currentFolder = SubPathSlashRemove(currentFolder);

            // We use breadcrums to get the parent folder
            var parrentFolderPath = Breadcrumbs.BreadcrumbHelper(currentFolder).LastOrDefault();
            
            var itemsInSubFolder = _context.FileIndex
                .Where(p => p.ParentDirectory == parrentFolderPath)
                .OrderBy(p => p.FileName).ToList();
            
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == currentFolder);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1 && currentFolder != "/")
            {
                // currentFolder != "/" >= on the home folder you will automaticly go to a subfolder
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