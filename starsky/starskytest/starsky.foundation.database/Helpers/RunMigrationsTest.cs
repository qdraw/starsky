using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class RunMigrationsTest
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task Test()
		{
			IServiceCollection services = new ServiceCollection();
			var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
			services
				.AddDbContext<ApplicationDbContext>(b =>
					b.UseInMemoryDatabase("test1234").UseInternalServiceProvider(efServiceProvider));
			var serviceProvider = services.BuildServiceProvider();
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			Assert.IsNotNull(serviceScopeFactory);
			await RunMigrations.Run(serviceScopeFactory.CreateScope());
			// expect exception: Relational-specific methods can only be used when the context is using a relational database provider.
		}

		private class AppDbMySqlException : ApplicationDbContext
		{
			public AppDbMySqlException(DbContextOptions options) : base(options)
			{
			}

			public override DatabaseFacade Database
			{
				get
				{
					var info = new SerializationInfo(typeof(Exception),
						new FormatterConverter());
					info.AddValue("Number", 1);
					info.AddValue("SqlState", "SqlState");
					info.AddValue("Message", "");
					info.AddValue("InnerException", new Exception());
					info.AddValue("HelpURL", "");
					info.AddValue("StackTraceString", "");
					info.AddValue("RemoteStackTraceString", "");
					info.AddValue("RemoteStackIndex", 1);
					info.AddValue("HResult", 1);
					info.AddValue("Source", "");
					info.AddValue("WatsonBuckets", new byte[0]);
					
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

					throw instance;
				}
			}
		}

		[TestMethod]
		public async Task MySqlException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			Assert.IsNotNull(options);
			await RunMigrations.Run(new AppDbMySqlException(options), new FakeIWebLogger());
			
			// should not crash
		}
	}
}
