using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query
    {
        // For displaying single photo's
        // input: Name of item by db style path
        public ObjectItem SingleItem(string singleItemDbPath)
        {
            if (string.IsNullOrWhiteSpace(singleItemDbPath)) return null;

            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == singleItemDbPath && !p.IsDirectory);

            if (query == null) return null;

            var relativeObject = _getNextPrevInSubFolder(query?.ParentDirectory, singleItemDbPath);

            var itemResult = new ObjectItem
            {
                FileIndexItem = query,
                RelativeObjects = relativeObject
            };

            return itemResult;
        }


        private RelativeObjects _getNextPrevInSubFolder(string parrentFolderPath, string fullImageFilePath)
        {
            var itemsInSubFolder = GetAllFiles(parrentFolderPath).OrderBy(p => p.FileName).ToList();
            var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == fullImageFilePath);

            var relativeObject = new RelativeObjects();
            if (photoIndexOfSubFolder != itemsInSubFolder.Count - 1)
            {
                relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1]?.FilePath;
            }

            if (photoIndexOfSubFolder != 0)
            {
                relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1]?.FilePath;
            }
            return relativeObject;
        }
    }
}
