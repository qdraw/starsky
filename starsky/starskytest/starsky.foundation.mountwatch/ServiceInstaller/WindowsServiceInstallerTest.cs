using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.ServiceInstaller;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.ServiceInstaller;

[TestClass]
public sealed class WindowsServiceInstallerTest
{
	[TestMethod]
	public async Task InstallAsync_Dll_UsesDotnetHostInCreateCommand()
	{
		var calls = new List<(string fileName, string args)>();
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			(_, _) => Task.FromResult(( true, string.Empty, 0 )),
			_ => Task.CompletedTask);

		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.dll");

		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.AreEqual("sc.exe", calls[0].fileName);
		Assert.Contains("create \"", calls[0].args);
		Assert.Contains("dotnet.exe", calls[0].args);
		Assert.Contains("starskymountwatchercli.dll", calls[0].args);
	}

	[TestMethod]
	public async Task InstallAsync_Exe_UsesExecutablePathInCreateCommand()
	{
		var calls = new List<(string fileName, string args)>();
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			(_, _) => Task.FromResult(( true, string.Empty, 0 )),
			_ => Task.CompletedTask);

		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.exe");

		Assert.IsTrue(result);
		Assert.HasCount(1, calls);
		Assert.Contains("starskymountwatchercli.exe", calls[0].args);
		Assert.IsFalse(calls[0].args.Contains("dotnet.exe", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public async Task StartAsync_RetriesOnce_WhenFirstStartFails()
	{
		var calls = new List<(string fileName, string args)>();
		var delays = new List<int>();
		var invocation = 0;
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				invocation++;
				return Task.FromResult(invocation > 1);
			},
			(_, _) => Task.FromResult(( true, string.Empty, 0 )),
			ms =>
			{
				delays.Add(ms);
				return Task.CompletedTask;
			});

		var result = await sut.StartAsync();

		Assert.IsTrue(result);
		Assert.HasCount(2, calls);
		Assert.HasCount(1, delays);
		Assert.AreEqual(2000, delays[0]);
		Assert.Contains("start", calls[0].args);
		Assert.Contains("start", calls[1].args);
	}

	[TestMethod]
	public async Task UninstallAsync_StopsThenDeletesService()
	{
		var calls = new List<(string fileName, string args)>();
		var sut = new WindowsServiceInstaller(new FakeIWebLogger(),
			(fileName, args) =>
			{
				calls.Add(( fileName, args ));
				return Task.FromResult(true);
			},
			(_, _) => Task.FromResult(( true, string.Empty, 0 )),
			_ => Task.CompletedTask);

		var result = await sut.UninstallAsync();

		Assert.IsTrue(result);
		Assert.HasCount(2, calls);
		Assert.Contains("stop", calls[0].args);
		Assert.Contains("delete", calls[1].args);
	}

	[TestMethod]
	public async Task StopAsync_ReturnsFalse_WhenRunnerThrows()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => throw new InvalidOperationException("boom"),
			(_, _) => throw new InvalidOperationException("boom"),
			_ => Task.CompletedTask);

		var result = await sut.StopAsync();

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task InstallAsync_ProcessReturnsFalse_LogsErrorAndReturnsFalse()
	{
		// Arrange: runner returns false to simulate sc.exe failing to create the service
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => Task.FromResult(false),
			(_, _) => Task.FromResult(( false, string.Empty, 1 )),
			_ => Task.CompletedTask);

		// Act
		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.exe");

		// Assert
		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
		// The non-exception overload logs an error message in TrackedExceptions
		var found = logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("Failed to install Windows Service"));
		Assert.IsTrue(found, "Expected LogError message for failed install");
	}

	[TestMethod]
	public async Task InstallAsync_ProcessThrowsException_LogsExceptionAndReturnsFalse()
	{
		// Arrange: runner throws to trigger the catch block
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => throw new InvalidOperationException("sc crash"),
			(_, _) => throw new InvalidOperationException("sc crash"),
			_ => Task.CompletedTask);

		// Act
		var result = await sut.InstallAsync("C:/apps/starskymountwatchercli.exe");

		// Assert
		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
		var found = logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("Failed to install Windows service"));
		Assert.IsTrue(found, "Expected LogError(Exception, ...) for exception during install");
	}

	[TestMethod]
	public async Task StatusAsync_NotInstalled_ReturnsFalseFalse()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => Task.FromResult(true),
			(_, _) => Task.FromResult((false, string.Empty, 1)),
			_ => Task.CompletedTask);

		var (installed, running) = await sut.StatusAsync();

		Assert.IsFalse(installed);
		Assert.IsFalse(running);
	}

	[TestMethod]
	public async Task StatusAsync_InstalledButNotRunning_ReturnsTrueFalse()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => Task.FromResult(true),
			(_, _) => Task.FromResult((true, "STATE: STOPPED", 0)),
			_ => Task.CompletedTask);

		var (installed, running) = await sut.StatusAsync();

		Assert.IsTrue(installed);
		Assert.IsFalse(running);
	}

	[TestMethod]
	public async Task StatusAsync_InstalledAndRunning_ReturnsTrueTrue()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => Task.FromResult(true),
			(_, _) => Task.FromResult((true, "   RUNNING   ", 0)),
			_ => Task.CompletedTask);

		var (installed, running) = await sut.StatusAsync();

		Assert.IsTrue(installed);
		Assert.IsTrue(running);
	}

	[TestMethod]
	public async Task StatusAsync_RunThrows_ReturnsFalseFalse()
	{
		var logger = new FakeIWebLogger();
		var sut = new WindowsServiceInstaller(logger,
			(_, _) => Task.FromResult(true),
			(_, _) => throw new InvalidOperationException("boom"),
			_ => Task.CompletedTask);

		var (installed, running) = await sut.StatusAsync();

		Assert.IsFalse(installed);
		Assert.IsFalse(running);
	}
}
