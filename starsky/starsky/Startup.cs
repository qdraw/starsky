using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
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
using starsky.feature.health.HealthCheck;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.injection;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.realtime.Extentions;
using starsky.foundation.realtime.Model;
using starsky.foundation.webtelemetry.Extensions;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.webtelemetry.Processor;
using starsky.Helpers;

namespace starsky
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private AppSettings _appSettings;
        private readonly IHostEnvironment _hostEnvironment;

        public Startup(IHostEnvironment hostEnvironment = null)
		{
			_hostEnvironment = hostEnvironment;
			_configuration = SetupAppSettings.AppSettingsToBuilder().Result;
		}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        _appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, _configuration);

	        // before anything else
			EnableCompression(services);
			
            services.AddMemoryCache();
            // this is ignored here: appSettings.AddMemoryCache; but implemented in cache
            
            SetupLogging.AddLogging(services,_appSettings);

            var foundationDatabaseName = typeof(ApplicationDbContext).Assembly.FullName.Split(",").FirstOrDefault();
            new SetupDatabaseTypes(_appSettings,services, new ConsoleWrapper()).BuilderDb(foundationDatabaseName);
			new SetupHealthCheck(_appSettings,services).BuilderHealth();
	            
            // Enable Dual Authentication 
            services
                .AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                })
                .AddCookie(options =>
                    {
                        options.Cookie.Name = "_id";
                        options.ExpireTimeSpan = TimeSpan.FromDays(60);
                        options.SlidingExpiration = false;
                        options.Cookie.HttpOnly = true;
                        options.Cookie.IsEssential = true;
                        options.Cookie.Path = "/";
                        options.Cookie.SameSite = SameSiteMode.Lax; // allow links from non-domain sites
                        options.LoginPath = "/account/login";
                        options.LogoutPath = "/account/logout";
                        options.Events.OnRedirectToLogin = ReplaceReDirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
                    }
                );
            
            // There is a base-cookie and in index controller there is an method to generate a token that is used to send with the header: X-XSRF-TOKEN
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true; // only used by .NET, there is a separate method to generate a X-XSRF-TOKEN cookie
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
						.WithOrigins("http://localhost:4200",
							"http://localhost:8080")
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials() );
				
				options.AddPolicy("CorsProduction",
					builder => builder
						.AllowCredentials() );
			});
			
			// Detect Application Insights
			services.AddMonitoring(_appSettings);
			
			services.AddMvcCore().AddApiExplorer().AddAuthorization().AddViews();

	        ConfigureForwardedHeaders(services);
	        
			// Application Insights
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			
			new RegisterDependencies().Configure(services);
        }

        /// <summary>
        /// Respect ForwardedHeaders
        /// </summary>
        private void ConfigureForwardedHeaders(IServiceCollection services)
        {
	        // Configure the X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
	        services.Configure<ForwardedHeadersOptions>(options =>
	        {
		        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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
        /// test without: `curl http://localhost:5000/starsky/manifest.json --silent --write-out "%{size_download}\n" --output /dev/null`
        /// test with: `curl http://localhost:5000/starsky/manifest.json --silent
        /// -H "Accept-Encoding: gzip,deflate" --write-out "%{size_download}\n" --output /dev/null`
        /// </summary>
        /// <param name="services"></param>
        private void EnableCompression(IServiceCollection services)
        {
	        services.AddResponseCompression(options =>
	        {
		        options.Providers.Add<GzipCompressionProvider>();
		        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
			        "application/xhtml+xml",
			        "image/svg+xml",
		        });
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
        private static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceReDirector(HttpStatusCode statusCode, 
	        Func<RedirectContext<CookieAuthenticationOptions>, Task> existingReDirector) => context => 
			{
				if ( !context.Request.Path.StartsWithSegments("/api") )
					return existingReDirector(context);
				context.Response.StatusCode = ( int ) statusCode;
				var jsonString = "{\"errors\": [{\"status\": \""+ (int) statusCode + "\" }]}";

				context.Response.ContentType = "application/json";
				var data = Encoding.UTF8.GetBytes(jsonString);
				return context.Response.Body.WriteAsync(data,0, data.Length);
			};

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">ApplicationBuilder</param>
        /// <param name="env">Hosting Env</param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
	        
	        app.UseResponseCompression();

	        if ( env.IsDevelopment()) app.UseDeveloperExceptionPage();
	        app.UseCors(env.IsDevelopment() ? "CorsDevelopment": "CorsProduction");
	        // use ErrorController with Error
	        app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

	        // Enable X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
	        app.UseForwardedHeaders();
	        
	        if ( !env.IsDevelopment() &&  _appSettings.UseHttpsRedirection == true )
	        {
		        app.UseHttpsRedirection();
	        }

            // Use the name of the application to use behind a reverse proxy
            app.UsePathBase( PathHelper.PrefixDbSlash("starsky") );

			app.UseRouting();

	        new SwaggerSetupHelper(_appSettings).Add02AppUseSwaggerAndUi(app);
			
			app.UseContentSecurityPolicy();

			void PrepareResponse(StaticFileResponseContext ctx)
			{
				// Cache static files for 356 days
				ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(365)
					.ToString("R", CultureInfo.InvariantCulture));
				ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
			}
			
	        // Allow Current Directory and wwwroot in Base Directory
	        app.UseStaticFiles(new StaticFileOptions
		        {
			        OnPrepareResponse = PrepareResponse
	        });
    
	        // Use in wwwroot in build directory; the default option assumes Current Directory
	        if ( _appSettings != null && Directory.Exists(Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot")) )
	        {
		        app.UseStaticFiles(new StaticFileOptions
		        {
			        OnPrepareResponse = PrepareResponse,
			        FileProvider = new PhysicalFileProvider(
				        Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot"))
		        });
	        }
			
			if ( _appSettings != null && Directory.Exists(Path.Combine(_appSettings.BaseDirectoryProject, "clientapp", "build", "static")) )
			{
				app.UseStaticFiles(new StaticFileOptions
					{
						OnPrepareResponse = PrepareResponse,
						FileProvider = new PhysicalFileProvider(
							Path.Combine(_appSettings.BaseDirectoryProject, "clientapp", "build", "static")),
						RequestPath = $"/static",
					}
				);
			}

			app.UseAuthentication();
            app.UseBasicAuthentication();

			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
			
			app.UseWebSockets();
			app.MapWebSocketConnections("/realtime", new WebSocketConnectionsOptions(),_appSettings?.UseRealtime);

	        EfCoreMigrationsOnProject(app).ConfigureAwait(false);

	        if ( _appSettings != null && !string.IsNullOrWhiteSpace(_appSettings
		        .ApplicationInsightsInstrumentationKey) )
	        {
		        var configuration = app.ApplicationServices.GetService<TelemetryConfiguration>();
		        configuration.TelemetryProcessorChainBuilder.Use(next => new FilterWebsocketsTelemetryProcessor(next));
		        configuration.TelemetryProcessorChainBuilder.Build();

		        var onStoppedSync = new FlushOnApplicationStopping(app);
		        applicationLifetime?.ApplicationStopping.Register(onStoppedSync.Flush);
	        }
        }


        /// <summary>
        /// Run the latest migration on the database. 
        /// To start over with a SQLite database please remove it and
        /// it will add a new one
        /// </summary>
        private async Task EfCoreMigrationsOnProject(IApplicationBuilder app)
        {
	        using var serviceScope = app.ApplicationServices
		        .GetRequiredService<IServiceScopeFactory>()
		        .CreateScope();
	        await RunMigrations.Run(serviceScope);
        }
    }
}
