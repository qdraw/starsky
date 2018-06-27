using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public partial class Query
    {
        // For displaying single photo's
        // Display feature only?!
        // input: Name of item by db style path
        // With Caching feature :)
        public DetailView SingleItem(string singleItemDbPath,
            List<FileIndexItem.Color> colorClassFilterList = null)
        {
            if (string.IsNullOrWhiteSpace(singleItemDbPath)) return null;

            // For creating an unique name: DetailView_/2018/01/1.jpg_Superior
            var uniqueSingleDbCacheName = "DetailView_" + singleItemDbPath;
            if (colorClassFilterList != null)
            {
                uniqueSingleDbCacheName += "_";
                foreach (var oneColor in colorClassFilterList)
                {
                    uniqueSingleDbCacheName += oneColor.ToString();
                }
            }

            // Return values from IMemoryCache
            if (_cache.TryGetValue(uniqueSingleDbCacheName, out var itemResult)) return itemResult as DetailView;
            
            var query = _context.FileIndex.FirstOrDefault(p => p.FilePath == singleItemDbPath && !p.IsDirectory);

            if (query == null) return null;

            var relativeObject = _getNextPrevInSubFolder(query.ParentDirectory, singleItemDbPath, colorClassFilterList);

            itemResult = new DetailView
            {
                FileIndexItem = query,
                RelativeObjects = relativeObject,
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItemDbPath),
                GetAllColor = FileIndexItem.GetAllColorUserInterface(),
                ColorClassFilterList = colorClassFilterList
            };
            
            // Cache with 1 hour timespan
            _cache.Set(singleItemDbPath, itemResult, new TimeSpan(1,0,0));

            // Cast object to DetailView
            return (DetailView) itemResult;
        }

        // Show previous en next items in the singleitem view.
        // There is equivalent class (GetNextPrevInFolder) for prev next in the folder view
        private RelativeObjects _getNextPrevInSubFolder(
            string parrentFolderPath, string fullImageFilePath,
            List<FileIndexItem.Color> colorClassFilterList = null
            )
        {
            if (colorClassFilterList == null) colorClassFilterList = new List<FileIndexItem.Color>();

            List<FileIndexItem> itemsInSubFolder;
            // Filter usage colorClassFilterList
            if (!colorClassFilterList.Any())
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
