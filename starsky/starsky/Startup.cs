using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
                    }
                );

            services.Configure<ForwardedHeadersOptions>(options =>
            {
				options.ForwardedHeaders =  ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
           
            services.AddScoped<IQuery, Query>();
            services.AddScoped<ISync, SyncService>();
            services.AddScoped<ISearch, SearchService>();
            services.AddScoped<IImport, ImportService>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IExiftool, ExifTool>();
            services.AddScoped<IReadMeta, ReadMeta>();

	        
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

	        services.AddMvc()
	            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
	        
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


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
	        
	        // Enable X-Forwarded-For and X-Forwarded-Proto to use for example an nginx reverse proxy
	        app.UseForwardedHeaders();
	        
            // Use the name of the application to use behind a reverse proxy
            app.UsePathBase(PathHelper.PrefixDbSlash(_appSettings.Name.ToLowerInvariant()) );
            
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Home/Error");
            }
	        
	        new SwaggerHelper(_appSettings).Add02AppUseSwaggerAndUi(app);

	        new SwaggerHelper(_appSettings).Add03AppExport(app);

	        app.UseContentSecurityPolicy();

	        // the Current Directory wwwroot directory
	        app.UseStaticFiles();
	        
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


			app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
	        
	        

            // Run the latest migration on the database. 
            // To startover with a sqlite database please remove it and
            // it will add a new one
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                        .Database.Migrate();
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                Console.WriteLine(e);
            }


        }
	    

    }
}
