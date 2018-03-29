using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query
    {
        // For displaying single photo's
        // input: Name of item by db style path
        public DetailView SingleItem(string singleItemDbPath,
            IEnumerable<FileIndexItem.Color> colorClassFilterList = null)
        {
            if (string.IsNullOrWhiteSpace(singleItemDbPath)) return null;

            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == singleItemDbPath && !p.IsDirectory);

            if (query == null) return null;

            var relativeObject = _getNextPrevInSubFolder(query.ParentDirectory, singleItemDbPath, colorClassFilterList);

            var itemResult = new DetailView
            {
                FileIndexItem = query,
                RelativeObjects = relativeObject
            };

            return itemResult;
        }

        // Show previous en next items in the singleitem view.
        // There is equivalent class (GetNextPrevInFolder) for prev next in the folder view
        private RelativeObjects _getNextPrevInSubFolder(
            string parrentFolderPath, string fullImageFilePath,
            IEnumerable<FileIndexItem.Color> colorClassFilterList = null
            )
        {
            List<FileIndexItem> itemsInSubFolder;
            // Filter usage colorClassFilterList
            if (colorClassFilterList == null)
            {
                itemsInSubFolder = GetAllFiles(parrentFolderPath).
                    OrderBy(
                        p => p.FileName
                    ).ToList();
            }
            else
            {
                itemsInSubFolder = GetAllFiles(parrentFolderPath).Where(
                        p => colorClassFilterList.Contains(p.ColorClass) 
                ).OrderBy(
                    p => p.FileName).ToList();
            }
            
            // todo: if image it self is deleted and page is refreshed
            // Use only if you are sure that there are no nulls
            if (itemsInSubFolder.Count >= 1)
            {
                itemsInSubFolder =
                    itemsInSubFolder
                        .Where(p => !p.Tags.Contains("!delete!"))
                        .ToList(); // Hide Deleted items from prev/next
            }
            
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == fullImageFilePath);
            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1)
            {
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
