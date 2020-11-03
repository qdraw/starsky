using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Health;

namespace starskytest.Health
{
	[TestClass]
	public class HealthResponseWriterTest
	{
		
		[TestMethod]
		public void FailTest()
		{
			var httpContext = new DefaultHttpContext();
			var content = new Dictionary<string, HealthReportEntry>
			{
				// HealthReportEntry(HealthStatus status, string description, TimeSpan duration, Exception exception, data
				{ "test", new HealthReportEntry(HealthStatus.Unhealthy,"",TimeSpan.Zero,null,null ) }
			};
			var healthReport = new HealthReport(content, TimeSpan.Zero);
			HealthResponseWriter.WriteResponse(httpContext, healthReport).ConfigureAwait(false);
                          
			Assert.AreEqual("application/json; charset=utf-8",httpContext.Response.ContentType);                          
		}
	}
}

