using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Services;

[TestClass]
public sealed class OllamaServiceTests
{
	private string _testDependenciesFolder = string.Empty;
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public TestContext TestContext { get; set; }

	[TestInitialize]
	public void Initialize()
	{
		_testDependenciesFolder = Path.Combine(Path.GetTempPath(), "starsky_ollama_service_tests",
			Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_testDependenciesFolder);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_testDependenciesFolder) )
		{
			Directory.Delete(_testDependenciesFolder, true);
		}
	}

	[TestMethod]
	public async Task EnsureServeIsRunning_MissingExecutable_ShouldFail()
	{
		var appSettings = new AppSettings
		{
			DependenciesFolder = _testDependenciesFolder,
			OllamaExecutablePath = Path.Combine(_testDependenciesFolder, "missing", "ollama")
		};
		var runner = new FakeOllamaProcessRunner();
		var sut = new OllamaService(appSettings, new FakeIWebLogger(), runner);

		var result = await sut.EnsureServeIsRunning(TestContext.CancellationToken);

		Assert.IsFalse(result);
		Assert.AreEqual(0, runner.StartServeCallCount);
	}

	[TestMethod]
	public async Task GenerateAsync_ShouldRunModelPrompt_WhenServeStarts()
	{
		var executable = CreateExecutablePath("ollama");
		var appSettings = new AppSettings
		{
			DependenciesFolder = _testDependenciesFolder,
			OllamaExecutablePath = executable,
			OllamaModel = "gemma3:4b"
		};

		var runner = new FakeOllamaProcessRunner
		{
			StartServeResult = true,
			RunProcessResult = new OllamaCommandResult
			{
				Success = true,
				Output = "tree,sky"
			}
		};
		var sut = new OllamaService(appSettings, new FakeIWebLogger(), runner);

		var result = await sut.GenerateAsync("tags please", TestContext.CancellationToken);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(2, runner.RunCallCount);
		Assert.Contains("run", runner.ArgumentsLog[1]);
		Assert.Contains("gemma3:4b", runner.ArgumentsLog[1]);
		Assert.Contains("tags please", runner.ArgumentsLog[1]);
	}

	[TestMethod]
	public async Task InferTagsAsync_ImageMissing_ShouldFail()
	{
		var executable = CreateExecutablePath("ollama");
		var appSettings = new AppSettings
		{
			DependenciesFolder = _testDependenciesFolder,
			OllamaExecutablePath = executable
		};
		var runner = new FakeOllamaProcessRunner();
		var sut = new OllamaService(appSettings, new FakeIWebLogger(), runner);

		var result = await sut.InferTagsAsync(Path.Combine(_testDependenciesFolder, "missing.jpg"),
			TestContext.CancellationToken);

		Assert.IsFalse(result.Success);
		Assert.AreEqual(0, runner.StartServeCallCount);
	}

	[TestMethod]
	public async Task InferTagsAsync_ShouldIncludeImagePathInArguments()
	{
		var executable = CreateExecutablePath("ollama");
		var imagePath = Path.Combine(_testDependenciesFolder, "image.jpg");
		await File.WriteAllTextAsync(imagePath, "fake image", TestContext.CancellationToken);
		var appSettings = new AppSettings
		{
			DependenciesFolder = _testDependenciesFolder,
			OllamaExecutablePath = executable,
			OllamaModel = "gemma3:4b"
		};

		var runner = new FakeOllamaProcessRunner
		{
			StartServeResult = true,
			RunProcessResult = new OllamaCommandResult
			{
				Success = true,
				Output = "water,night"
			}
		};
		var sut = new OllamaService(appSettings, new FakeIWebLogger(), runner);

		var result = await sut.InferTagsAsync(imagePath, TestContext.CancellationToken);

		Assert.IsTrue(result.Success);
		Assert.AreEqual(2, runner.RunCallCount);
		Assert.Contains(imagePath, runner.ArgumentsLog[1]);
	}

	private string CreateExecutablePath(string executableName)
	{
		var executablePath = Path.Combine(_testDependenciesFolder, executableName);
		File.WriteAllText(executablePath, "test");
		return executablePath;
	}

	private sealed class FakeOllamaProcessRunner : IOllamaProcessRunner
	{
		public bool IsServeRunning { get; private set; }
		public int StartServeCallCount { get; private set; }
		public int RunCallCount { get; private set; }
		public bool StartServeResult { get; set; }
		public OllamaCommandResult RunProcessResult { get; set; } =
			new OllamaCommandResult { Success = true, Output = string.Empty, ExitCode = 0 };
		public List<string> ArgumentsLog { get; } = [];

		public Task<bool> StartServeAsync(string fileName,
			IDictionary<string, string>? environmentVariables = null,
			CancellationToken cancellationToken = default)
		{
			StartServeCallCount++;
			IsServeRunning = StartServeResult;
			return Task.FromResult(StartServeResult);
		}

		public Task<bool> StopServeAsync(CancellationToken cancellationToken = default)
		{
			IsServeRunning = false;
			return Task.FromResult(true);
		}

		public Task<OllamaCommandResult> RunProcessWithOutputAsync(string fileName,
			string arguments,
			IDictionary<string, string>? environmentVariables = null,
			int[]? allowedExitCodes = null,
			CancellationToken cancellationToken = default)
		{
			RunCallCount++;
			ArgumentsLog.Add(arguments);
			if ( arguments == "list" )
			{
				return Task.FromResult(new OllamaCommandResult
				{
					Success = true,
					Output = "NAME ID SIZE MODIFIED",
					ExitCode = 0
				});
			}

			return Task.FromResult(RunProcessResult);
		}
	}
}



