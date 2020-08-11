using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Extensions;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]
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
	    private readonly CopyPublishedContent _copyPublishedContent;
	    private readonly ToCreateSubfolder _toCreateSubfolder;
	    private readonly Thumbnail _thumbnailService;

	    public WebHtmlPublishService(IPublishPreflight publishPreflight, ISelectorStorage 
			    selectorStorage, AppSettings appSettings, IExifTool exifTool, 
		    IOverlayImage overlayImage, IConsole console)
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
		    _publishManifest = new PublishManifest(_hostFileSystemStorage, new PlainTextFileHelper());
		    _toCreateSubfolder = new ToCreateSubfolder(_hostFileSystemStorage);
		    _copyPublishedContent = new CopyPublishedContent(_appSettings, _toCreateSubfolder, 
			    selectorStorage);
		    _thumbnailService = new Thumbnail(_subPathStorage,_thumbnailStorage);
	    }
	    
	    public async Task<Dictionary<string, bool>> RenderCopy(List<FileIndexItem> fileIndexItemsList,
		    string publishProfileName, string itemName, string outputParentFullFilePathFolder,
		    bool moveSourceFiles = false)
	    {
		    fileIndexItemsList = AddFileHashIfNotExist(fileIndexItemsList);
			    
		    PreGenerateThumbnail(fileIndexItemsList);
		    var base64ImageArray = Base64DataUriList(fileIndexItemsList);
		    
		    var copyContent = await Render(fileIndexItemsList, base64ImageArray, 
			    publishProfileName, itemName, outputParentFullFilePathFolder, moveSourceFiles);

		    _publishManifest.ExportManifest(outputParentFullFilePathFolder, itemName, copyContent);

		    return copyContent;
	    }

	    internal List<FileIndexItem> AddFileHashIfNotExist(List<FileIndexItem> fileIndexItemsList)
	    {
		    foreach ( var item in fileIndexItemsList.Where(item => item.FileHash == null) )
		    {
			    item.FileHash = new FileHash(_subPathStorage).GetHashCode(item.FilePath).Key;
		    }
		    return fileIndexItemsList;
	    }

	    internal void PreGenerateThumbnail(IEnumerable<FileIndexItem> fileIndexItemsList)
	    {
		    foreach ( var item in fileIndexItemsList )
		    {
			    _thumbnailService.CreateThumb(item.FilePath, item.FileHash);
		    }
	    }

	    /// <summary>
	    /// Get base64 uri lists 
	    /// </summary>
	    /// <returns></returns>
	    private string[] Base64DataUriList(IEnumerable<FileIndexItem> fileIndexItemsList)
	    {
		    return new ToBase64DataUriList(_subPathStorage, _thumbnailStorage).Create(fileIndexItemsList.ToList());
	    }
	    
	    public async Task<Dictionary<string,bool>> Render(List<FileIndexItem> fileIndexItemsList,
		    string[] base64ImageArray, string publishProfileName, string itemName, 
		    string outputParentFullFilePathFolder, bool moveSourceFiles = false)
	    {
		    if ( !_appSettings.PublishProfiles.Any() )
		    {
			    _console.WriteLine("There are no config items");
			    return null;
		    }
	        
		    if ( !_appSettings.PublishProfiles.ContainsKey(publishProfileName) )
		    {
			    _console.WriteLine("Key not found");
			    return null;
		    }
		    
		    if ( !_hostFileSystemStorage.ExistFolder(outputParentFullFilePathFolder) )
		    {
			    _hostFileSystemStorage.CreateDirectory(outputParentFullFilePathFolder);
		    }
            
		    if(base64ImageArray == null) base64ImageArray = new string[fileIndexItemsList.Count];
            
		    // Order alphabetically
		    fileIndexItemsList = fileIndexItemsList.OrderBy(p => p.FileName).ToList();

		    var copyResult = new Dictionary<string,bool>();
            
		    var profiles = _publishPreflight.GetPublishProfileName(publishProfileName);
		    foreach (var currentProfile in profiles)
		    {
			    switch (currentProfile.ContentType)
			    {
				    case TemplateContentType.Html:
					    copyResult.AddRangeOverride(await GenerateWebHtml(profiles, currentProfile, itemName, 
						    base64ImageArray, fileIndexItemsList, outputParentFullFilePathFolder));
					    break;
				    case TemplateContentType.Jpeg:
					    copyResult.AddRangeOverride(GenerateJpeg(currentProfile, fileIndexItemsList, 
						    outputParentFullFilePathFolder));
					    break;
				    case TemplateContentType.MoveSourceFiles:
					    copyResult.AddRangeOverride(await GenerateMoveSourceFiles(currentProfile,fileIndexItemsList, 
						    outputParentFullFilePathFolder, moveSourceFiles));
					    break;
				    case TemplateContentType.PublishContent:
					    // Copy all items in the subFolder content for example JavaScripts
					    copyResult.AddRangeOverride(_copyPublishedContent.CopyContent(currentProfile, outputParentFullFilePathFolder));
					    break;
				    case TemplateContentType.PublishManifest:
					    copyResult.Add(
						    _overlayImage.FilePathOverlayImage("_settings.json", currentProfile)
						    ,true);
					    break;
			    }
		    }
		    return copyResult;
	    }

	    internal async Task<Dictionary<string, bool>> GenerateWebHtml(List<AppSettingsPublishProfiles> profiles, 
		    AppSettingsPublishProfiles currentProfile, string itemName, string[] base64ImageArray, 
		    IEnumerable<FileIndexItem> fileIndexItemsList, string outputParentFullFilePathFolder)
	    {
		    if ( string.IsNullOrEmpty(currentProfile.Template) )
		    {
			    _console.WriteLine("CurrentProfile Template not configured");
			    return new Dictionary<string, bool>();
		    }
		    
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
		    await _hostFileSystemStorage.WriteStreamAsync(stream, 
			    Path.Combine(outputParentFullFilePathFolder, currentProfile.Path));

		    _console.Write(_appSettings.Verbose ? embeddedResult +"\n" : "•");

		    return new Dictionary<string, bool>
		    {
			    {
				    currentProfile.Path.Replace(outputParentFullFilePathFolder, string.Empty),
				    currentProfile.Copy
			    }
		    };
	    }

	    private Dictionary<string, bool> GenerateJpeg(AppSettingsPublishProfiles profile, 
		    IReadOnlyCollection<FileIndexItem> fileIndexItemsList, string outputParentFullFilePathFolder)
	    {
		    _toCreateSubfolder.Create(profile,outputParentFullFilePathFolder);

		    foreach (var item in fileIndexItemsList)
		    {
			    var outputPath = _overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder, 
				    item.FilePath, profile);
                        
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
				    var comparedNames = FileIndexCompareHelper.Compare(
					    new FileIndexItem(), item);
				    comparedNames.Add(nameof(FileIndexItem.Software));
				    new ExifToolCmdHelper(_exifTool,_hostFileSystemStorage, 
					    _thumbnailStorage, null).Update(item, comparedNames, false);
			    }
		    }

		    return fileIndexItemsList.ToDictionary(item =>
			    _overlayImage.FilePathOverlayImage(item.FilePath, profile), 
			    item => profile.Copy);
	    }

	    private async Task<Dictionary<string, bool>> GenerateMoveSourceFiles(
		    AppSettingsPublishProfiles profile, IReadOnlyCollection<FileIndexItem> fileIndexItemsList,
		    string outputParentFullFilePathFolder, bool moveSourceFiles)
	    {
		    _toCreateSubfolder.Create(profile,outputParentFullFilePathFolder);
            
		    var overlayImage = new OverlayImage(_selectorStorage, _appSettings);

		    foreach (var item in fileIndexItemsList)
		    {
			    // input: item.FilePath
			    var outputPath = overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder,
				    item.FilePath, profile);

			    await _hostFileSystemStorage.WriteStreamAsync(_subPathStorage.ReadStream(item.FilePath),
				    outputPath);
                
			    // only delete when using in cli mode
			    if ( moveSourceFiles )
			    {
				    _subPathStorage.FileDelete(item.FilePath);
			    }
		    }
		    return fileIndexItemsList.ToDictionary(item =>
				    _overlayImage.FilePathOverlayImage(item.FilePath, profile), 
			    item => profile.Copy);
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="fullFileParentFolderPath">One folder deeper than where the folder </param>
	    /// <param name="itemName">blog item name</param>
	    /// <param name="renderCopyResult"></param>
	    /// <param name="deleteFolderAfterwards"></param>
	    public async Task GenerateZip(string fullFileParentFolderPath, string itemName,
		    Dictionary<string, bool> renderCopyResult, bool deleteFolderAfterwards = false)
	    {
		    var fileNames = renderCopyResult.Where(p => p.Value).Select(p => p.Key).ToList();

		    var slugItemName = _appSettings.GenerateSlug(itemName, true);
		    var filePaths = fileNames.Select(p => Path.Combine(fullFileParentFolderPath, slugItemName, p)).ToList();

		    new Zipper().CreateZip(fullFileParentFolderPath, filePaths, fileNames,
			    _appSettings.GenerateSlug(itemName));
		    
		    // Write a single file to be sure that writing is ready
		    var doneFileFullPath = Path.Combine(_appSettings.TempFolder, slugItemName) + ".done";
		    await _hostFileSystemStorage.
			    WriteStreamAsync(new PlainTextFileHelper().StringToStream("OK"), doneFileFullPath);

		    if ( deleteFolderAfterwards )
		    {
			    _hostFileSystemStorage.FolderDelete(Path.Combine(_appSettings.TempFolder,
				    slugItemName));
		    }
	    }
    }
}
