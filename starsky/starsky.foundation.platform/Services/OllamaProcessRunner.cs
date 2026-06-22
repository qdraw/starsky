using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services;

[Service(typeof(IOllamaProcessRunner), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class OllamaProcessRunner(IWebLogger logger) : IOllamaProcessRunner
{
	private readonly object _lock = new();
	private Process? _serveProcess;

	public bool IsServeRunning
	{
		get
		{
			lock ( _lock )
			{
				return _serveProcess is { HasExited: false };
			}
		}
	}

	public async Task<bool> StartServeAsync(string fileName,
		IDictionary<string, string>? environmentVariables = null,
		CancellationToken cancellationToken = default)
	{
		if ( IsServeRunning )
		{
			return true;
		}

		try
		{
			var processInfo = CreateProcessStartInfo(fileName, "serve", environmentVariables,
				false);
			var process = Process.Start(processInfo);
			if ( process == null )
			{
				logger.LogError($"[OllamaProcessRunner] Failed to start: {fileName} serve");
				return false;
			}

			lock ( _lock )
			{
				_serveProcess = process;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
			if ( process.HasExited )
			{
				logger.LogError(
					$"[OllamaProcessRunner] Serve process exited early with code {process.ExitCode}");
				return false;
			}

			return true;
		}
		catch ( Win32Exception exception )
		{
			logger.LogError(exception,
				$"[OllamaProcessRunner] Unable to start Ollama process: {fileName}");
			return false;
		}
		catch ( Exception exception )
		{
			logger.LogError(exception,
				$"[OllamaProcessRunner] Unexpected error while starting Ollama process: {fileName}");
			return false;
		}
	}

	public async Task<bool> StopServeAsync(CancellationToken cancellationToken = default)
	{
		Process? process;
		lock ( _lock )
		{
			process = _serveProcess;
			_serveProcess = null;
		}

		if ( process == null )
		{
			return true;
		}

		try
		{
			if ( !process.HasExited )
			{
				process.Kill(true);
				await process.WaitForExitAsync(cancellationToken);
			}

			process.Dispose();
			return true;
		}
		catch ( Exception exception )
		{
			logger.LogError(exception,
				"[OllamaProcessRunner] Failed to stop Ollama serve process");
			return false;
		}
	}

	public async Task<OllamaCommandResult> RunProcessWithOutputAsync(string fileName,
		string arguments,
		IDictionary<string, string>? environmentVariables = null,
		int[]? allowedExitCodes = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var processInfo = CreateProcessStartInfo(fileName, arguments, environmentVariables, true);
			using var process = Process.Start(processInfo);
			if ( process == null )
			{
				return OllamaCommandResult.Failed(
					$"Unable to start process: {fileName} {arguments}");
			}

			var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
			var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

			await process.WaitForExitAsync(cancellationToken);
			var output = await outputTask;
			var error = await errorTask;

			var succeeded = process.ExitCode == 0 ||
			                ( allowedExitCodes != null &&
			                  Array.IndexOf(allowedExitCodes, process.ExitCode) >= 0 );
			if ( !succeeded )
			{
				logger.LogError(
					$"[OllamaProcessRunner] {fileName} {arguments} failed with exit code {process.ExitCode}\nOutput: {output}\nError: {error}");
			}

			return new OllamaCommandResult
			{
				Success = succeeded,
				Output = output,
				Error = error,
				ExitCode = process.ExitCode
			};
		}
		catch ( Win32Exception exception )
		{
			logger.LogError(exception,
				$"[OllamaProcessRunner] Unable to start process: {fileName} {arguments}");
			return OllamaCommandResult.Failed(exception.Message);
		}
		catch ( Exception exception )
		{
			logger.LogError(exception,
				$"[OllamaProcessRunner] Unexpected error while running process: {fileName} {arguments}");
			return OllamaCommandResult.Failed(exception.Message);
		}
	}

	private static ProcessStartInfo CreateProcessStartInfo(string fileName,
		string arguments,
		IDictionary<string, string>? environmentVariables,
		bool redirectOutput)
	{
		var processInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = redirectOutput,
			RedirectStandardError = redirectOutput,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		if ( environmentVariables == null )
		{
			return processInfo;
		}

		foreach ( var variable in environmentVariables )
		{
			processInfo.Environment[variable.Key] = variable.Value;
		}

		return processInfo;
	}
}

