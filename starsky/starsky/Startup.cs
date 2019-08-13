using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using starsky.Helpers;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Query = starskycore.Services.Query;
using SyncService = starskycore.Services.SyncService;


namespace starsky
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private AppSettings _appSettings;

		public Startup()
		{
			var builder = ConfigCliAppsStartupHelper.AppSettingsToBuilder();
			_configuration = builder.Build();
		}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // configs
            services.ConfigurePoco<AppSettings>(_configuration.GetSection("App"));
            var serviceProvider = services.BuildServiceProvider();
            
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();

            services.AddMemoryCache();
            // this is ignored here: appSettings.AddMemoryCache; but implemented in cache
                        
            switch (_appSettings.DatabaseType)
            {
                case (AppSettings.DatabaseTypeList.Mysql):
                    services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(_appSettings.DatabaseConnection, b => b.MigrationsAssembly(nameof(starskycore))));
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection, b => b.MigrationsAssembly(nameof(starskycore))));
                    break;
                default:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection, b => b.MigrationsAssembly(nameof(starskycore))));
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
                        options.ExpireTimeSpan = TimeSpan.FromDays(365);
                        options.SlidingExpiration = true;
                        options.LoginPath = "/account/login";
                        options.LogoutPath = "/account/logout";
                        options.Events.OnRedirectToLogin = ReplaceRedirector(HttpStatusCode.Unauthorized, options.Events.OnRedirectToLogin);
                    }
                );

            services.AddScoped<IQuery, Query>();
            services.AddScoped<ISync, SyncService>();
            services.AddScoped<ISearch, SearchService>();
            services.AddScoped<ISearchSuggest, SearchSuggestionsService>();

            services.AddScoped<IImport, ImportService>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IExifTool, ExifTool>();
            services.AddScoped<IReadMeta, ReadMeta>();
	        services.AddScoped<IStorage, StorageSubPathFilesystem>();

	        
            // AddHostedService in .NET Core 2.1 / background service
            services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

	        
			// to add support for swagger
			new SwaggerHelper(_appSettings).Add01SwaggerGenHelper(services);

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
			});
			
			// Cache the response at the browser
			services.AddResponseCaching();

			// NET Core 3 -> removed newtonsoft from core
#if NETCOREAPP3_0
			services.AddMvc();
				//.AddNewtonsoftJson();
#else
	        services.AddMvc();
#endif

			// Configure the X-Forwarded-For and X-Forwarded-Proto to use for example an nginx reverse proxy
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			});
	        
	        // Detect ip in code
	        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
	        
	        // Application Insights
	        var appInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
	        if ( !string.IsNullOrWhiteSpace(appInsightsKey) ) services.AddApplicationInsightsTelemetry();
	        services.AddScoped<ApplicationInsightsJsHelper>();

	        // For the import service
	        services.AddSingleton<IHttpProvider,HttpProvider>();
	        services.AddSingleton<HttpClientHelper>();
	        services.AddSingleton<System.Net.Http.HttpClient>();
	       
        }

        /// <summary>
        /// Does the current user get a redirect or 401 page
        /// </summary>
        /// <param name="statusCode">current status code</param>
        /// <param name="existingRedirector">func of RedirectContext</param>
        /// <returns></returns>
        static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(HttpStatusCode statusCode, 
	        Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector) =>
	        context => {
		        if ( !context.Request.Path.StartsWithSegments("/api") )
			        return existingRedirector(context);
		        context.Response.StatusCode = (int)statusCode;
		        return Task.CompletedTask;
	        };

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">ApplicationBuilder</param>
        /// <param name="env">Hosting Env</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			// Enable X-Forwarded-For and X-Forwarded-Proto to use for example an nginx reverse proxy
			app.UseForwardedHeaders();
	        
            // Use the name of the application to use behind a reverse proxy
            app.UsePathBase( PathHelper.PrefixDbSlash(_appSettings.Name.ToLowerInvariant()) );

#if NETCOREAPP3_0
			app.UseRouting();
#endif

			if ( env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
                
                // Allow in dev to use localhost services
                app.UseCors("CorsDevelopment");
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Home/Error");
            }

			// need rewrite for netcore3
			new SwaggerHelper(_appSettings).Add02AppUseSwaggerAndUi(app);
			new SwaggerHelper(_appSettings).Add03AppExport(app);

			app.UseContentSecurityPolicy();
	        
	        // Allow Current Directory and wwwroot in Base Directory
	        app.UseStaticFiles(new StaticFileOptions
		        {
			        OnPrepareResponse = ctx =>
			        {
				        const int durationInSeconds = 60 * 60 * 24;
				        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
					        "public,max-age=" + durationInSeconds;
			        }
	        });

	        // Use in wwwroot in build directory; the default option assumes Current Directory
	        if ( Directory.Exists(Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot")) )
	        {
		        app.UseStaticFiles(new StaticFileOptions
		        {
			        FileProvider = new PhysicalFileProvider(
				        Path.Combine(_appSettings.BaseDirectoryProject, "wwwroot"))
		        });
	        }

			app.UseAuthentication();
            app.UseBasicAuthentication();

#if NETCOREAPP3_0
			app.UseAuthorization();
#endif

#if NETCOREAPP3_0
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

			// Run the latest migration on the database. 
			// To startover with a sqlite database please remove it and
			// it will add a new one
			try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    var dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                    dbContext.Database.Migrate();

                }
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                Console.WriteLine(e);
            }

            if ( _appSettings.AddSinglePageApplicationFallback && File.Exists(Path.Combine(env.WebRootPath,"index.html")))
            {
	            // handle client side routes
	            app.Run( async (context) =>
	            {
		            context.Response.ContentType = "text/html";
		            await context.Response.SendFileAsync(Path.Combine(env.WebRootPath,"index.html"));
	            });
            }


        }
	    

    }
}
