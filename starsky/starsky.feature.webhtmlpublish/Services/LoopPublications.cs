using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;

namespace starsky.feature.webhtmlpublish.Services
{
    public class LoopPublications
    {

        private readonly AppSettings _appSettings;
        private readonly IExifTool _exifTool;
	    private readonly IStorage _iStorage;
	    private readonly IReadMeta _readMeta;
	    private readonly IStorage _thumbnailStorage;
	    private readonly ISelectorStorage _selectorStorage;
	    private readonly IStorage _hostFileSystemStorage;
	    private readonly IConsole _console;

	    public LoopPublications(ISelectorStorage selectorStorage, AppSettings appSettings, 
		    IExifTool exifTool, IReadMeta readMeta, IConsole console)
	    {
		    _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		    _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		    _hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		    _selectorStorage = selectorStorage;
            _appSettings = appSettings;
            _exifTool = exifTool;
		    _readMeta = readMeta;
		    _console = console;
	    }

        public void Render(List<FileIndexItem> fileIndexItemsList, string[] base64ImageArray, string publishProfileName)
        {
	        if ( !_appSettings.PublishProfiles.Any() )
	        {
		        _console.WriteLine("There are no config items");
		        return;
	        }
	        
            if ( !_appSettings.PublishProfiles.ContainsKey(publishProfileName) )
            {
	            _console.WriteLine("Key not found");
	            return;
            }
            
            if(base64ImageArray == null) base64ImageArray = new string[fileIndexItemsList.Count];
            
            // Order alphabetically
            fileIndexItemsList = fileIndexItemsList.OrderBy(p => p.FileName).ToList();

            var profiles = _appSettings.PublishProfiles
	            .FirstOrDefault(p => p.Key == publishProfileName).Value;
            foreach (var currentProfile in profiles)
            {
                switch (currentProfile.ContentType)
                {
                    case TemplateContentType.Html:
                        GenerateWebHtml(profiles, currentProfile,base64ImageArray,fileIndexItemsList);
                        break;
                    case TemplateContentType.Jpeg:
                        GenerateJpeg(currentProfile,fileIndexItemsList);
                        break;
                    case TemplateContentType.MoveSourceFiles:
                        GenerateMoveSourceFiles(currentProfile,fileIndexItemsList);
                        break;
                }
            }
        }

        private void GenerateWebHtml(List<AppSettingsPublishProfiles> profiles, AppSettingsPublishProfiles currentProfile, string[] base64ImageArray, 
	        IEnumerable<FileIndexItem> fileIndexItemsList)
        {
            // Generates html by razorLight
            var viewModel = new WebHtmlViewModel
            {
	            Profiles = profiles,
                AppSettings = _appSettings,
                CurrentProfile = currentProfile,
                Base64ImageArray = base64ImageArray,
                // apply slug to items, but use it only in the copy
                FileIndexItems = fileIndexItemsList.Select(c => c.Clone()).ToList(),
            };

            // add to IClonable
            foreach (var item in viewModel.FileIndexItems)
            {
                item.FileName = _appSettings.GenerateSlug(item.FileCollectionName, true) + 
                                Path.GetExtension(item.FileName);
            }
                  
            var embeddedResult = new ParseRazor(_hostFileSystemStorage)
	            .EmbeddedViews(currentProfile.Template, viewModel).Result;

	        var stream = new PlainTextFileHelper().StringToStream(embeddedResult);
	        _iStorage.WriteStream(stream, currentProfile.Path);

	        if ( _appSettings.Verbose ) _console.WriteLine(embeddedResult);
        }

        private void GenerateJpeg(AppSettingsPublishProfiles profile, 
	        IReadOnlyCollection<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_selectorStorage,_appSettings);

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
		            new ExifCopy(_iStorage, _thumbnailStorage, _exifTool, _readMeta)
			            .CopyExifPublish(item.FilePath, outputPath);
	            }
            }
        }

        private void GenerateMoveSourceFiles(AppSettingsPublishProfiles profile, 
	        IReadOnlyCollection<FileIndexItem> fileIndexItemsList)
        {
            ToCreateSubfolder(profile,fileIndexItemsList.FirstOrDefault()?.ParentDirectory);
            var overlayImage = new OverlayImage(_selectorStorage, _appSettings);

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

	        if ( _iStorage.IsFolderOrFile(profileFolderStringBuilder.ToString()) 
	             == FolderOrFileModel.FolderOrFileTypeList.Deleted)
	        {
		        _iStorage.CreateDirectory(profileFolderStringBuilder.ToString());
	        }
        }
    }
}
