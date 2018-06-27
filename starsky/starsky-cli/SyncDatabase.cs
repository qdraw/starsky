using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.Data;
using starsky.Services;
using starsky.Models;

namespace starskyCli
{
    public class SyncDatabase
    {
       
        public SyncDatabase(IMemoryCache memoryCache)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (AppSettingsProvider.DatabaseType)
            {
                case AppSettingsProvider.DatabaseTypeList.mysql:
                    builder.UseMySql(AppSettingsProvider.DbConnectionString);
                    break;
                case AppSettingsProvider.DatabaseTypeList.inmemorydatabase:
                    builder.UseInMemoryDatabase("starsky");
                    break;
                case AppSettingsProvider.DatabaseTypeList.sqlite:
                    builder.UseSqlite(AppSettingsProvider.DbConnectionString);
                    break;
                default:
                    builder.UseSqlite(AppSettingsProvider.DbConnectionString);
                    break;
            }

            var options = builder.Options;

            var context = new ApplicationDbContext(options);
            _query = new Query(context,memoryCache);
            _syncservice = new SyncService(context, _query);
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

        public IEnumerable<FileIndexItem> GetAll(string subPath = "")
        {
            return _query.GetAllFiles(subPath);
        }

    }

}
