using System.Data;
using System.Data.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.databasetelemetry.Services;

namespace starskytest.starsky.foundation.databasetelemetry.Services
{
	[TestClass]
	public class DatabaseTelemetryInterceptorTest
	{
		[TestMethod]
		public void DatabaseTelemetryInterceptor_GetSqlName_Ok()
		{
			var result =
				DatabaseTelemetryInterceptor.GetSqlName(
					new MySqlCommand("t", new MySqlConnection("Server=test.nl;database=test;uid=test;pwd=test;")));
			
			Assert.AreEqual("test.nl | test",result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_GetSqlName_Null()
		{
			var result =
				DatabaseTelemetryInterceptor.GetSqlName(
					new MySqlCommand("t", null));
			
			Assert.AreEqual("SQLDatabase",result);
		}

		[TestMethod]
		public void DatabaseTelemetryInterceptor_CommandCreating()
		{
			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.CommandCreating(null, new InterceptionResult<DbCommand>());
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_CommandCreated()
		{
			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.CommandCreated(null, null);
			Assert.IsNull(result);
		}
				
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ReaderExecuting()
		{
			InterceptionResult<DbDataReader> eventData = InterceptionResult<DbDataReader>.SuppressWithResult(new DataTableReader(new DataTable()));
			
			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ReaderExecuting(null, null, eventData);
			Assert.IsNotNull(result);
		}
				
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ScalarExecuting()
		{
			InterceptionResult<object> eventData = InterceptionResult<object>.SuppressWithResult(new DataTableReader(new DataTable()));

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ScalarExecuting(null, null, eventData);
			Assert.IsNotNull(result);
		}
		
						
		[TestMethod]
		public void DatabaseTelemetryInterceptor_NonQueryExecuting()
		{
			InterceptionResult<int> eventData = InterceptionResult<int>.SuppressWithResult(1);

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.NonQueryExecuting(null, null, eventData);
			Assert.IsNotNull(result);
		}
		
								
								
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ReaderExecutingAsync()
		{
			InterceptionResult<DbDataReader> eventData = InterceptionResult<DbDataReader>.SuppressWithResult(new DataTableReader(new DataTable()));

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ReaderExecutingAsync(null, null, eventData);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ScalarExecutingAsync()
		{
			InterceptionResult<object> eventData = InterceptionResult<object>.SuppressWithResult(1);

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ScalarExecutingAsync(null, null, eventData);
			Assert.IsNotNull(result);
		}
				
		[TestMethod]
		public void DatabaseTelemetryInterceptor_NonQueryExecutingAsync()
		{
			InterceptionResult<int> eventData = InterceptionResult<int>.SuppressWithResult(1);

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.NonQueryExecutingAsync(null, null, eventData);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ReaderExecuted()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ReaderExecuted(null, null, null);
			Assert.IsNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ScalarExecuted()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ScalarExecuted(null, null, null);
			Assert.IsNull(result);
		}
		
										
		[TestMethod]
		public void DatabaseTelemetryInterceptor_NonQueryExecuted()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.NonQueryExecuted(null, null, 1);
			Assert.IsNotNull(result);
		}
		
												
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ReaderExecutedAsync()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ReaderExecutedAsync(null, null, new DataTableReader(new DataTable()));
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_ScalarExecutedAsync()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.ScalarExecutedAsync(null, null, 1);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_NonQueryExecutedAsync()
		{

			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.NonQueryExecutedAsync(null, null, 1);
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_CommandFailed()
		{
			CommandErrorEventData eventData = null;
			// void >
			new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.CommandFailed(null, eventData);
			Assert.IsNull(eventData);
		}
				
		[TestMethod]
		public void DatabaseTelemetryInterceptor_CommandFailedAsync()
		{
			CommandErrorEventData eventData = null;
			// void >
			new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.CommandFailedAsync(null, eventData);
			Assert.IsNull(eventData);
		}

		[TestMethod]
		public void DatabaseTelemetryInterceptor_DataReaderDisposing__EFMigrationsHistory()
		{
			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.DataReaderDisposing(new MySqlCommand("__EFMigrationsHistory"), null, InterceptionResult.Suppress());
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public void DatabaseTelemetryInterceptor_DataReaderDisposing_test()
		{
			var result = new DatabaseTelemetryInterceptor(
					new TelemetryClient(new TelemetryConfiguration()))
				.DataReaderDisposing(new MySqlCommand("test"), null, InterceptionResult.Suppress());
			Assert.IsNotNull(result);
		}
	}
}
