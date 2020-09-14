using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.sockets.Helpers;
using starsky.foundation.sockets.Middleware;
using starsky.Health;
using starsky.Helpers;
using starskycore.Middleware;

namespace starsky
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private AppSettings _appSettings;

		public Startup()
		{
			_configuration = SetupAppSettings.AppSettingsToBuilder();
		}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        _appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, _configuration);

	        // before anything else
			EnableCompression(services);
	        
            services.AddMemoryCache();
            // this is ignored here: appSettings.AddMemoryCache; but implemented in cache
                 
            // Enable .NET CORE health checks
            services.AddHealthChecks()
	            .AddDbContextCheck<ApplicationDbContext>()
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.StorageFolder,
				            diskOptions);
		            },
		            name: "Storage_StorageFolder")
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.ThumbnailTempFolder,
				            diskOptions);
		            },
		            name: "Storage_ThumbnailTempFolder")
	            .AddDiskStorageHealthCheck(
		            setup: diskOptions =>
		            {
			            new DiskOptionsPercentageSetup().Setup(_appSettings.TempFolder,
				            diskOptions);
		            },
		            name: "Storage_TempFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.StorageFolder),
		            name: "Exist_StorageFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.TempFolder),
		            name: "Exist_TempFolder")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.ExifToolPath),
		            name: "Exist_ExifToolPath")
	            .AddPathExistHealthCheck(
		            setup: pathOptions => pathOptions.AddPath(_appSettings.ThumbnailTempFolder),
		            name: "Exist_ThumbnailTempFolder")
	            .AddCheck<DateAssemblyHealthCheck>("DateAssemblyHealthCheck");
            
            var healthSqlQuery = "SELECT * FROM `__EFMigrationsHistory` WHERE ProductVersion > 9";
            var foundationDatabaseName = typeof(ApplicationDbContext).Assembly.FullName.Split(",").FirstOrDefault();

            switch (_appSettings.DatabaseType)
            {
                case (AppSettings.DatabaseTypeList.Mysql):
                    services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(_appSettings.DatabaseConnection, 
	                    b => b.MigrationsAssembly(foundationDatabaseName)));
                    services.AddHealthChecks().AddMySql(_appSettings.DatabaseConnection);
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection, 
	                    b => b.MigrationsAssembly(foundationDatabaseName)));
                    services.AddHealthChecks().AddSqlite(_appSettings.DatabaseConnection, healthSqlQuery, "sqlite");
                    break;
            }
            
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
                        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; // allow links from non-domain sites
                        options.LoginPath = "/account/login";
                        options.LogoutPath = "/account/logout";
                        options.Events.OnRedirectToLogin = ReplaceRedirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
                    }
                );
            
            // There is a base-cookie and in index controller there is an method to generate a token that is used to send with the header: X-XSRF-TOKEN
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true; // only used by .NET, there is a separate method to generate a X-XSRF-TOKEN cookie
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
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
			
#if SYSTEM_TEXT_ENABLED
			// NET Core 3 -> removed newtonsoft from core
			services.AddMvcCore().AddApiExplorer().AddAuthorization().AddViews();
#else
	        services.AddMvcCore().AddApiExplorer().AddAuthorization().AddViews().AddNewtonsoftJson();
#endif
	        
			// Configure the X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			});
	        
			// Application Insights
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			
			// Detect Application Insights
			if ( !string.IsNullOrWhiteSpace(_appSettings.ApplicationInsightsInstrumentationKey) )
			{
				services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
				{
					ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
					EnableDependencyTrackingTelemetryModule = true,
					EnableHeartbeat = true,
					EnableAuthenticationTrackingJavaScript = true,
					InstrumentationKey = _appSettings.ApplicationInsightsInstrumentationKey
				});
			}

			new RegisterDependencies().Configure(services);

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
        /// <param name="existingRedirector">func of RedirectContext</param>
        /// <returns></returns>
        static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(HttpStatusCode statusCode, 
	        Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector) => 
	        context => 
			{
				if ( !context.Request.Path.StartsWithSegments("/api") )
					return existingRedirector(context);
				context.Response.StatusCode = ( int ) statusCode;
				// used to fetch in the process to catch
				context.Response.Headers["X-Status"] = new StringValues((( int ) statusCode).ToString());
				return Task.CompletedTask;
			};

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">ApplicationBuilder</param>
        /// <param name="env">Hosting Env</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
	        app.UseResponseCompression();

	        if ( env.IsDevelopment())
	        {
		        app.UseDeveloperExceptionPage();

		        // Allow in dev to use localhost services
		        app.UseCors("CorsDevelopment");
	        }
	        else
	        {
		        app.UseCors("CorsProduction");   
		        app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
	        }
	        
			// Enable X-Forwarded-For and X-Forwarded-Proto to use for example an NgInx reverse proxy
			app.UseForwardedHeaders();
	        
            // Use the name of the application to use behind a reverse proxy
            app.UsePathBase( PathHelper.PrefixDbSlash("starsky") );

#if NETCOREAPP3_0 || NETCOREAPP3_1
			app.UseRouting();
#endif

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
	        if ( Directory.Exists(Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot")) )
	        {
		        app.UseStaticFiles(new StaticFileOptions
		        {
			        OnPrepareResponse = PrepareResponse,
			        FileProvider = new PhysicalFileProvider(
				        Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot"))
		        });
	        }
			
			if ( Directory.Exists(Path.Combine(env.ContentRootPath, "clientapp", "build", "static")) )
			{
				app.UseStaticFiles(new StaticFileOptions
					{
						OnPrepareResponse = PrepareResponse,
						FileProvider = new PhysicalFileProvider(
							Path.Combine(env.ContentRootPath, "clientapp", "build", "static")),
						RequestPath = $"/static",
					}
				);
			}

			app.UseAuthentication();
            app.UseBasicAuthentication();

#if NETCOREAPP3_0 || NETCOREAPP3_1
			app.UseAuthorization();
#endif

	        // Enable websockets
	        app.UseWebSockets(DefaultWebSocketOptions.GetDefault());
	        app.UseCustomWebSocketManager();
	        
#if NETCOREAPP3_0 || NETCOREAPP3_1
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
			});
#else
	        app.UseMvc(routes =>
					 {
						 routes.MapRoute(
							 name: "default",
							 template: "{controller=Home}/{action=Index}/{id?}");
					 });
#endif

	        EfCoreMigrationsOnProject(app);

        }


        /// <summary>
        /// Run the latest migration on the database. 
        /// To start over with a SQLite database please remove it and
        /// it will add a new one
        /// </summary>
        private void EfCoreMigrationsOnProject(IApplicationBuilder app)
        {
	        try
	        {
		        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
			        .CreateScope();
		        var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
		        dbContext.Database.Migrate();
	        }
	        catch (MySql.Data.MySqlClient.MySqlException e)
	        {
		        Console.WriteLine(e);
	        }
        }
    }
}
