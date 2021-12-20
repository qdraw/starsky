using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Services
{
	[TestClass]
	public class WebLoggerTest
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public class FakeILogger : ILogger
		{
			public List<string> ErrorLog { get; set; } = new List<string>();
			public List<LogLevel> LogLevelLog { get; set; } = new List<LogLevel>();

			public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
				Exception exception, Func<TState, Exception, string> formatter)
			{
				LogLevelLog.Add(logLevel);
				if ( exception != null )
				{
					ErrorLog.Add(exception.Message);
					return;
				}
				ErrorLog.Add(state.ToString());
			}

			public bool IsEnabled(LogLevel logLevel)
			{
				throw new NotImplementedException();
			}

			public IDisposable BeginScope<TState>(TState state)
			{
				throw new NotImplementedException();
			}
		}

		
		public class FakeILoggerFactory : ILoggerFactory
		{
			public FakeILogger Storage { get; set; } = new FakeILogger();

			public void Dispose()
			{
			}

			public ILogger CreateLogger(string categoryName)
			{
				return Storage;
			}

			public void AddProvider(ILoggerProvider provider)
			{
			}
		}

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

			Assert.AreEqual("error1",error);
			Assert.AreEqual(LogLevel.Error,	logLevel);
		}
				
		[TestMethod]
		public void Error_string_ConsoleFallback()
		{
			new WebLogger(null, _scopeFactory).LogError("error_message");
			
			var fakeConsole = _scopeFactory?.CreateScope().ServiceProvider.GetService<IConsole>() as FakeConsoleWrapper;
			
			Assert.AreEqual("error_message",fakeConsole.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void Error_Exception_ShouldPassFakeLogger()
		{
			var factory = new FakeILoggerFactory();
			var expectedException = new Exception("some thing bad happens");
			new WebLogger(factory).LogError(expectedException, "message");
			var error = factory.Storage.ErrorLog[0];
			var logLevel = factory.Storage.LogLevelLog[0];

			Assert.AreEqual(expectedException.Message,error);
			Assert.AreEqual(LogLevel.Error,	logLevel);
		}
		
		[TestMethod]
		public void Error_Exception_ConsoleFallback()
		{
			var fakeConsole = _scopeFactory?.CreateScope().ServiceProvider.GetService<IConsole>() as FakeConsoleWrapper;
			var expectedException = new Exception("some thing bad happens console");
			new WebLogger(null, _scopeFactory).LogError(expectedException, "message");
			
			Assert.AreEqual("some thing bad happens console message",fakeConsole.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void Information_string_ShouldPassFakeLogger()
		{
			var factory = new FakeILoggerFactory();
			new WebLogger(factory).LogInformation("error1");
			var error = factory.Storage.ErrorLog[0];
			var logLevel = factory.Storage.LogLevelLog[0];

			Assert.AreEqual("error1",error);
			Assert.AreEqual(LogLevel.Information,	logLevel);
		}
		
		[TestMethod]
		public void Information_string_ConsoleFallback()
		{
			new WebLogger(null, _scopeFactory).LogInformation("message_info");
			var fakeConsole = _scopeFactory?.CreateScope().ServiceProvider.GetService<IConsole>() as FakeConsoleWrapper;
			Assert.AreEqual("message_info",fakeConsole.WrittenLines[0]);
		}
		
		[TestMethod]
		public void Debug_string_ShouldPassFakeLogger()
		{
			var factory = new FakeILoggerFactory();
			new WebLogger(factory).LogDebug("error1");
			var error = factory.Storage.ErrorLog[0];
			var logLevel = factory.Storage.LogLevelLog[0];

			Assert.AreEqual("error1",error);
			Assert.AreEqual(LogLevel.Debug,	logLevel);
		}
		
		[TestMethod]
		public void Debug_string_ConsoleFallback()
		{
			new WebLogger(null, _scopeFactory).LogDebug("message_info");
			var fakeConsole = _scopeFactory?.CreateScope().ServiceProvider.GetService<IConsole>() as FakeConsoleWrapper;
			Assert.AreEqual("message_info",fakeConsole.WrittenLines[0]);
		}
		
		[TestMethod]
		public void Information_Exception_ShouldPassFakeLogger()
		{
			var factory = new FakeILoggerFactory();
			var expectedException = new Exception("some thing bad happens");
			new WebLogger(factory).LogInformation(expectedException, "message_ex");
			var error = factory.Storage.ErrorLog[0];
			var logLevel = factory.Storage.LogLevelLog[0];

			Assert.AreEqual(expectedException.Message,error);
			Assert.AreEqual(LogLevel.Information,	logLevel);
		}

		[TestMethod]
		public void Information_Exception_ConsoleFallback()
		{
			var expectedException = new Exception("some thing bad happens");
			new WebLogger(null, _scopeFactory).LogInformation(expectedException, "message info");
			var fakeConsole = _scopeFactory?.CreateScope().ServiceProvider.GetService<IConsole>() as FakeConsoleWrapper;
			Assert.AreEqual("some thing bad happens message info",fakeConsole.WrittenLines[0]);
		}
	}
}
