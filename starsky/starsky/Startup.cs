using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using starsky.Data;
using starsky.Models;
using starsky.Services;

namespace starsky
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigRead.SetAppSettingsProvider();

            if (AppSettingsProvider.DatabaseType == AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(AppSettingsProvider.DbConnectionString));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(AppSettingsProvider.DbConnectionString));
            }

            services.AddScoped<IQuery, Query>();
            services.AddScoped<ISync, SyncService>();

            services.AddScoped<ISearch, SearchService>();

            

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePathBase("/starsky");

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Shared/Error");
            }


            app.UseStaticFiles();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
