using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Extensions;

[TestClass]
public sealed class TestConnectionTest
{
	[TestMethod]
	public void CatchMySqlException_ConnectionIsFalse()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var result = new AppDbMySqlException(options).TestConnection(new FakeIWebLogger());
		Assert.IsFalse(result);
	}

	private sealed class AppDbMySqlException : ApplicationDbContext
	{
		public AppDbMySqlException(DbContextOptions options) : base(options)
		{
		}

		public int Count { get; set; }

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
		public override DatabaseFacade? Database
		{
			get
#pragma warning restore CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
			{
				Count++;
				if ( Count <= 1 )
				{
					return null;
				}

#pragma warning disable SYSLIB0050
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
				info.AddValue("WatsonBuckets", Array.Empty<byte>());

				// private MySqlException(SerializationInfo info, StreamingContext context)
				var ctor =
					typeof(MySqlException).GetConstructors(BindingFlags.Instance |
					                                       BindingFlags.NonPublic |
					                                       BindingFlags.InvokeMethod)
						.FirstOrDefault();
				var instance =
					( MySqlException ) ctor!.Invoke(new object[]
					{
						info, new StreamingContext(StreamingContextStates.All)
					});
#pragma warning restore SYSLIB0050

				throw instance;
			}
		}
	}
}
