using System;
using System.Data;
using System.Data.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.databasetelemetry.Helpers;

namespace starskytest.starsky.foundation.databasetelemetry.Helpers
{
	[TestClass]
	public sealed class TrackDependencyTest
	{
		/// <summary>
		/// Test code --> scroll down
		/// </summary>
		private class TestDbCommand : DbCommand
		{
			public override void Cancel()
			{
				throw new NotImplementedException();
			}

			public override int ExecuteNonQuery()
			{
				throw new NotImplementedException();
			}

			public override object ExecuteScalar()
			{
				throw new NotImplementedException();
			}

			public override void Prepare()
			{
				throw new NotImplementedException();
			}

			public override string CommandText { get; set; } = string.Empty;
			public override int CommandTimeout { get; set; }
			public override CommandType CommandType { get; set; }
			public override UpdateRowSource UpdatedRowSource { get; set; }
			protected override DbConnection? DbConnection { get; set; }

			protected override DbParameterCollection DbParameterCollection
			{
				get;
			} = null!;

			protected override DbTransaction? DbTransaction { get; set; }
			public override bool DesignTimeVisible { get; set; }

			protected override DbParameter CreateDbParameter()
			{
				throw new NotImplementedException();
			}

			protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
			{
				throw new NotImplementedException();
			}
		}
		
		[TestMethod]
		public void Track_ShouldGiveBackTrue()
		{
			var command = new TestDbCommand() as DbCommand;
			var result = new TrackDependency(new TelemetryClient(new TelemetryConfiguration())).Track(command, DateTimeOffset.Now, "", "");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void Track_ShouldGiveBackTrue_default_DateTimeOffset()
		{
			var command = new TestDbCommand() as DbCommand;
			var result = new TrackDependency(new TelemetryClient(new TelemetryConfiguration())).Track(command, default(DateTimeOffset), "", "");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void Track_ShouldGiveBackFalse_WhenNull()
		{
			var command = new TestDbCommand() as DbCommand;
			var result = new TrackDependency(null).Track(command, DateTimeOffset.Now, "", "");
			Assert.IsFalse(result);
		}
	}
}
