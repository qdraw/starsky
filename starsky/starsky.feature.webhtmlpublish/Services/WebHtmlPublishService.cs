using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.webhtmlpublish.Services
{
	[Service(typeof(IWebHtmlPublishService), InjectionLifetime = InjectionLifetime.Scoped)]
    public class WebHtmlPublishService : IWebHtmlPublishService
    {
        private readonly AppSettings _appSettings;
        private readonly IExifTool _exifTool;
	    private readonly IStorage _subPathStorage;
	    private readonly IStorage _thumbnailStorage;
	    private readonly ISelectorStorage _selectorStorage;
	    private readonly IStorage _hostFileSystemStorage;
	    private readonly IConsole _console;
	    private readonly IOverlayImage _overlayImage;
	    private readonly PublishManifest _publishManifest;
	    private readonly IPublishPreflight _publishPreflight;

	    public WebHtmlPublishService(IPublishPreflight publishPreflight, ISelectorStorage selectorStorage, AppSettings appSettings, 
		    IExifTool exifTool, IOverlayImage overlayImage, IConsole console)
	    {
		    _publishPreflight = publishPreflight;
		    _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		    _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		    _hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		    _selectorStorage = selectorStorage;
            _appSettings = appSettings;
            _exifTool = exifTool;
		    _console = console;
		    _overlayImage = overlayImage;
		    _publishManifest = new PublishManifest(_publishPreflight, _hostFileSystemStorage, _appSettings,
			    new PlainTextFileHelper());
	    }
	    
	    public async Task<bool> RenderCopy(List<FileIndexItem> fileIndexItemsList, 
		    string[] base64ImageArray, string publishProfileName, string itemName, string outputFullFilePath,
		    bool moveSourceFiles = false)
	    {
		    var render = await Render(fileIndexItemsList, base64ImageArray, publishProfileName, itemName, outputFullFilePath, moveSourceFiles);

		    _publishManifest.ExportManifest(outputFullFilePath, itemName, publishProfileName);
		    
		    // Copy all items in the subFolder content for example JavaScripts
		    new Content(_subPathStorage).CopyPublishedContent();
		    return render;
	    }
	    
        public async Task<bool> Render(List<FileIndexItem> fileIndexItemsList,
	        string[] base64ImageArray, string publishProfileName, string itemName, 
	        string outputParentFullFilePathFolder, bool moveSourceFiles = false)
        {
	        if ( !_appSettings.PublishProfiles.Any() )
	        {
		        _console.WriteLine("There are no config items");
		        return false;
	        }
	        
            if ( !_appSettings.PublishProfiles.ContainsKey(publishProfileName) )
            {
	            _console.WriteLine("Key not found");
	            return false;
            }
            
            if(base64ImageArray == null) base64ImageArray = new string[fileIndexItemsList.Count];
            
            // Order alphabetically
            fileIndexItemsList = fileIndexItemsList.OrderBy(p => p.FileName).ToList();

			var profiles = _publishPreflight.GetPublishProfileName(publishProfileName);
            foreach (var currentProfile in profiles)
            {
                switch (currentProfile.ContentType)
                {
                    case TemplateContentType.Html:
                        await GenerateWebHtml(profiles, currentProfile, itemName, base64ImageArray, fileIndexItemsList, outputParentFullFilePathFolder);
                        break;
                    case TemplateContentType.Jpeg:
                        GenerateJpeg(currentProfile, fileIndexItemsList, outputParentFullFilePathFolder);
                        break;
                    case TemplateContentType.MoveSourceFiles:
                        await GenerateMoveSourceFiles(currentProfile,fileIndexItemsList, outputParentFullFilePathFolder, moveSourceFiles);
                        break;
                }
            }
            return true;
        }

        private async Task GenerateWebHtml(List<AppSettingsPublishProfiles> profiles, 
	        AppSettingsPublishProfiles currentProfile, string itemName, string[] base64ImageArray, 
	        IEnumerable<FileIndexItem> fileIndexItemsList, string outputParentFullFilePathFolder)
        {
            // Generates html by razorLight
            var viewModel = new WebHtmlViewModel
            {
	            ItemName = itemName,
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
            
            // has a direct dependency on the filesystem
            var embeddedResult = await new ParseRazor(_hostFileSystemStorage)
	            .EmbeddedViews(currentProfile.Template, viewModel);

	        var stream = new PlainTextFileHelper().StringToStream(embeddedResult);
	        await _hostFileSystemStorage.WriteStreamAsync(stream, Path.Combine(outputParentFullFilePathFolder, currentProfile.Path));

	        _console.Write(_appSettings.Verbose ? embeddedResult +"\n" : "•");
        }

        private void GenerateJpeg(AppSettingsPublishProfiles profile, 
	        IReadOnlyCollection<FileIndexItem> fileIndexItemsList, string outputParentFullFilePathFolder)
        {
	        ToCreateSubfolder(profile,outputParentFullFilePathFolder);

            foreach (var item in fileIndexItemsList)
            {
                var outputPath = _overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder, item.FilePath, profile);
                        
                // for less than 1000px
                if (profile.SourceMaxWidth <= 1000)
                {
	                _overlayImage.ResizeOverlayImageThumbnails(item.FileHash, outputPath, profile);
                }
                else
                {
	                // Thumbs are 1000 px (and larger)
	                _overlayImage.ResizeOverlayImageLarge(item.FilePath, outputPath, profile);
                }
                            
	            if ( profile.MetaData )
	            {
		            // Write the metadata to the new created file
		            var comparedNames = FileIndexCompareHelper.Compare(new FileIndexItem(), item);
		            comparedNames.Add(nameof(FileIndexItem.Software));
		            new ExifToolCmdHelper(_exifTool,_hostFileSystemStorage, _thumbnailStorage, null)
			            .Update(item, comparedNames, false);
	            }
            }
        }

        private async Task GenerateMoveSourceFiles(AppSettingsPublishProfiles profile, 
	        IReadOnlyCollection<FileIndexItem> fileIndexItemsList, string outputParentFullFilePathFolder, bool moveSourceFiles)
        {
            ToCreateSubfolder(profile,outputParentFullFilePathFolder);
            
            var overlayImage = new OverlayImage(_selectorStorage, _appSettings);

            foreach (var item in fileIndexItemsList)
            {
	            // input: item.FilePath
                var outputPath = overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder, item.FilePath, profile);

                await _hostFileSystemStorage.WriteStreamAsync(_subPathStorage.ReadStream(item.FilePath),
	                outputPath);
                
                // only delete when using in cli mode
                if ( moveSourceFiles )
                {
	                _subPathStorage.FileDelete(item.FilePath);
                }
            }
        }

        /// <summary>
        /// Create SubFolders by the profile.Folder setting
        /// </summary>
        /// <param name="profile">config</param>
        /// <param name="parentFolder">root folder</param>
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

	        if ( _hostFileSystemStorage.IsFolderOrFile(profileFolderStringBuilder.ToString()) 
	             == FolderOrFileModel.FolderOrFileTypeList.Deleted)
	        {
		        _hostFileSystemStorage.CreateDirectory(profileFolderStringBuilder.ToString());
	        }
        }
    }
}
