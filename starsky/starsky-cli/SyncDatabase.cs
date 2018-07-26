using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.Data;
using starsky.Helpers;
using starsky.Services;
using starsky.Models;

namespace starskyCli
{
    public class SyncDatabase
    {
       
        public SyncDatabase()
        {
            var appSettings = new ConfigCliAppsStartupHelper().AppSettings();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (appSettings.DatabaseType)
            {
                case AppSettings.DatabaseTypeList.Mysql:
                    builder.UseMySql(appSettings.DatabaseConnection);
                    break;
                case AppSettings.DatabaseTypeList.InMemoryDatabase:
                    builder.UseInMemoryDatabase("starsky");
                    break;
                case AppSettings.DatabaseTypeList.Sqlite:
                    builder.UseSqlite(appSettings.DatabaseConnection);
                    break;
                default:
                    builder.UseSqlite(appSettings.DatabaseConnection);
                    break;
            }

            var options = builder.Options;

            var context = new ApplicationDbContext(options);
            _query = new Query(context); //without cache
            _syncservice = new SyncService(context, _query, appSettings);
        }

        private readonly Query _query;
        private readonly SyncService _syncservice;

        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            return _syncservice.SyncFiles(subPath);
        }

        public void OrphanFolder(string subPath = "")
        {
            _syncservice.OrphanFolder(subPath);
        }

    }

}
