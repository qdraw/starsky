using starsky.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.Data;
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
