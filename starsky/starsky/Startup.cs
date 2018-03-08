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

        private string GetConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("STARSKY_SQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine(">> connectionString from .json file");

                connectionString = _configuration.GetConnectionString("DefaultConnection");
            }
            return connectionString;
        }

        private string _getBasePath()
        {
            var connectionString = Environment.GetEnvironmentVariable("STARSKY_BASEPATH");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration.GetConnectionString("STARSKY_BASEPATH");
            }
            return connectionString;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(GetConnectionString()));
            services.AddScoped<IUpdate, SqlUpdateStatus>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            AppSettingsProvider.BasePath = _getBasePath();
            AppSettingsProvider.DbConnectionString = GetConnectionString();

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
