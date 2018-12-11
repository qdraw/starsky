using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using starsky.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Data;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using Microsoft.Extensions.Hosting;

using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace starsky
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private AppSettings _appSettings;

        public Startup()
        {
            // new style config
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",true)
                .AddEnvironmentVariables();
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
            
            // services.AddResponseCaching();
            
            switch (_appSettings.DatabaseType)
            {
                case (AppSettings.DatabaseTypeList.Mysql):
                    services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(_appSettings.DatabaseConnection));
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection));
                    break;
                default:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_appSettings.DatabaseConnection));
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

	        

	        
	        services.AddSwaggerGen(c =>
	        {
		        c.SwaggerDoc(_appSettings.Name, new Info { Title = _appSettings.Name, Version = "v1" });
		        
		        c.AddSecurityDefinition("basic", new BasicAuthScheme {Type = "basic", Description = "basic authentication" }); 
		        c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "basic", new string[] { } },});
		        
		        c.IncludeXmlComments(GetXmlCommentsPath());
		        c.DescribeAllEnumsAsStrings();
		        c.DocumentFilter<BasicAuthFilter>();
	        }); 

	        services.AddMvc()
	            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
	        
	        // Application Insights
	        var appInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
	        if(!string.IsNullOrWhiteSpace(appInsightsKey)) services.AddApplicationInsightsTelemetry();
        }
	    
	    public class BasicAuthFilter : IDocumentFilter
	    {
		    public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
		    {
			    var securityRequirements = new Dictionary<string, IEnumerable<string>>()
			    {
				    { "basic", new string[] { } }
			    };

			    swaggerDoc.Security = new IDictionary<string, IEnumerable<string>>[] { securityRequirements };
		    }
	    }

	    
	    private string GetXmlCommentsPath()
	    {
		    var app = AppContext.BaseDirectory;
		    return Path.Combine(app, "starsky.xml");
	    }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            // Use the name of the application to use behind a reverse proxy
            app.UsePathBase(ConfigRead.PrefixDbSlash(_appSettings.Name.ToLowerInvariant()) );
            
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
	        
	        // Use swagger in development Environment or when env variable SWAGGER is enabled
			var swaggerKey = Environment.GetEnvironmentVariable("SWAGGER")?.ToLower();
			if ( swaggerKey == "true" || env.IsDevelopment() )
			{
				app.UseSwagger(); // registers the two documents in separate routes
				
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/"+ _appSettings.Name + "/swagger.json", _appSettings.Name);
					options.OAuthAppName(_appSettings.Name + " - Swagger");					
//					options.InjectJavascript("/js/vendor/SwashbuckleCustomAuth.CustomContent.basic-auth.js"); 
				}); // makes the ui visible    
			}
	        
            // Use in wwwroot
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseBasicAuthentication();

			app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
	        

	        
	        // Add Content Security Policy/CSP
	        app.Use(async (ctx, next) =>
	        {
		        ctx.Response.Headers
			        .Add("Content-Security-Policy",
				        "default-src 'self'; img-src 'self' https://*.tile.openstreetmap.org; script-src 'self';");
		        await next();
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
