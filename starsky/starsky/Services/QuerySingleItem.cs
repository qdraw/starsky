using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var relativeObject = _getNextPrevInSubFolder(query?.ParentDirectory, singleItemDbPath, colorClassFilterList);

            var itemResult = new DetailView
            {
                FileIndexItem = query,
                RelativeObjects = relativeObject
            };

            return itemResult;
        }


        private RelativeObjects _getNextPrevInSubFolder(
            string parrentFolderPath, string fullImageFilePath,
            IEnumerable<FileIndexItem.Color> colorClassFilterList = null
            )
        {
            List<FileIndexItem> itemsInSubFolder;
            if (colorClassFilterList == null)
            {
                itemsInSubFolder = GetAllFiles(parrentFolderPath).
                    Where(p => !p.Tags.Contains("!delete!")). // Hide Deleted items from prev/next
                    OrderBy(
                        p => p.FileName
                    ).ToList();
            }
            else
            {
                itemsInSubFolder = GetAllFiles(parrentFolderPath).Where(
                        p => colorClassFilterList.Contains(p.ColorClass) &&
                             !p.Tags.Contains("!delete!") // Hide Deleted items from prev/next
                    ).OrderBy(
                    p => p.FileName).ToList();
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
