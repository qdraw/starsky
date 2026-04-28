using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Services;

[Service(typeof(IOllamaService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class OllamaService : IOllamaService
{
	private const string TagPrompt =
		"Return a concise comma-separated list of tags for this image. Use lowercase tags only.";

	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IOllamaProcessRunner _processRunner;
	private readonly OllamaExePath _ollamaExePath;

	public OllamaService(AppSettings appSettings, IWebLogger logger,
		IOllamaProcessRunner processRunner)
	{
		_appSettings = appSettings;
		_logger = logger;
		_processRunner = processRunner;
		_ollamaExePath = new OllamaExePath(_appSettings);
	}

	public async Task<bool> EnsureServeIsRunning(CancellationToken cancellationToken = default)
	{
		if ( _processRunner.IsServeRunning )
		{
			return true;
		}

		var executablePath = _ollamaExePath.GetConfiguredOrDefaultPath();
		if ( !_ollamaExePath.IsValidExecutablePath(executablePath) )
		{
			_logger.LogError(
				$"[OllamaService] Ollama executable not found: {executablePath}");
			return false;
		}

		var environmentVariables = GetOllamaEnvironmentVariables();
		var started = await _processRunner.StartServeAsync(executablePath,
			environmentVariables, cancellationToken);
		if ( !started )
		{
			return false;
		}

		for ( var attempt = 0; attempt < 10; attempt++ )
		{
			var probe = await _processRunner.RunProcessWithOutputAsync(executablePath,
				"list", environmentVariables, cancellationToken: cancellationToken);
			if ( probe.Success )
			{
				return true;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
		}

		_logger.LogError("[OllamaService] Ollama serve did not become ready in time");
		return false;
	}

	public Task<bool> StopServeAsync(CancellationToken cancellationToken = default)
	{
		return _processRunner.StopServeAsync(cancellationToken);
	}

	public async Task<OllamaCommandResult> GenerateAsync(string prompt,
		CancellationToken cancellationToken = default)
	{
		if ( string.IsNullOrWhiteSpace(prompt) )
		{
			return OllamaCommandResult.Failed("Prompt should not be empty");
		}

		if ( !await EnsureServeIsRunning(cancellationToken) )
		{
			return OllamaCommandResult.Failed("Ollama serve process is not running");
		}

		var executablePath = _ollamaExePath.GetConfiguredOrDefaultPath();
		var arguments = $"run {EscapeArgument(_appSettings.OllamaModel)} {EscapeArgument(prompt)}";
		return await _processRunner.RunProcessWithOutputAsync(executablePath,
			arguments, GetOllamaEnvironmentVariables(), cancellationToken: cancellationToken);
	}

	public async Task<OllamaCommandResult> InferTagsAsync(string imageFilePath,
		CancellationToken cancellationToken = default)
	{
		if ( string.IsNullOrWhiteSpace(imageFilePath) || !File.Exists(imageFilePath) )
		{
			return OllamaCommandResult.Failed(
				$"Image file not found: {imageFilePath}");
		}

		if ( !await EnsureServeIsRunning(cancellationToken) )
		{
			return OllamaCommandResult.Failed("Ollama serve process is not running");
		}

		var executablePath = _ollamaExePath.GetConfiguredOrDefaultPath();
		var arguments =
			$"run {EscapeArgument(_appSettings.OllamaModel)} {EscapeArgument(TagPrompt)} {EscapeArgument(imageFilePath)}";
		return await _processRunner.RunProcessWithOutputAsync(executablePath,
			arguments, GetOllamaEnvironmentVariables(), cancellationToken: cancellationToken);
	}

	private Dictionary<string, string> GetOllamaEnvironmentVariables()
	{
		var modelsDirectory = Path.Combine(_appSettings.DependenciesFolder, "ollama-models");
		Directory.CreateDirectory(modelsDirectory);
		return new Dictionary<string, string>
		{
			{ "OLLAMA_MODELS", modelsDirectory }
		};
	}

	private static string EscapeArgument(string value)
	{
		return $"\"{value.Replace("\"", "\\\"")}\"";
	}
}

