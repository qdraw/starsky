using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Services;

[TestClass]
public sealed class WebLoggerTest
{
	private readonly IServiceScopeFactory _scopeFactory;

	public WebLoggerTest()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IConsole, FakeConsoleWrapper>();
		var serviceProvider = services.BuildServiceProvider();
		_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public void Error_string_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogError("error1");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.AreEqual("error1", error);
		Assert.AreEqual(LogLevel.Error, logLevel);
	}

	[TestMethod]
	public void Error_Null_string_ShouldSkipFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogError(null);
		var error = factory.Storage.ErrorLog.Count;
		var logLevel = factory.Storage.LogLevelLog.Count;

		Assert.AreEqual(0, error);
		Assert.AreEqual(0, logLevel);
	}

	[TestMethod]
	public void Error_string_ConsoleFallback()
	{
		new WebLogger(null, _scopeFactory).LogError("error_message");

		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;

		Assert.AreEqual("error_message", fakeConsole?.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void Error_Exception_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		var expectedException = new Exception("some thing bad happens");
		new WebLogger(factory).LogError(expectedException, "message");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.AreEqual(expectedException.Message, error);
		Assert.AreEqual(LogLevel.Error, logLevel);
	}

	[TestMethod]
	public void Error_Exception_ConsoleFallback()
	{
		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;
		var expectedException = new Exception("some thing bad happens console");
		new WebLogger(null, _scopeFactory).LogError(expectedException, "message");

		Assert.AreEqual("some thing bad happens console message",
			fakeConsole?.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void Information_string_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogInformation("error1");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.AreEqual("error1", error);
		Assert.AreEqual(LogLevel.Information, logLevel);
	}

	[TestMethod]
	public void Information_Null_string_ShouldSkipFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogInformation(null);
		var error = factory.Storage.ErrorLog.Count;
		var logLevel = factory.Storage.LogLevelLog.Count;

		Assert.AreEqual(0, error);
		Assert.AreEqual(0, logLevel);
	}

	[TestMethod]
	public void Information_string_ConsoleFallback()
	{
		new WebLogger(null, _scopeFactory).LogInformation("message_info");
		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;
		Assert.AreEqual("message_info", fakeConsole?.WrittenLines[0]);
	}

	[TestMethod]
	public void Debug_string_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogDebug("error1");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.AreEqual("error1", error);
		Assert.AreEqual(LogLevel.Debug, logLevel);
	}

	[TestMethod]
	public void Debug_Null_string_ShouldSkipFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogDebug(null);
		var error = factory.Storage.ErrorLog.Count;
		var logLevel = factory.Storage.LogLevelLog.Count;

		Assert.AreEqual(0, error);
		Assert.AreEqual(0, logLevel);
	}

	[TestMethod]
	public void Debug_string_ConsoleFallback()
	{
		new WebLogger(null, _scopeFactory).LogDebug("message_info");
		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;
		Assert.AreEqual("message_info", fakeConsole?.WrittenLines[0]);
	}

	[TestMethod]
	public void Information_Exception_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		var expectedException = new Exception("some thing bad happens");
		new WebLogger(factory).LogInformation(expectedException, "message_ex");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.AreEqual(expectedException.Message, error);
		Assert.AreEqual(LogLevel.Information, logLevel);
	}

	[TestMethod]
	public void Warning_Exception_String_ShouldPassFakeLogger()
	{
		var factory = new FakeILoggerFactory();
		new WebLogger(factory).LogWarning(new Exception(),
			"warning");
		var error = factory.Storage.ErrorLog[0];
		var logLevel = factory.Storage.LogLevelLog[0];

		Assert.Contains("Exception", error);
		Assert.AreEqual(LogLevel.Warning, logLevel);
	}

	[TestMethod]
	public void Warning_Exception_String_ConsoleFallback()
	{
		new WebLogger(null, _scopeFactory).LogWarning(new Exception(),
			"warning");
		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;

		Assert.Contains("Exception", fakeConsole?.WrittenLines[0]!);
	}

	[TestMethod]
	public void Information_Exception_ConsoleFallback()
	{
		var expectedException = new Exception("some thing bad happens");
		new WebLogger(null, _scopeFactory).LogInformation(expectedException, "message info");
		var fakeConsole =
			_scopeFactory.CreateScope().ServiceProvider
				.GetService<IConsole>() as FakeConsoleWrapper;
		Assert.AreEqual("some thing bad happens message info", fakeConsole?.WrittenLines[0]);
	}

	[TestMethod]
	public void AllLogMethods_WithNullLoggerAndNullConsole_DoNotThrow_And_PrivateFieldsRemainNull()
	{
		// Arrange
		var webLogger = new WebLogger();
		Assert.IsNotNull(webLogger);

		var type = typeof(WebLogger);
		var loggerField = type.GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic);
		var consoleField =
			type.GetField("_console", BindingFlags.Instance | BindingFlags.NonPublic);

		Assert.IsNotNull(loggerField, "Expected private field '_logger' to exist");
		Assert.IsNotNull(consoleField, "Expected private field '_console' to exist");

		var loggerValueBefore = loggerField.GetValue(webLogger);
		var consoleValueBefore = consoleField.GetValue(webLogger);

		Assert.IsNull(loggerValueBefore, "_logger should be null when no ILoggerFactory provided");
		Assert.IsNull(consoleValueBefore,
			"_console should be null when no IServiceScopeFactory provided");

		// Act: call all logging methods (should not throw)
		webLogger.LogDebug("debug {0}", 1);
		webLogger.LogInformation("info {0}", 2);
		webLogger.LogInformation(new Exception("ex"), "info ex {0}", 3);
		webLogger.LogError("error {0}", 4);
		webLogger.LogError(new Exception("err"), "error ex {0}", 5);

		// Assert: private fields remain null after calls
		var loggerValueAfter = loggerField.GetValue(webLogger);
		var consoleValueAfter = consoleField.GetValue(webLogger);

		Assert.IsNull(loggerValueAfter, "_logger should remain null after log calls");
		Assert.IsNull(consoleValueAfter, "_console should remain null after log calls");
	}

	[TestMethod]
	public void SafeLog_WhenLoggerThrows_WritesToConsole()
	{
		var capture = new FakeConsoleWrapper();

		// Build a simple service collection that returns our IConsole when requested
		var services = new ServiceCollection();
		services.AddSingleton<IConsole>(capture);

		var scopeFactory = new FakeServiceScopeFactory(services);
		var loggerFactory = new ThrowingLoggerFactory();

		var webLogger = new WebLogger(loggerFactory, scopeFactory);

		// This will call into the ThrowingLogger which throws; SafeLog should catch and
		// write a message to the injected IConsole instance.
		webLogger.LogInformation("hello world");

		Assert.IsNotEmpty(capture.WrittenLines, "No console output captured");
		Assert.Contains("Logging failed:", capture.WrittenLines[0]);
		Assert.Contains("boom", capture.WrittenLines[0]);
	}

	private sealed class FakeILogger : ILogger
	{
		public List<string> ErrorLog { get; } = new();
		public List<LogLevel> LogLevelLog { get; } = new();

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception? exception, Func<TState, Exception, string> formatter)
		{
			LogLevelLog.Add(logLevel);
			if ( exception != null )
			{
				ErrorLog.Add(exception.Message);
				return;
			}

			ErrorLog.Add(state!.ToString()!);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
		public IDisposable BeginScope<TState>(TState state)
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
		{
			throw new NotImplementedException();
		}
	}

	private sealed class ThrowingLogger : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
			Exception? exception, Func<TState, Exception?, string> formatter)
		{
			throw new InvalidOperationException("boom");
		}
	}

	private sealed class ThrowingLoggerFactory : ILoggerFactory
	{
		public void AddProvider(ILoggerProvider provider)
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new ThrowingLogger();
		}

		public void Dispose()
		{
		}
	}


	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
	[SuppressMessage("Performance",
		"CA1852: Type can be sealed because it has no subtypes in its containing assembly")]
	private class FakeILoggerFactory : ILoggerFactory
	{
		public FakeILogger Storage { get; } = new();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public ILogger CreateLogger(string categoryName)
		{
			return Storage;
		}

		public void AddProvider(ILoggerProvider provider)
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			// Cleanup
		}
	}
}
