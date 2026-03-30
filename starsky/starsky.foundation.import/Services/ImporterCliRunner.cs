using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.import.Services;

[Service(typeof(IImporterCliRunner), InjectionLifetime = InjectionLifetime.Scoped)]
public class ImporterCliRunner(AppSettings appSettings, ISelectorStorage selectorStorage,
	IWebLogger logger) : IImporterCliRunner
{
	internal const string DefaultCameraArguments = "--camera -r --move";

	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public async Task<ImporterCliRunResult> RunCameraImportAsync(
		CancellationToken cancellationToken = default)
	{
		var exePath = ResolveImporterPath();
		if ( string.IsNullOrEmpty(exePath) )
		{
			return new ImporterCliRunResult
			{
				Success = false,
				Message = "starskyimportercli not found"
			};
		}

		var psi = new ProcessStartInfo
		{
			FileName = exePath,
			Arguments = BuildCameraImportArguments(appSettings),
			WorkingDirectory = Path.GetDirectoryName(exePath) ?? appSettings.BaseDirectoryProject,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = new Process();
		process.StartInfo = psi;
		if ( !process.Start() )
		{
			return new ImporterCliRunResult
			{
				Success = false,
				ExecutablePath = exePath,
				Message = "Unable to start process"
			};
		}

		var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
		var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
		await process.WaitForExitAsync(cancellationToken);

		var output = await outputTask;
		var error = await errorTask;

		if ( process.ExitCode != 0 )
		{
			logger.LogError("[ImporterCliRunner] exit code {0}, stderr: {1}",
				process.ExitCode, error);
		}

		return new ImporterCliRunResult
		{
			Success = process.ExitCode == 0,
			ExitCode = process.ExitCode,
			ExecutablePath = exePath,
			StandardOutput = output,
			StandardError = error,
			Message = process.ExitCode == 0 ? "ok" : "importer failed"
		};
	}

	internal string? ResolveImporterPath()
	{
		if ( !string.IsNullOrWhiteSpace(appSettings.MountWatcherImporterPath) &&
		     _hostStorage.ExistFile(appSettings.MountWatcherImporterPath) )
		{
			return appSettings.MountWatcherImporterPath;
		}

		var baseDir = appSettings.BaseDirectoryProject;
		var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? "starskyimportercli.exe"
			: "starskyimportercli";

		var candidates = new List<string>
		{
			Path.Combine(baseDir, "bin", exeName),
			Path.Combine(baseDir, exeName),
			Path.Combine(baseDir, "..", "starskyimportercli", "bin", "Debug", "net8.0", exeName),
			Path.Combine(baseDir, "..", "starskyimportercli", "bin", "Release", "net8.0", exeName)
		};

		return candidates.FirstOrDefault(_hostStorage.ExistFile);
	}

	internal static string BuildCameraImportArguments(AppSettings appSettings)
	{
		return IsAllowedCameraArguments(appSettings.MountWatcherImporterArguments)
			? appSettings.MountWatcherImporterArguments
			: DefaultCameraArguments;
	}

	internal static bool IsAllowedCameraArguments(string? args)
	{
		if ( string.IsNullOrWhiteSpace(args) )
		{
			return false;
		}

		var tokens = args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var allowed = new HashSet<string>
		{
			"--camera", "-r", "--recursive", "--move"
		};

		if ( tokens.Any(token => !allowed.Contains(token)) )
		{
			return false;
		}

		return tokens.Contains("--camera") &&
		       tokens.Contains("--move") &&
		       (tokens.Contains("-r") || tokens.Contains("--recursive"));
	}
}



