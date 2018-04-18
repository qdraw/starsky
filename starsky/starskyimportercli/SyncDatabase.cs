using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using starsky.Data;
using starsky.Services;
using starsky.Models;

namespace starskyimportercli
{
    public class SyncDatabase
    {

        public SyncDatabase()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (AppSettingsProvider.DatabaseType == AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                builder.UseMySql(AppSettingsProvider.DbConnectionString);
            }
            else
            {
                builder.UseSqlite(AppSettingsProvider.DbConnectionString);
            }

            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _importIndexItem = new ImportService(context);

        }

        private readonly ImportService _importIndexItem;

//        public IEnumerable<string> SyncFiles(string subPath = "")
//        {
//            return _syncservice.SyncFiles(subPath);
//        }
//
//        public void OrphanFolder(string subPath = "")
//        {
//            _syncservice.OrphanFolder(subPath);
//        }
//
//        public IEnumerable<FileIndexItem> GetAll(string subPath = "")
//        {
//            return _query.GetAllFiles(subPath);
//        }

    }

}
