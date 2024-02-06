using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using starsky.feature.demo.Services;
using starsky.feature.health.HealthCheck;
using starsky.feature.packagetelemetry.Services;
using starsky.feature.syncbackground.Services;
using starsky.foundation.accountmanagement.Extensions;
using starsky.foundation.database.Data;
using starsky.foundation.database.DataProtection;
using starsky.foundation.database.Helpers;
using starsky.foundation.diagnosticsource.Metrics;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Extentions;
using starsky.foundation.realtime.Model;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.Helpers;

namespace starsky
{
	public sealed class Startup
	{
		private readonly IConfigurationRoot _configuration;
		private AppSettings? _appSettings;

		/// <summary>
		/// application/xhtml+xml image/svg+xml
		/// </summary>
		private static readonly string[] CompressionMimeTypes =
		[
			"application/xhtml+xml",
			"image/svg+xml"
		];

		public Startup(string[]? args = null)
		{
			if ( !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("app__appsettingspath")) )
			{
				Console.WriteLine("app__appSettingsPath: " +
				                  Environment.GetEnvironmentVariable("app__appsettingspath"));
			}

			_configuration = SetupAppSettings.AppSettingsToBuilder(args).ConfigureAwait(false)
				.GetAwaiter().GetResult();
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		/// <param name="services">where from to configure</param>
		public void ConfigureServices(IServiceCollection services)
		{
			// By default, the last 15 most recently used static regular expression patterns are cached.
			// For applications that require a larger number of cached static regular expressions,
			// the size of the cache can be adjusted by setting the Regex.CacheSize property.
			// We are setting it to 15 + 15 = 30
			Regex.CacheSize += 15;

			_appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, _configuration);

			// before anything else
			EnableCompression(services);

			services.AddMemoryCache();
			// this is ignored here: appSettings.AddMemoryCache; but implemented in cache

			services.AddOpenTelemetryMonitoring(_appSettings);

			// LoggerFactory
			services.AddTelemetryLogging(_appSettings);

			var foundationDatabaseName = typeof(ApplicationDbContext)
				.Assembly.FullName?.Split(",").FirstOrDefault();
			new SetupDatabaseTypes(_appSettings, services).BuilderDb(foundationDatabaseName);
			new SetupHealthCheck(_appSettings, services).BuilderHealth();
			EfCoreMigrationsOnProject(services).ConfigureAwait(false);
			services.SetupDataProtection();

			// Enable Dual Authentication 
			services
				.AddAuthentication(sharedOptions =>
				{
					sharedOptions.DefaultAuthenticateScheme =
						CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultSignInScheme =
						CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultChallengeScheme =
						CookieAuthenticationDefaults.AuthenticationScheme;
				})
				.AddCookie(options =>
					{
						options.Cookie.Name = "_id";
						options.ExpireTimeSpan = TimeSpan.FromDays(60);
						options.SlidingExpiration = false;
						options.Cookie.HttpOnly = true;
						options.Cookie.IsEssential = true;
						options.Cookie.Path = "/";
						options.Cookie.SecurePolicy = _appSettings.HttpsOn == true
							? CookieSecurePolicy.Always
							: CookieSecurePolicy.SameAsRequest;
						options.Cookie.SameSite =
							SameSiteMode
								.Lax; // when on strict and visiting the page again its logged out
						options.LoginPath = "/account/login";
						options.LogoutPath = "/account/logout";
						options.Events.OnRedirectToLogin =
							ReplaceReDirector(HttpStatusCode.Unauthorized,
								options.Events.OnRedirectToLogin);
					}
				);

			// There is a base-cookie and in index controller there is an method to generate a token that is used to send with the header: X-XSRF-TOKEN
			services.AddAntiforgery(
				options =>
				{
					options.Cookie.Name = "_af";
					options.Cookie.HttpOnly =
						true; // only used by .NET, there is a separate method to generate a X-XSRF-TOKEN cookie
					options.Cookie.SameSite = SameSiteMode.Strict;
					options.Cookie.SecurePolicy = _appSettings.HttpsOn == true
						? CookieSecurePolicy.Always
						: CookieSecurePolicy.SameAsRequest;
					options.Cookie.Path = "/";
					options.Cookie.IsEssential = true;
					options.HeaderName = "X-XSRF-TOKEN";
				}
			);

			// to add support for swagger
			new SwaggerSetupHelper(_appSettings).Add01SwaggerGenHelper(services);

			// Now only for dev
			services.AddCors(options =>
			{
				options.AddPolicy("CorsDevelopment",
					builder => builder
						.WithOrigins(
							"http://localhost:4000",
							"https://localhost:4001",
							"http://localhost:4200",
							"http://localhost:5000",
							"https://localhost:5001",
							"http://localhost:5173",
							"http://localhost:8080"
						)
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials());

				options.AddPolicy("CorsProduction",
					builder => builder
						.AllowCredentials());
			});

			services.AddMvcCore(options =>
			{
				options.ModelBinderProviders.Insert(0, new DoubleBinderProvider());
			}).AddApiExplorer().AddAuthorization().AddViews();

			ConfigureForwardedHeaders(services);

			// Application Insights
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			RegisterDependencies.Configure(services);

			// Reference due missing links between services
			services.AddSingleton<PackageTelemetryBackgroundService>();
			services.AddSingleton<OnStartupSyncBackgroundService>();
			services.AddSingleton<CleanDemoDataService>();

			// todo remove
			services.AddSingleton<HatCoMetrics>();
		}

		/// <summary>
		/// Respect ForwardedHeaders
		/// </summary>
		private static void ConfigureForwardedHeaders(IServiceCollection services)
		{
			// Configure the X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders =
					ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
				// https://medium.com/@laimis/couple-issues-with-https-redirect-asp-net-core-7021cf383e00
				// Without the explicit Clear() call, it continued to do the infinite redirect.
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();
			});
		}

		/// <summary>
		/// Enable Gzip Compression Provider (.NET Core Compression)
		/// You need to enable this before anything else
		/// also needed to UseResponseCompression before using static content
		/// test without: `curl http://localhost:4000/starsky/manifest.json --silent --write-out "%{size_download}\n" --output /dev/null`
		/// test with: `curl http://localhost:4000/starsky/manifest.json --silent
		/// -H "Accept-Encoding: gzip,deflate" --write-out "%{size_download}\n" --output /dev/null`
		/// </summary>
		/// <param name="services"></param>
		private static void EnableCompression(IServiceCollection services)
		{
			services.AddResponseCompression(options =>
			{
				options.Providers.Add<GzipCompressionProvider>();
				options.MimeTypes =
					ResponseCompressionDefaults.MimeTypes.Concat(CompressionMimeTypes);
			});
			services.Configure<GzipCompressionProviderOptions>(options =>
			{
				options.Level = CompressionLevel.Fastest;
			});
		}

		/// <summary>
		/// Does the current user get a redirect or 401 page
		/// </summary>
		/// <param name="statusCode">current status code</param>
		/// <param name="existingReDirector">func of RedirectContext</param>
		/// <returns></returns>
		private static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceReDirector(
			HttpStatusCode statusCode,
			Func<RedirectContext<CookieAuthenticationOptions>, Task> existingReDirector) =>
			context =>
			{
				if ( !context.Request.Path.StartsWithSegments("/api") )
					return existingReDirector(context);
				context.Response.StatusCode = ( int )statusCode;
				var jsonString = "{\"errors\": [{\"status\": \"" + ( int )statusCode + "\" }]}";

				context.Response.ContentType = "application/json";
				var data = Encoding.UTF8.GetBytes(jsonString);
				return context.Response.Body.WriteAsync(data, 0, data.Length);
			};

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app">ApplicationBuilder</param>
		/// <param name="env">Hosting Env</param>
		/// <param name="applicationLifetime">application Lifetime</param>
		public void Configure(IApplicationBuilder app, IHostEnvironment env,
			IHostApplicationLifetime applicationLifetime)
		{
			app.UseResponseCompression();

			if ( env.IsDevelopment() ) app.UseDeveloperExceptionPage();
			app.UseCors(env.IsDevelopment() ? "CorsDevelopment" : "CorsProduction");
			// use ErrorController with Error
			app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

			// Enable X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
			app.UseForwardedHeaders();

			if ( !env.IsDevelopment() && _appSettings?.UseHttpsRedirection == true )
			{
				app.UseHttpsRedirection();
			}

			// Use the name of the application to use behind a reverse proxy
			app.UsePathBase(PathHelper.PrefixDbSlash("starsky"));

			app.UseRouting();

			new SwaggerSetupHelper(_appSettings!).Add02AppUseSwaggerAndUi(app);

			app.UseContentSecurityPolicy();

			SetupStaticFiles(app);

			app.UseAuthentication();
			app.UseBasicAuthentication();
			app.UseNoAccount(_appSettings?.NoAccountLocalhost == true ||
			                 _appSettings?.DemoUnsafeDeleteStorageFolder == true);
			app.UseCheckIfAccountExist();

			app.UseAuthorization();

			try
			{
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapControllerRoute("default",
						"{controller=Home}/{action=Index}/{id?}");
				});
			}
			catch ( InvalidOperationException )
			{
				// for testing
				// nothing here
			}

			app.UseWebSockets();
			app.MapWebSocketConnections("/realtime", new WebSocketConnectionsOptions(),
				_appSettings?.UseRealtime);
		}

		/// <summary>
		/// Add Static Files to the application
		/// </summary>
		/// <param name="app">Application builder</param>
		/// <param name="assetsName">assetsName</param>
		/// <returns>1. bool = local dir 2. wwwroot in assembly 3. clientapp</returns>
		internal (bool, bool, bool) SetupStaticFiles(IApplicationBuilder app,
			string assetsName = "assets")
		{
			var result = ( false, false, false );

			// Allow Current Directory and wwwroot in Base Directory
			// AppSettings can be null when running tests
			if ( _appSettings?.AddSwaggerExportExitAfter != true )
			{
				app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = PrepareResponse });
				result.Item1 = true;
			}

			if ( _appSettings == null )
			{
				return result;
			}

			// Use in wwwroot in build directory; the default option assumes Current Directory
			if ( Directory.Exists(Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot")) )
			{
				app.UseStaticFiles(new StaticFileOptions
				{
					OnPrepareResponse = PrepareResponse,
					FileProvider = new PhysicalFileProvider(
						Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot"))
				});
				result.Item2 = true;
			}

			// Check if clientapp is build and use the assets folder
			if ( !Directory.Exists(Path.Combine(
				    _appSettings.BaseDirectoryProject, "clientapp", "build", assetsName)) )
			{
				return result;
			}

			app.UseStaticFiles(new StaticFileOptions
				{
					OnPrepareResponse = PrepareResponse,
					FileProvider = new PhysicalFileProvider(
						Path.Combine(_appSettings.BaseDirectoryProject, "clientapp", "build",
							assetsName)),
					RequestPath = $"/assets",
				}
			);
			result.Item3 = true;
			return result;
		}

		internal static void PrepareResponse(StaticFileResponseContext ctx)
		{
			// Cache static files for 356 days
			ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(365)
				.ToString("R", CultureInfo.InvariantCulture));
			ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
		}

		/// <summary>
		/// Run the latest migration on the database. 
		/// To start over with a SQLite database please remove it and
		/// it will add a new one
		/// </summary>
		private static async Task EfCoreMigrationsOnProject(IServiceCollection serviceCollection)
		{
			using var serviceScope = serviceCollection.BuildServiceProvider().CreateScope();
			await RunMigrations.Run(serviceScope);
		}
	}
}
