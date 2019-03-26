using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskywebhtmlcli.ViewModels;

namespace starskywebhtmlcli.Services
{
    public class LoopPublications
    {

        private readonly AppSettings _appSettings;
        private readonly IExifTool _exifTool;

        public LoopPublications(AppSettings appSettings, IExifTool exifTool)
        {
            _appSettings = appSettings;
            _exifTool = exifTool;
        }

        public void Render(List<FileIndexItem> fileIndexItemsList, string[] base64ImageArray)
        {
            if(!_appSettings.PublishProfiles.Any()) Console.WriteLine("There are no config items");
            if(base64ImageArray == null) base64ImageArray = new string[fileIndexItemsList.Count];
            
            // Order alphabetcly
            fileIndexItemsList = fileIndexItemsList.OrderBy(p => p.FileName).ToList();
            
            foreach (var profile in _appSettings.PublishProfiles)
            {
                switch (profile.ContentType)
                {
                    case TemplateContentType.Html:
                        GenerateWebHtml(profile,base64ImageArray,fileIndexItemsList);
                        break;
                    case TemplateContentType.Jpeg:
                        GenerateJpeg(profile,fileIndexItemsList);
                        break;
                    case TemplateContentType.MoveSourceFiles:
                        GenerateMoveSourceFiles(profile,fileIndexItemsList);
                        break;
                }
            }
        }

        private void GenerateWebHtml(AppSettingsPublishProfiles profile,string[] base64ImageArray, List<FileIndexItem> fileIndexItemsList)
        {
            // Generates html by razorLight
            var viewModel = new WebHtmlViewModel
            {
                AppSettings = _appSettings,
                Profile = profile,
                Base64ImageArray = base64ImageArray,
                // apply slug to items, but use it only in the copy
                FileIndexItems = fileIndexItemsList.Select(c => c.Clone()).ToList(),
            };

            // add to IClonable
            foreach (var item in viewModel.FileIndexItems)
            {
                item.FileName = _appSettings.GenerateSlug(item.FileCollectionName, true) + Path.GetExtension(item.FileName);
            }

            // Files.DeleteFile(profile.Path)
                    
            var embeddedResult = new ParseRazor().EmbeddedViews(profile.Template,viewModel).Result;
            new PlainTextFileHelper().WriteFile(_appSettings.StorageFolder 
                                                + profile.Path, embeddedResult);
            Console.WriteLine(embeddedResult);
        }

        private void GenerateJpeg(AppSettingsPublishProfiles profile, List<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_appSettings,_exifTool);

            foreach (var item in fileIndexItemsList)
            {

                var fullFilePath = _appSettings.DatabasePathToFilePath(item.FilePath);

                var outputFilePath = overlayImage.FilePathOverlayImage(fullFilePath, profile);
                        
                // for less than 1000px
                if (profile.SourceMaxWidth <= 1000)
                {
	                throw new NotImplementedException();
//                    var inputFullFilePath = new Thumbnail(_appSettings).GetThumbnailPath(item.FileHash);
//                    new OverlayImage(_appSettings,_exifTool).ResizeOverlayImage(
//                        inputFullFilePath, outputFilePath,profile);
                }
                            
                // Thumbs are 1000 px
                if (profile.SourceMaxWidth > 1000)
                {
                    overlayImage.ResizeOverlayImage(fullFilePath, outputFilePath, profile);
                }

            }
        }

        private void GenerateMoveSourceFiles(AppSettingsPublishProfiles profile, List<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_appSettings,_exifTool);

            foreach (var item in fileIndexItemsList)
            {
                var fullFilePath = _appSettings.DatabasePathToFilePath(item.FilePath);
                var outputFilePath = overlayImage.FilePathOverlayImage(fullFilePath, profile);

                File.Move(fullFilePath, outputFilePath);
                item.ParentDirectory = profile.Folder;
            }
        }

        private void ToCreateSubfolder(AppSettingsPublishProfiles profile, string parentFolder)
        {
            // check if subfolder '1000' exist on disk
            // used for moving subfolders first
            var profileFolderStringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(parentFolder))
            {
                profileFolderStringBuilder.Append(parentFolder);
                profileFolderStringBuilder.Append("/");
            }
            profileFolderStringBuilder.Append(profile.Folder);
            var toCreateSubfolder = _appSettings.DatabasePathToFilePath(profileFolderStringBuilder.ToString(), false);

            if (FilesHelper.IsFolderOrFile(toCreateSubfolder) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
            {
                Directory.CreateDirectory(toCreateSubfolder);
            }
        }

        
    }
}