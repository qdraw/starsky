using Medallion.Shell;
using starsky.foundation.injection;
using starsky.foundation.optimisation.Helpers;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using static Medallion.Shell.Shell;

namespace starsky.foundation.optimisation.Services;

[Service(typeof(IMozJpegService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MozJpegService : IMozJpegService
{
	private readonly ImageOptimisationExePath _exePathHelper;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IWebLogger _logger;
	private readonly IMozJpegDownload _mozJpegDownload;

	public MozJpegService(AppSettings appSettings,
		ISelectorStorage selectorStorage, IWebLogger logger, IMozJpegDownload mozJpegDownload)
	{
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
		_mozJpegDownload = mozJpegDownload;
		_exePathHelper = new ImageOptimisationExePath(appSettings);
	}

	public async Task RunMozJpeg(Optimizer optimizer,
		IEnumerable<ImageOptimisationItem> targets)
	{
		var downloadStatus = await _mozJpegDownload.Download();
		if ( downloadStatus != ImageOptimisationDownloadStatus.Ok &&
		     downloadStatus != ImageOptimisationDownloadStatus.OkAlreadyDownloaded )
		{
			_logger.LogError(
				$"[ImageOptimisationService] MozJPEG download failed: {downloadStatus}");
			return;
		}

		var exePath = _exePathHelper.GetExePath(optimizer.Id,
			CurrentArchitecture.GetCurrentRuntimeIdentifier());
		if ( !_hostFileSystemStorage.ExistFile(exePath) )
		{
			_logger.LogError($"[ImageOptimisationService] MozJPEG not found at {exePath}");
			return;
		}

		foreach ( var outputInputPath in targets.Select(item => item.OutputPath) )
		{
			if ( !IsJpegOutput(outputInputPath) )
			{
				continue;
			}

			var tempFilePath = outputInputPath + ".optimizing";

			var parent = Directory.GetParent(exePath);

			var (command, outputStream) =
				await CommandRetry(exePath, outputInputPath, optimizer, parent);
			if ( command == null || outputStream == null )
			{
				continue;
			}

			await _hostFileSystemStorage.WriteStreamAsync(outputStream, tempFilePath);

			await outputStream.DisposeAsync();

			if ( !command.Result.Success )
			{
				_logger.LogError(
					$"[ImageOptimisationService] MozJPEG failed for {outputInputPath}: " +
					$"{command.Result.StandardError}");
				if ( _hostFileSystemStorage.ExistFile(tempFilePath) )
				{
					_hostFileSystemStorage.FileDelete(tempFilePath);
				}

				continue;
			}

			_hostFileSystemStorage.FileDelete(outputInputPath);
			_hostFileSystemStorage.FileMove(tempFilePath, outputInputPath);

			_logger.LogInformation("[ImageOptimisationService] MozJPEG optimized: " +
			                       outputInputPath);
		}
	}

	private async Task<(Command? command, MemoryStream? outputStream)> CommandRetry(string exePath,
		string outputInputPath, Optimizer optimizer, DirectoryInfo? parent)
	{
		Command command;
		MemoryStream outputStream;
		try
		{
			( command, outputStream ) = await Command(exePath, outputInputPath, optimizer, parent);
		}
		catch ( Exception )
		{
			await _mozJpegDownload.FixPermissions(exePath);
			try
			{
				( command, outputStream ) =
					await Command(exePath, outputInputPath, optimizer, parent);
			}
			catch ( Exception exception )
			{
				_logger.LogError(
					$"[ImageOptimisationService] " +
					$"MozJPEG failed to run for {outputInputPath}: " +
					$"{exception.Message}");
				return ( null, null );
			}
		}

		return ( command, outputStream );
	}

	private static async Task<(Command command, MemoryStream outputStream)> Command(string exePath,
		string outputInputPath, Optimizer optimizer, DirectoryInfo? parent)
	{
		var outputStream = new MemoryStream();

		List<string> arguments =
		[
			"-quality", optimizer.Options.Quality.ToString(),
			"-optimize", outputInputPath
		];

		var command = Default.Run(
			exePath,
			options: opts =>
			{
				opts.StartInfo(i => i.Arguments = string.Join(" ", arguments));
				opts.WorkingDirectory(parent!.FullName);
			}
		) > outputStream;
		await command.Task.TimeoutAfter(TimeSpan.FromMinutes(5));
		return ( command, outputStream );
	}


	private bool IsJpegOutput(string outputPath)
	{
		using var stream = _hostFileSystemStorage.ReadStream(outputPath, 68);
		var format = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		return format == ExtensionRolesHelper.ImageFormat.jpg;
	}
}
