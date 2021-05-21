using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.webtelemetry.Services;

namespace starskytest.starsky.foundation.platform.Services
{
	[TestClass]
	public class FilterStatusCodesInitializerTest
	{
		[TestMethod]
		public void Overwrite401ResultToSuccess()
		{
			var tel = new RequestTelemetry("string name", 
				new DateTimeOffset(), new TimeSpan(), "401",false);
			new FilterStatusCodesInitializer().Initialize(tel);
			Assert.AreEqual(tel.Success, true);
		}
		
		[TestMethod]
		public void Overwrite404ResultToSuccess()
		{
			var tel = new RequestTelemetry("string name", 
				new DateTimeOffset(), new TimeSpan(), "404",false);
			new FilterStatusCodesInitializer().Initialize(tel);
			Assert.AreEqual(tel.Success, true);
		}
				
		[TestMethod]
		public void IgnoreOtherStatus()
		{
			var tel = new RequestTelemetry("string name", 
				new DateTimeOffset(), new TimeSpan(), "500",false);
			new FilterStatusCodesInitializer().Initialize(tel);
			Assert.AreEqual(tel.Success, false);
		}
		
		[TestMethod]
		public void IgnoreNullInput()
		{
			RequestTelemetry input = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			new FilterStatusCodesInitializer().Initialize(input);
			// should not crash
			Assert.IsNull(input);
		}
	}
}
