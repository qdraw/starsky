using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.webtelemetry.Helpers;
using starskycore.Helpers;

namespace starskytest.Helpers
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
			
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			services.AddSingleton(context);

			services.AddSingleton<AppSettings>();

			services.AddSingleton<IUserManager, UserManager>();
			var mockTelemetryChannel = new MockTelemetryChannel();
			
			_telemetryConfiguration = new TelemetryConfiguration
			{
				TelemetryChannel = mockTelemetryChannel,
				InstrumentationKey = Guid.NewGuid().ToString()
			};
			_telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
			
			_telemetryClient = new TelemetryClient(_telemetryConfiguration);
			services
				.AddAuthentication(sharedOptions =>
				{
					sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				}).AddCookie();

			services.AddLogging();
			
			_serviceProvider = services.BuildServiceProvider();


			var httpContext = new DefaultHttpContext
			{
				RequestServices = _serviceProvider
			};
			
			services.AddSingleton<IHttpContextAccessor>(
				new HttpContextAccessor()
				{
					HttpContext = httpContext,
				});
			// and rebuild
			_serviceProvider = services.BuildServiceProvider();

		}
		
		[TestMethod]
		public void ApplicationInsightsJsHelper_NullCheck_ScriptTag()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

			var script = new ApplicationInsightsJsHelper(httpContext, null).ScriptTag;
			Assert.AreEqual(true, script.Contains("disabled"));
		}
		
		[TestMethod]
		public void ApplicationInsightsJsHelper_NullCheck_ScriptPlain()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();

			var script = new ApplicationInsightsJsHelper(httpContext, null).ScriptPlain;
			Assert.AreEqual(true, script.Contains("disabled"));
		}

		[TestMethod]
		public void ApplicationInsightsJsHelper_CheckIfContainsNonce_ScriptTag()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			
			var toCheckNonce = Guid.NewGuid() + "__";
			httpContext.HttpContext.Items["csp-nonce"] = toCheckNonce;
			
			var someOptions = Options.Create(new ApplicationInsightsServiceOptions());
			
			var fakeJs = new MockJavaScriptSnippet(_telemetryConfiguration, someOptions, httpContext,
				new JavaScriptTestEncoder());
			
			var script = new ApplicationInsightsJsHelper(httpContext, fakeJs).ScriptTag;
			
			Assert.AreEqual(true, script.Contains(toCheckNonce));
			Assert.AreEqual(true, script.Contains("Microsoft.ApplicationInsights"));
		}
		
		[TestMethod]
		public void ApplicationInsightsJsHelper_CheckIfContainsNonce_ScriptPlain()
		{
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			
			IOptions<ApplicationInsightsServiceOptions> someOptions = Options.Create<ApplicationInsightsServiceOptions>(new ApplicationInsightsServiceOptions());
			
			var fakeJs = new MockJavaScriptSnippet(_telemetryConfiguration, someOptions, httpContext,
				new JavaScriptTestEncoder());
			
			var script = new ApplicationInsightsJsHelper(httpContext, fakeJs).ScriptPlain;
			
			Assert.AreEqual(true, script.Contains("Microsoft.ApplicationInsights"));
		}

		[TestMethod]
		public async Task ApplicationInsightsJsHelper_GetCurrentUserId_WithId()
		{
			var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			var userManager = _serviceProvider.GetRequiredService<IUserManager>();

			var result = await userManager.SignUpAsync("test", "email", "test", "test");
			
			await userManager.SignIn(httpContextAccessor.HttpContext, result.User);

			IOptions<ApplicationInsightsServiceOptions> someOptions = Options.Create(new ApplicationInsightsServiceOptions());
			
			var fakeJs = new MockJavaScriptSnippet(_telemetryConfiguration, someOptions, httpContextAccessor,
				new JavaScriptTestEncoder());
			
			
			var script = new ApplicationInsightsJsHelper(httpContextAccessor, fakeJs).GetCurrentUserId();
			Assert.AreEqual(userManager.GetUser("email","test").Id.ToString(), script);
		}
		
		[TestMethod]
		public void ApplicationInsightsJsHelper_GetCurrentUserId_NotLogin()
		{
			var httpContextAccessor = new HttpContextAccessor
			{
				HttpContext = new DefaultHttpContext()
			};
			IOptions<ApplicationInsightsServiceOptions> someOptions = Options.Create<ApplicationInsightsServiceOptions>(new ApplicationInsightsServiceOptions());
			
			var fakeJs = new MockJavaScriptSnippet(_telemetryConfiguration, someOptions, httpContextAccessor,
				new JavaScriptTestEncoder());
			
			var script = new ApplicationInsightsJsHelper(httpContextAccessor, fakeJs).GetCurrentUserId();
			Assert.AreEqual("", script);
		}
	}
}
