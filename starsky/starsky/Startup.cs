using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using starsky.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigRead.SetAppSettingsProvider();
            
            if(AppSettingsProvider.AddMemoryCache) services.AddMemoryCache();
            
            services.AddResponseCaching();
            
            switch (AppSettingsProvider.DatabaseType)
            {
                case AppSettingsProvider.DatabaseTypeList.mysql:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(AppSettingsProvider.DbConnectionString));
                    break;
                case AppSettingsProvider.DatabaseTypeList.inmemorydatabase:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"));
                    break;
                case AppSettingsProvider.DatabaseTypeList.sqlite:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(AppSettingsProvider.DbConnectionString));
                    break;
                default:
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(AppSettingsProvider.DbConnectionString));
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
                        options.Cookie.Expiration = TimeSpan.FromDays(7);
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
