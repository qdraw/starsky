using System.Runtime.InteropServices;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Helpers;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IImageOptimisationToolDownload), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImageOptimisationToolDownload(
	ISelectorStorage selectorStorage,
	IHttpClientHelper httpClientHelper,
	AppSettings appSettings,
	IWebLogger logger,
	IImageOptimisationToolDownloadIndex toolDownloadIndex,
	IZipper zipper,
	IImageOptimisationChmod chmod)
	: IImageOptimisationToolDownload
{
	private readonly ImageOptimisationExePath _exePathHelper = new(appSettings);

	private readonly IStorage _hostFileSystemStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	/// <summary>
	///     Downloads the image optimisation tool for a specific architecture. It checks if the tool is
	///     already downloaded and if not, it downloads the zip file, checks the sha256 hash, extracts the
	///     zip
	/// </summary>
	/// <param name="options">which tool to download</param>
	/// <param name="architecture">use .net names for architecture</param>
	/// <param name="retryInSeconds">to retry</param>
	/// <returns>status of download</returns>
	public async Task<ImageOptimisationDownloadStatus> Download(
		ImageOptimisationToolDownloadOptions options, string? architecture = null,
		int retryInSeconds = 15)
	{
		if ( appSettings.ImageOptimisationDownloadOnStartup == true
		     || appSettings is { AddSwaggerExport: true, AddSwaggerExportExitAfter: true } )
		{
			var name = appSettings.ImageOptimisationDownloadOnStartup == true
				? "ImageOptimisationDownloadOnStartup"
				: "AddSwaggerExport and AddSwaggerExportExitAfter";
			logger.LogInformation(
				$"[ImageOptimisationToolDownload] Skipped due true of {name} setting");
			return ImageOptimisationDownloadStatus.SettingsDisabled;
		}

		architecture ??= CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var toolFolder = Path.Combine(appSettings.DependenciesFolder, options.ToolName);
		var architectureFolder = _exePathHelper.GetExeParentFolder(options.ToolName, architecture);

		CreateDirectories([appSettings.DependenciesFolder, architectureFolder]);

		// Check if the binary already exists in the architecture folder
		var exePath = _exePathHelper.GetExePath(options.ToolName, architecture);

		if ( _hostFileSystemStorage.ExistFile(exePath) )
		{
			logger.LogInformation(
				$"[ImageOptimisationToolDownload] Tool already exists: {exePath}");
			return ImageOptimisationDownloadStatus.OkAlreadyDownloaded;
		}


		var container = await toolDownloadIndex.DownloadIndex(options);
		if ( !container.Success )
		{
			logger.LogError(
				$"[ImageOptimisationToolDownload] Index not found for {options.ToolName}");
			return ImageOptimisationDownloadStatus.DownloadIndexFailed;
		}

		var binaryIndex = container.Data?.Binaries.Find(p => p.Architecture == architecture);
		if ( binaryIndex?.FileName == null )
		{
			logger.LogError(
				$"[ImageOptimisationToolDownload] No binary for {options.ToolName} on {architecture}");
			return ImageOptimisationDownloadStatus.DownloadBinariesFailedMissingFileName;
		}

		var zipFullFilePath = Path.Combine(toolFolder, binaryIndex.FileName);
		if ( !await DownloadMirror(container.BaseUrls, zipFullFilePath, binaryIndex,
			    retryInSeconds) )
		{
			logger.LogError(
				$"[ImageOptimisationToolDownload] Download failed for {options.ToolName}");
			return ImageOptimisationDownloadStatus.DownloadBinariesFailed;
		}

		if ( !new CheckSha256Helper(_hostFileSystemStorage).CheckSha256(zipFullFilePath,
			    [binaryIndex.Sha256]) )
		{
			logger.LogError(
				$"[ImageOptimisationToolDownload] Sha256 check failed for {options.ToolName}");
			return ImageOptimisationDownloadStatus.DownloadBinariesFailedSha256Check;
		}

		if ( !zipper.ExtractZip(zipFullFilePath, architectureFolder) )
		{
			logger.LogError(
				$"[ImageOptimisationToolDownload] Extract zip failed for {options.ToolName}");
			return ImageOptimisationDownloadStatus.DownloadBinariesFailedZipperNotExtracted;
		}

		_hostFileSystemStorage.FileDelete(zipFullFilePath);

		if ( !_hostFileSystemStorage.ExistFile(exePath) )
		{
			logger.LogError($"Zipper failed {exePath}");
			return ImageOptimisationDownloadStatus.DownloadBinariesFailedZipperNotExtracted;
		}

		if ( !options.RunChmodOnUnix || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			return ImageOptimisationDownloadStatus.Ok;
		}

		var chmodResult = await chmod.Chmod(exePath);
		return !chmodResult
			? ImageOptimisationDownloadStatus.RunChmodFailed
			: ImageOptimisationDownloadStatus.Ok;
	}

	/// <summary>
	///     Downloads the image optimisation tool for multiple architectures. It iterates through the
	///     provided architectures and calls the Download method for each architecture. The results are
	///     collected in a list of ImageOptimisationDownloadStatus and returned at the end.
	/// </summary>
	/// <param name="options">which tool to download</param>
	/// <param name="architectures">use the .net names for architectures</param>
	/// <returns>status of download</returns>
	public async Task<List<ImageOptimisationDownloadStatus>> Download(
		ImageOptimisationToolDownloadOptions options,
		List<string> architectures)
	{
		var result = new List<ImageOptimisationDownloadStatus>();
		foreach ( var architecture in
		         DotnetRuntimeNames.GetArchitecturesNoGenericAndFallback(architectures) )
		{
			result.Add(await Download(options, architecture));
		}

		return result;
	}

	private void CreateDirectories(List<string> paths)
	{
		foreach ( var path in paths.Where(path => !_hostFileSystemStorage.ExistFolder(path)) )
		{
			_hostFileSystemStorage.CreateDirectory(path);
		}
	}

	private async Task<bool> DownloadMirror(List<Uri> baseUrls, string zipFullFilePath,
		ImageOptimisationBinaryIndex binaryIndex, int retryInSeconds = 15)
	{
		foreach ( var uri in baseUrls.Select(baseUrl => new Uri(baseUrl, binaryIndex.FileName)) )
		{
			if ( await httpClientHelper.Download(uri, zipFullFilePath, retryInSeconds) )
			{
				return true;
			}
		}

		return false;
	}
}
