using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using starsky.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using starsky.Data;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starsky
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private ServiceProvider _serviceProvider;

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
            _serviceProvider = services.BuildServiceProvider();
            
            var appSettings = _serviceProvider.GetRequiredService<AppSettings>();

            services.AddMemoryCache();
            // this is ignored here: appSettings.AddMemoryCache; but implemented in cache
            
            services.AddResponseCaching();
            
            switch (appSettings.DatabaseType)
            {
                case (AppSettings.DatabaseTypeList.Mysql):
                    services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(appSettings.DatabaseConnection));
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(appSettings.DatabaseConnection));
                    break;
                default:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(appSettings.DatabaseConnection));
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
                        options.Cookie.Expiration = TimeSpan.FromDays(200);
                        options.SlidingExpiration = true;
                    }
                );

           
            services.AddScoped<IQuery, Query>();
            services.AddScoped<ISync, SyncService>();
            services.AddScoped<ISearch, SearchService>();
            services.AddScoped<IImport, ImportService>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IExiftool, ExifTool>();
            
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCaching();

            app.UsePathBase("/starsky");
            
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

            app.UseStaticFiles();

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
