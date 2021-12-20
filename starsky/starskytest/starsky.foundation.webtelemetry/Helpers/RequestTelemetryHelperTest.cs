using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.webtelemetry.Models;

namespace starskytest.starsky.foundation.webtelemetry.Helpers
{
	[TestClass]
	public class RequestTelemetryHelperTest
	{
		[TestMethod]
		public void GetOperationIdNull()
		{
			HttpContext context = null;
			var id = context.GetOperationId();
			Assert.AreEqual(string.Empty, id);
		}
		
		[TestMethod]
		public void GetOperationIdTest1()
		{
			var context = new DefaultHttpContext();
			context.Features.Set(new RequestTelemetry());

			var id = context.GetOperationId();
			Assert.AreEqual(null, id);
		}
		
		[TestMethod]
		public void GetOperationIdTest2()
		{
			var context = new DefaultHttpContext();
			context.Features.Set(new RequestTelemetry{ Context = { Operation = { Id = "1"}}});

			var id = context.GetOperationId();
			Assert.AreEqual("1", id);
		}
		
				
		[TestMethod]
		public void GetOperationHolderNull()
		{
			var holder = RequestTelemetryHelper.GetOperationHolder(
				null,null, null) as EmptyOperationHolder<DependencyTelemetry>;
			Assert.IsNotNull(holder);
			Assert.IsTrue(holder.Empty);
		}
		
						
		[TestMethod]
		public void GetOperationHolderTest1()
		{
			var service = new ServiceCollection();
			var serviceProvider = service.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var holder = RequestTelemetryHelper.GetOperationHolder(
				scopeFactory,null, "1") as EmptyOperationHolder<DependencyTelemetry>;
			Assert.IsNotNull(holder);
			Assert.IsTrue(holder.Empty);
		}
		
		[TestMethod]
		public void GetOperationHolderTest2()
		{
			var service = new ServiceCollection();
			service.AddSingleton<TelemetryClient>();
			var serviceProvider = service.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var holder =
				RequestTelemetryHelper.GetOperationHolder(scopeFactory, "1",
					"1");
			
			Assert.IsNotNull(holder);
			Assert.IsNotNull(holder.Telemetry.Timestamp);
		}
		
		[TestMethod]
		public void SetDataDataNull()
		{
			var holder = new EmptyOperationHolder<DependencyTelemetry>();
			holder.SetData(null);
			Assert.IsNotNull(holder);
		}
		
		[TestMethod]
		public void SetDataNullHolder()
		{
			IOperationHolder<DependencyTelemetry> holder = null;
			
			// ReSharper disable once ExpressionIsAlwaysNull
			holder.SetData(null);
			Assert.IsNull(holder);
		}
		
		[TestMethod]
		public void SetData()
		{
			var service = new ServiceCollection();
			service.AddSingleton<TelemetryClient>();
			var serviceProvider = service.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var holder = RequestTelemetryHelper.GetOperationHolder(scopeFactory, "1",
					"1");
			
			Assert.IsNotNull(holder);
			Assert.IsNotNull(holder.Telemetry.Timestamp);
		}
	}
}
