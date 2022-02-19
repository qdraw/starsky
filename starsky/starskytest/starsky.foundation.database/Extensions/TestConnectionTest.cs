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

namespace starskytest.starsky.foundation.database.Extensions
{
	[TestClass]
	public class TestConnectionTest
	{
		private class AppDbMySqlException : ApplicationDbContext
		{
			public AppDbMySqlException(DbContextOptions options) : base(options)
			{
			}

			public int Count { get; set; }

			public override DatabaseFacade Database
			{
				get
				{
					Count++;
					if ( Count <= 1 )
					{
						return null;
					}
					
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

					throw instance;
				}
			}
		}
		
		[TestMethod]
		public void CatchMySqlException_ConnectionIsFalse()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "MovieListDatabase")
				.Options;
			
			var result = new AppDbMySqlException(options).TestConnection();
			Assert.IsFalse(result);
		}
	}
}
