using System.Diagnostics;
using System.Runtime.InteropServices;
using starsky.foundation.http.Interfaces;
using starsky.foundation.injection;
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
	IZipper zipper)
	: IImageOptimisationToolDownload
{
	private readonly IStorage _hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public async Task<ImageOptimisationDownloadStatus> Download(
		ImageOptimisationToolDownloadOptions options, string? architecture = null,
		int retryInSeconds = 15)
	{
		architecture ??= CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var toolFolder = Path.Combine(appSettings.DependenciesFolder, options.ToolName);
		var architectureFolder = Path.Combine(toolFolder, architecture);

		CreateDirectories([appSettings.DependenciesFolder, toolFolder, architectureFolder]);

		var container = await toolDownloadIndex.DownloadIndex(options);
		if ( !container.Success )
		{
			logger.LogError($"[ImageOptimisationToolDownload] Index not found for {options.ToolName}");
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
			logger.LogError($"[ImageOptimisationToolDownload] Download failed for {options.ToolName}");
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

		if ( !options.RunChmodOnUnix || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) )
		{
			return ImageOptimisationDownloadStatus.Ok;
		}

		var chmodResult = await RunChmod(architectureFolder);
		if ( !chmodResult )
		{
			return ImageOptimisationDownloadStatus.RunChmodFailed;
		}

		return ImageOptimisationDownloadStatus.Ok;
	}

	private void CreateDirectories(List<string> paths)
	{
		foreach (var path in paths.Where(path => !_hostFileSystemStorage.ExistFolder(path)))
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

	private async Task<bool> RunChmod(string path)
	{
		const string cmdPath = "/bin/chmod";
		if ( !_hostFileSystemStorage.ExistFile(cmdPath) )
		{
			logger.LogError("[ImageOptimisationToolDownload] /bin/chmod does not exist");
			return false;
		}

		var processStartInfo = new ProcessStartInfo(cmdPath)
		{
			RedirectStandardError = true
		};
		processStartInfo.ArgumentList.Add("-R");
		processStartInfo.ArgumentList.Add("0755");
		processStartInfo.ArgumentList.Add(path);

		var process = Process.Start(processStartInfo);
		if ( process == null )
		{
			logger.LogError("[ImageOptimisationToolDownload] Could not start chmod process");
			return false;
		}

		await process.WaitForExitAsync();
		if ( process.ExitCode == 0 )
		{
			return true;
		}

		var standardError = await process.StandardError.ReadToEndAsync();
		logger.LogError(
			$"[ImageOptimisationToolDownload] chmod failed with {process.ExitCode}: {standardError}");
		return false;
	}
}
