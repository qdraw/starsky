using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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

        // do the query for singleitem + return detailview object
        public DetailView SingleItem(
            string singleItemDbPath,
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true)
        {
            if (string.IsNullOrWhiteSpace(singleItemDbPath) ) return null;
            var parentFolder = Breadcrumbs.BreadcrumbHelper(singleItemDbPath).LastOrDefault();
            var fileIndexItemsList = DisplayFileFolders(
                parentFolder,null,false,false).ToList();

            return SingleItem(
                fileIndexItemsList,
                singleItemDbPath,
                colorClassFilterList,
                enableCollections,
                hideDeleted);
        }

        // Create an detailview object
        public DetailView SingleItem(
            List<FileIndexItem> fileIndexItemsList, 
            string singleItemDbPath,
            List<FileIndexItem.Color> colorClassFilterList = null,
            bool enableCollections = true,
            bool hideDeleted = true)
        {
            // reject emphy requests
            if (string.IsNullOrWhiteSpace(singleItemDbPath) ) return null;
            var parentFolder = Breadcrumbs.BreadcrumbHelper(singleItemDbPath).LastOrDefault();

            // RemoveLatestSlash is for '/' folder
            var fileName = singleItemDbPath.Replace(
                ConfigRead.RemoveLatestSlash(parentFolder) + "/", string.Empty);

            var currentFileIndexItem = fileIndexItemsList.FirstOrDefault(p => p.FileName == fileName);
            if (currentFileIndexItem == null) return null;

            if(currentFileIndexItem.IsDirectory) return new DetailView
            {
                IsDirectory = true,
                SubPath = singleItemDbPath
            };

            if (currentFileIndexItem.Tags.Contains("!delete!")) hideDeleted = false;
            
            var fileIndexItemsForPrevNextList = DisplayFileFolders(
                parentFolder,colorClassFilterList,enableCollections,hideDeleted).ToList();

            var itemResult = new DetailView
            {
                FileIndexItem = currentFileIndexItem,
                RelativeObjects = GetNextPrevInSubFolder(currentFileIndexItem,fileIndexItemsForPrevNextList),
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItemDbPath),
                GetAllColor = FileIndexItem.GetAllColorUserInterface(),
                ColorClassFilterList = colorClassFilterList,
                IsDirectory = false,
                SubPath = singleItemDbPath,
            };

            // First item is current item
            var collectionPaths = new List<string> {singleItemDbPath};
            collectionPaths.AddRange(fileIndexItemsList
                .Where(p => p.FileCollectionName == currentFileIndexItem.FileCollectionName)
                .Select(p => p.FilePath));
            itemResult.FileIndexItem.CollectionPaths = collectionPaths.ToHashSet().ToList(); 
            
            return itemResult;
        }
        
        private RelativeObjects GetNextPrevInSubFolder(FileIndexItem currentFileIndexItem, 
            List<FileIndexItem> fileIndexItemsList)
        {
            // Check if this is item is not !deleted! yet
            if (currentFileIndexItem == null) return new RelativeObjects();
            
            var currentIndex = fileIndexItemsList.FindIndex(p => p.FilePath == currentFileIndexItem.FilePath);
            var relativeObject = new RelativeObjects();

            if (currentIndex != fileIndexItemsList.Count - 1)
            {
                relativeObject.NextFilePath = fileIndexItemsList[currentIndex + 1].FilePath;
            }
            if (currentIndex >= 1)
            {
                relativeObject.PrevFilePath = fileIndexItemsList[currentIndex - 1].FilePath;
            }
            
            return relativeObject;
        }
  
        
    }
}
