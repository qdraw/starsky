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
	    private readonly IStorage _iStorage;
	    private readonly IReadMeta _readMeta;

	    public LoopPublications(IStorage iStorage, AppSettings appSettings, IExifTool exifTool, IReadMeta readMeta)
	    {
		    _iStorage = iStorage;
            _appSettings = appSettings;
            _exifTool = exifTool;
		    _readMeta = readMeta;
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

                  
            var embeddedResult = new ParseRazor().EmbeddedViews(profile.Template,viewModel).Result;

	        var stream = new PlainTextFileHelper().StringToStream(embeddedResult);
	        _iStorage.WriteStream(stream, profile.Path);

            Console.WriteLine(embeddedResult);
        }

        private void GenerateJpeg(AppSettingsPublishProfiles profile, List<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_iStorage, _appSettings,_exifTool);

            foreach (var item in fileIndexItemsList)
            {

                var outputPath = overlayImage.FilePathOverlayImage(item.FilePath, profile);
                        
                // for less than 1000px
                if (profile.SourceMaxWidth <= 1000)
                {
	                overlayImage.ResizeOverlayImageThumbnails(item.FileHash, outputPath, profile);
                }
                else
                {
	                // Thumbs are 1000 px (and larger)
	                overlayImage.ResizeOverlayImageLarge(item.FilePath, outputPath, profile);
                }
                            
	            if ( profile.MetaData )
	            {
		            new ExifCopy(_iStorage, _exifTool, _readMeta).CopyExifPublish(item.FilePath,
			            outputPath);
	            }

            }
        }

        private void GenerateMoveSourceFiles(AppSettingsPublishProfiles profile, List<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_iStorage, _appSettings,_exifTool);

            foreach (var item in fileIndexItemsList)
            {
	            // input: item.FilePath
                var outputPath = overlayImage.FilePathOverlayImage(item.FilePath, profile);
	            _iStorage.FileMove(item.FilePath,outputPath);
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

	        if ( _iStorage.IsFolderOrFile(profileFolderStringBuilder.ToString()) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
	        {
		        _iStorage.CreateDirectory(profileFolderStringBuilder.ToString());
	        }
	        
        }

        
    }
}