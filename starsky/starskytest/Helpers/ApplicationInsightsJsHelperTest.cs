using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytests.Helpers
{

	
	public class MockTelemetryChannel : ITelemetryChannel
	{
		public IList<ITelemetry> Items
		{
			get;
			private set;
		}

		public void Send(ITelemetry item)
		{
			Items.Add(item);
		}

		public void Flush()
		{
			throw new System.NotImplementedException();
		}

		public bool? DeveloperMode { get; set; }
		public string EndpointAddress { get; set; }

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
	
	public class MockJavaScriptSnippet : JavaScriptSnippet
	{
		public MockJavaScriptSnippet(TelemetryConfiguration telemetryConfiguration, IOptions<ApplicationInsightsServiceOptions> serviceOptions, IHttpContextAccessor httpContextAccessor, JavaScriptEncoder encoder) : base(telemetryConfiguration, serviceOptions, httpContextAccessor, encoder)
		{
		}
	}


	[TestClass]
	public class ApplicationInsightsJsHelperTest
	{
		private ServiceProvider _serviceProvider;
		private TelemetryClient _telemetryClient;
		private TelemetryConfiguration _telemetryConfiguration;

		public ApplicationInsightsJsHelperTest()
		{
			var services = new ServiceCollection();
			// IHttpContextAccessor is required for SignInManager, and UserManager
			var context = new DefaultHttpContext();
			services.AddSingleton<IHttpContextAccessor>(
				new HttpContextAccessor()
				{
					HttpContext = context,
				});
			
			
			var mockTelemetryChannel = new MockTelemetryChannel();
			
			_telemetryConfiguration = new TelemetryConfiguration
			{
				TelemetryChannel = mockTelemetryChannel,
				InstrumentationKey = Guid.NewGuid().ToString()
			};
			_telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
			
			_telemetryClient = new TelemetryClient(_telemetryConfiguration);
			
			_serviceProvider = services.BuildServiceProvider();

		}
		
		[TestMethod]
		public void ApplicationInsightsJsHelper_NullCheck()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

			var script = new ApplicationInsightsJsHelper(httpContext, null).Script;
			Assert.AreEqual(true, script.Contains("disabled"));
		}

		[TestMethod]
		public void ApplicationInsightsJsHelper_CheckIfContainsNonce()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			
			var toCheckNonce = Guid.NewGuid() + "__";
			httpContext.HttpContext.Items["csp-nonce"] = toCheckNonce;
			
			IOptions<ApplicationInsightsServiceOptions> someOptions = Options.Create<ApplicationInsightsServiceOptions>(new ApplicationInsightsServiceOptions());
			
			var fakeJs = new MockJavaScriptSnippet(_telemetryConfiguration, someOptions, httpContext,
				new JavaScriptTestEncoder());
			
			var script = new ApplicationInsightsJsHelper(httpContext, fakeJs).Script;
			
			Assert.AreEqual(true, script.Contains(toCheckNonce));
			Assert.AreEqual(true, "window.appInsights=appInsights,");

		}
	}
}
