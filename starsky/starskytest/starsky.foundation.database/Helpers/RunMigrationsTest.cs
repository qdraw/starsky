using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class RunMigrationsTest
	{
		[TestMethod]
		public async Task Test()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
			services
				.AddDbContext<ApplicationDbContext>(b =>
					b.UseInMemoryDatabase("test1234").UseInternalServiceProvider(efServiceProvider));
			services.AddSingleton<AppSettings>();

			var serviceProvider = services.BuildServiceProvider();
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var appSettings = serviceProvider.GetRequiredService<AppSettings>();

			appSettings.DatabaseType = AppSettings.DatabaseTypeList.Mysql;
			
			Assert.IsNotNull(serviceScopeFactory);
			await RunMigrations.Run(serviceScopeFactory.CreateScope(),1);
			// expect exception: Relational-specific methods can only be used when the context is using a relational database provider.
		}

		private static MySqlException CreateMySqlException(string message)
		{
			var info = new SerializationInfo(typeof(Exception),
				new FormatterConverter());
			info.AddValue("Number", 1);
			info.AddValue("SqlState", "SqlState");
			info.AddValue("Message", message);
			info.AddValue("InnerException", new Exception());
			info.AddValue("HelpURL", "");
			info.AddValue("StackTraceString", "");
			info.AddValue("RemoteStackTraceString", "");
			info.AddValue("RemoteStackIndex", 1);
			info.AddValue("HResult", 1);
			info.AddValue("Source", "");
			info.AddValue("WatsonBuckets",  Array.Empty<byte>() );
					
			// private MySqlException(SerializationInfo info, StreamingContext context)
			var ctor =
				typeof(MySqlException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			var instance =
				( MySqlException ) ctor.Invoke(new object[]
				{
					info,
					new StreamingContext(StreamingContextStates.All)
				});
			return instance;
		}
		
		private class AppDbMySqlException : ApplicationDbContext
		{
			public AppDbMySqlException(DbContextOptions options) : base(options)
			{
			}

			public override DatabaseFacade Database => throw CreateMySqlException("");

		}

		[TestMethod]
		public async Task MySqlException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			Assert.IsNotNull(options);
			await RunMigrations.Run(
				new AppDbMySqlException(options), new FakeIWebLogger(),new AppSettings{DatabaseType = AppSettings.DatabaseTypeList.Mysql},1);
			
			// should not crash
		}

		

		private class AppDbMySqlException2 : ApplicationDbContext
		{
			public AppDbMySqlException2(DbContextOptions options) : base(options)
			{
			}

			public override DatabaseFacade Database => new DatabaseFacade(this);


		}
		
		public static void HijackMethod(Type sourceType, string sourceMethod, Type targetType, string targetMethod)
		{
			// Get methods using reflection
			var source = sourceType.GetMethod(sourceMethod,new []{typeof(CancellationToken)});
			var target = targetType.GetMethod(targetMethod);

			// Prepare methods to get machine code (not needed in this example, though)
			RuntimeHelpers.PrepareMethod(source.MethodHandle);
			RuntimeHelpers.PrepareMethod(target.MethodHandle);

			var sourceMethodDescriptorAddress = source.MethodHandle.Value;
			var targetMethodMachineCodeAddress = target.MethodHandle.GetFunctionPointer();

			// Pointer is two pointers from the beginning of the method descriptor
			Marshal.WriteIntPtr(sourceMethodDescriptorAddress, 2 * IntPtr.Size, targetMethodMachineCodeAddress);
		}

		public class MyClass
		{
			public Task OpenAsync()
			{
				Console.WriteLine("Sdfsdf");
				return Task.CompletedTask;
			}
		}
		
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		[Timeout(300)]
		public async Task MigrateAsync1()
		{
			var sql = new MySqlConnection(null);
			HijackMethod(typeof(MySqlConnection), nameof(MySqlConnection.OpenAsync), 
				typeof(MyClass), nameof(MyClass.OpenAsync));
			
			await sql.OpenAsync(); // only for test
			
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			await RunMigrations.MysqlFixes(sql,
				new AppSettings
				{
					DatabaseType = AppSettings.DatabaseTypeList.Mysql
				}, new AppDbMySqlException2(options));
		}

	}
}
