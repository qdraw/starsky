using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Exceptions;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.feature.webhtmlpublish.Services;

[Service(typeof(IWebHtmlPublishService), InjectionLifetime = InjectionLifetime.Scoped)]
public class WebHtmlPublishService : IWebHtmlPublishService
{
	private readonly AppSettings _appSettings;
	private readonly IConsole _console;
	private readonly CopyPublishedContent _copyPublishedContent;
	private readonly IExifTool _exifTool;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IOverlayImage _overlayImage;
	private readonly PublishManifest _publishManifest;
	private readonly IPublishPreflight _publishPreflight;
	private readonly IStorage _subPathStorage;
	private readonly IThumbnailService _thumbnailService;
	private readonly IStorage _thumbnailStorage;
	private readonly ToCreateSubfolder _toCreateSubfolder;

	[SuppressMessage("Usage",
		"S107: Constructor has 8 parameters, which is greater than the 7 authorized")]
	public WebHtmlPublishService(IPublishPreflight publishPreflight, ISelectorStorage
			selectorStorage, AppSettings appSettings, IExifToolHostStorage exifTool,
		IOverlayImage overlayImage, IConsole console, IWebLogger logger,
		IThumbnailService thumbnailService)
	{
		_publishPreflight = publishPreflight;
		_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_appSettings = appSettings;
		_exifTool = exifTool;
		_console = console;
		_overlayImage = overlayImage;
		_publishManifest = new PublishManifest(_hostFileSystemStorage);
		_toCreateSubfolder = new ToCreateSubfolder(_hostFileSystemStorage);
		_copyPublishedContent = new CopyPublishedContent(_toCreateSubfolder,
			selectorStorage);
		_logger = logger;
		_thumbnailService = thumbnailService;
	}

	public async Task<Dictionary<string, bool>?> RenderCopy(
		List<FileIndexItem> fileIndexItemsList,
		string publishProfileName, string itemName, string outputParentFullFilePathFolder,
		bool moveSourceFiles = false)
	{
		fileIndexItemsList = AddFileHashIfNotExist(fileIndexItemsList);

		await PreGenerateThumbnail(fileIndexItemsList, publishProfileName);
		var base64ImageArray = await Base64DataUriList(fileIndexItemsList);

		var copyContent = await Render(fileIndexItemsList, base64ImageArray,
			publishProfileName, itemName, outputParentFullFilePathFolder, moveSourceFiles);

		_publishManifest.ExportManifest(outputParentFullFilePathFolder, itemName, copyContent);

		return copyContent;
	}

	/// <summary>
	///     Generate Zip on Host
	/// </summary>
	/// <param name="fullFileParentFolderPath">One folder deeper than where the folder </param>
	/// <param name="itemName">blog item name</param>
	/// <param name="renderCopyResult">[[string,bool],[]]</param>
	public async Task GenerateZip(string fullFileParentFolderPath, string itemName,
		Dictionary<string, bool>? renderCopyResult)
	{
		ArgumentNullException.ThrowIfNull(renderCopyResult);

		// to keep non publish files out use Where(p => p.Value)
		var fileNames = renderCopyResult.Select(p => p.Key).ToList();

		var slugItemName = GenerateSlugHelper.GenerateSlug(itemName, true);
		var filePaths = fileNames
			.Select(p => Path.Combine(fullFileParentFolderPath, slugItemName, p)).ToList();

		new Zipper(_logger).CreateZip(fullFileParentFolderPath, filePaths, fileNames,
			slugItemName);

		// Write a single file to be sure that writing is ready
		var doneFileFullPath = Path.Combine(_appSettings.TempFolder, slugItemName) + ".done";
		await _hostFileSystemStorage.WriteStreamAsync(StringToStreamHelper.StringToStream("OK"),
			doneFileFullPath);

		_hostFileSystemStorage.FolderDelete(Path.Combine(_appSettings.TempFolder,
			slugItemName));
	}

	internal List<FileIndexItem> AddFileHashIfNotExist(List<FileIndexItem> fileIndexItemsList)
	{
		foreach ( var item in fileIndexItemsList.Where(item =>
			         string.IsNullOrEmpty(item.FileHash)) )
		{
			item.FileHash = new FileHash(_subPathStorage, _logger).GetHashCode(item.FilePath!).Key;
		}

		return fileIndexItemsList;
	}

	internal bool ShouldSkipExtraLarge(string publishProfileName)
	{
		var skipExtraLarge = _publishPreflight.GetPublishProfileName(publishProfileName)
			.TrueForAll(p => p.SourceMaxWidth <= 1999);
		return skipExtraLarge;
	}

	internal async Task PreGenerateThumbnail(IEnumerable<FileIndexItem> fileIndexItemsList,
		string publishProfileName)
	{
		var skipExtraLarge = ShouldSkipExtraLarge(publishProfileName);
		foreach ( var item in fileIndexItemsList )
		{
			await _thumbnailService.GenerateThumbnail(item.FilePath!, item.FileHash!,
				skipExtraLarge);
		}
	}

	/// <summary>
	///     Get base64 uri lists
	/// </summary>
	/// <returns></returns>
	private Task<string[]> Base64DataUriList(IEnumerable<FileIndexItem> fileIndexItemsList)
	{
		var service = new ToBase64DataUriList(_thumbnailService);
		return service.Create(fileIndexItemsList.ToList());
	}

	/// <summary>
	///     Render output of Publish action
	/// </summary>
	/// <param name="fileIndexItemsList">which items need to be published</param>
	/// <param name="base64ImageArray">list of base64 hashes for html pages</param>
	/// <param name="publishProfileName">name of profile</param>
	/// <param name="itemName">output publish item name</param>
	/// <param name="outputParentFullFilePathFolder">path on host disk where to publish to</param>
	/// <param name="moveSourceFiles">include source files (false by default)</param>
	/// <returns></returns>
	private async Task<Dictionary<string, bool>?> Render(List<FileIndexItem> fileIndexItemsList,
		string[]? base64ImageArray, string publishProfileName, string itemName,
		string outputParentFullFilePathFolder, bool moveSourceFiles = false)
	{
		if ( _appSettings.PublishProfiles?.Count == 0 )
		{
			_console.WriteLine("There are no config items");
			return null;
		}

		if ( _appSettings.PublishProfiles?.ContainsKey(publishProfileName) == false )
		{
			_console.WriteLine("Key not found");
			return null;
		}

		if ( !_hostFileSystemStorage.ExistFolder(outputParentFullFilePathFolder) )
		{
			_hostFileSystemStorage.CreateDirectory(outputParentFullFilePathFolder);
		}

		base64ImageArray ??= new string[fileIndexItemsList.Count];

		// Order alphabetically
		// Ignore Items with Errors
		fileIndexItemsList = fileIndexItemsList
			.Where(p => p.Status is FileIndexItem.ExifStatus.Ok
				or FileIndexItem.ExifStatus.ReadOnly)
			.OrderBy(p => p.FileName).ToList();

		var copyResult = new Dictionary<string, bool>();

		var profiles = _publishPreflight.GetPublishProfileName(publishProfileName);
		foreach ( var currentProfile in profiles )
		{
			switch ( currentProfile.ContentType )
			{
				case TemplateContentType.Html:
					copyResult.AddRangeOverride(await GenerateWebHtml(profiles, currentProfile,
						itemName,
						base64ImageArray, fileIndexItemsList, outputParentFullFilePathFolder));
					break;
				case TemplateContentType.Jpeg:
					copyResult.AddRangeOverride(await GenerateJpeg(currentProfile,
						fileIndexItemsList,
						outputParentFullFilePathFolder));
					break;
				case TemplateContentType.MoveSourceFiles:
					copyResult.AddRangeOverride(await GenerateMoveSourceFiles(currentProfile,
						fileIndexItemsList,
						outputParentFullFilePathFolder, moveSourceFiles));
					break;
				case TemplateContentType.PublishContent:
					// Copy all items in the subFolder content for example JavaScripts
					copyResult.AddRangeOverride(
						_copyPublishedContent.CopyContent(currentProfile,
							outputParentFullFilePathFolder));
					break;
				case TemplateContentType.PublishManifest:
					copyResult.Add(
						_overlayImage.FilePathOverlayImage("_settings.json", currentProfile)
						, true);
					break;
				case TemplateContentType.OnlyFirstJpeg:
					var item = fileIndexItemsList.FirstOrDefault();
					if ( item == null )
					{
						break;
					}

					var firstInList = new List<FileIndexItem> { item };
					copyResult.AddRangeOverride(await GenerateJpeg(currentProfile, firstInList,
						outputParentFullFilePathFolder));
					break;
			}
		}

		return copyResult;
	}

	internal async Task<Dictionary<string, bool>> GenerateWebHtml(
		List<AppSettingsPublishProfiles> profiles,
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
			FileIndexItems = fileIndexItemsList.Select(c => c.Clone()).ToList()
		};

		// add to IClonable
		foreach ( var item in viewModel.FileIndexItems )
		{
			item.FileName = GenerateSlugHelper.GenerateSlug(item.FileCollectionName!, true) +
			                Path.GetExtension(item.FileName);
		}

		// has a direct dependency on the filesystem
		var embeddedResult = await new ParseRazor(_hostFileSystemStorage, _logger)
			.EmbeddedViews(currentProfile.Template, viewModel);

		var stream = StringToStreamHelper.StringToStream(embeddedResult);
		await _hostFileSystemStorage.WriteStreamAsync(stream,
			Path.Combine(outputParentFullFilePathFolder, currentProfile.Path));

		_console.Write(_appSettings.IsVerbose() ? embeddedResult + "\n" : "•");

		return new Dictionary<string, bool>
		{
			{
				currentProfile.Path.Replace(outputParentFullFilePathFolder, string.Empty),
				currentProfile.Copy
			}
		};
	}

	/// <summary>
	///     Generate loop of Jpeg images with overlay image
	///     With Retry included
	/// </summary>
	/// <param name="profile">contains sizes</param>
	/// <param name="fileIndexItemsList">list of items to generate jpeg for</param>
	/// <param name="outputParentFullFilePathFolder">outputParentFullFilePathFolder</param>
	/// <param name="delay">when failed output, has default value</param>
	/// <returns></returns>
	internal async Task<Dictionary<string, bool>> GenerateJpeg(
		AppSettingsPublishProfiles profile,
		IReadOnlyCollection<FileIndexItem> fileIndexItemsList,
		string outputParentFullFilePathFolder, int delay = 6)
	{
		_toCreateSubfolder.Create(profile, outputParentFullFilePathFolder);

		foreach ( var item in fileIndexItemsList )
		{
			var outputPath = _overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder,
				item.FilePath!, profile);

			async Task<bool> ResizerLocal()
			{
				return await Resizer(outputPath, profile, item);
			}

			try
			{
				await RetryHelper.DoAsync(ResizerLocal, TimeSpan.FromSeconds(delay));
			}
			catch ( AggregateException e )
			{
				_logger.LogError(
					$"[ResizerLocal] Skip due errors: (catch-ed exception) {item.FilePath} {item.FileHash}");
				foreach ( var exception in e.InnerExceptions )
				{
					_logger.LogError("[ResizerLocal] " + exception.Message, exception);
				}
			}
		}

		return fileIndexItemsList.ToDictionary(item =>
				_overlayImage.FilePathOverlayImage(item.FilePath!, profile),
			_ => profile.Copy);
	}

	/// <summary>
	///     Resize image with overlay
	/// </summary>
	/// <param name="outputPath">absolute path of output on host disk</param>
	/// <param name="profile">size of output, overlay size, must contain metaData </param>
	/// <param name="item">database item with filePath</param>
	/// <returns>true when success</returns>
	/// <exception cref="DecodingException">when output is not valid</exception>
	private async Task<bool> Resizer(string outputPath, AppSettingsPublishProfiles profile,
		FileIndexItem item)
	{
		// for less than 1000px
		if ( profile.SourceMaxWidth <= 1000 &&
		     _thumbnailStorage.ExistFile(
			     ThumbnailNameHelper.Combine(item.FileHash!, ThumbnailSize.Large,
				     _appSettings.ThumbnailImageFormat)) )
		{
			await _overlayImage.ResizeOverlayImageThumbnails(item.FileHash!, outputPath,
				profile);
		}
		else if ( profile.SourceMaxWidth <= 2000 &&
		          _thumbnailStorage.ExistFile(
			          ThumbnailNameHelper.Combine(item.FileHash!, ThumbnailSize.ExtraLarge,
				          _appSettings.ThumbnailImageFormat)) )
		{
			await _overlayImage.ResizeOverlayImageThumbnails(
				ThumbnailNameHelper.Combine(item.FileHash!, ThumbnailSize.ExtraLarge,
					_appSettings.ThumbnailImageFormat),
				outputPath, profile);
		}
		else if ( _subPathStorage.ExistFile(item.FilePath!) )
		{
			// Thumbs are 2000 px (and larger)
			await _overlayImage.ResizeOverlayImageLarge(item.FilePath!, outputPath, profile);
		}

		if ( profile.MetaData )
		{
			await MetaData(item, outputPath);
		}

		var imageFormat =
			new ExtensionRolesHelper(_logger).GetImageFormat(
				_hostFileSystemStorage.ReadStream(outputPath, 160));
		if ( imageFormat == ExtensionRolesHelper.ImageFormat.jpg )
		{
			return true;
		}

		_hostFileSystemStorage.FileDelete(outputPath);

		throw new DecodingException("[WebHtmlPublishService] image output is not valid");
	}

	/// <summary>
	///     Copy the metaData over the output path
	/// </summary>
	/// <param name="item">all the metadata</param>
	/// <param name="outputPath">absolute path on host disk</param>
	private async Task MetaData(FileIndexItem item, string outputPath)
	{
		if ( !_subPathStorage.ExistFile(item.FilePath!) )
		{
			return;
		}

		// Write the metadata to the new created file
		var comparedNames = FileIndexCompareHelper.Compare(
			new FileIndexItem(), item);

		// Output has already rotated the image
		var rotation = nameof(FileIndexItem.Orientation).ToLowerInvariant();

		// should already check if it exists
		comparedNames.Remove(rotation);

		// Write it back
		await new ExifToolCmdHelper(_exifTool, _hostFileSystemStorage,
			_thumbnailStorage, null!, null!, _logger).UpdateAsync(item,
			new List<string> { outputPath }, comparedNames,
			false, false);
	}

	internal async Task<Dictionary<string, bool>> GenerateMoveSourceFiles(
		AppSettingsPublishProfiles profile,
		IReadOnlyCollection<FileIndexItem> fileIndexItemsList,
		string outputParentFullFilePathFolder, bool moveSourceFiles)
	{
		_toCreateSubfolder.Create(profile, outputParentFullFilePathFolder);

		foreach ( var subPath in fileIndexItemsList.Select(p => p.FilePath!) )
		{
			// input: item.FilePath
			var outputPath = _overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder,
				subPath, profile);

			await _hostFileSystemStorage.WriteStreamAsync(_subPathStorage.ReadStream(subPath),
				outputPath);

			// only delete when using in cli mode
			if ( moveSourceFiles )
			{
				_subPathStorage.FileDelete(subPath);
			}
		}

		return fileIndexItemsList.ToDictionary(item =>
				_overlayImage.FilePathOverlayImage(item.FilePath!, profile),
			_ => profile.Copy);
	}
}
