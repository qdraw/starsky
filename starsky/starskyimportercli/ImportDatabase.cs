using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.Data;
using starsky.Services;
using starsky.Models;

namespace starskyimportercli
{
    public class ImportDatabase
    {
        public ImportDatabase(IMemoryCache memoryCache)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (AppSettingsProvider.DatabaseType == AppSettingsProvider.DatabaseTypeList.mysql)
            {
                builder.UseMySql(AppSettingsProvider.DbConnectionString);
            }
            else
            {
                builder.UseSqlite(AppSettingsProvider.DbConnectionString);
            }

            var options = builder.Options;
            var context = new ApplicationDbContext(options);

            var query = new Query(context,memoryCache);
            var isync = new SyncService(context,query);
            _importService = new ImportService(context,isync);
        }

        private readonly ImportService _importService;

        public IEnumerable<string> Import(string inputFullPath, bool deleteAfter, bool ageFileFilter)
        {
            return _importService.Import(inputFullPath, deleteAfter,ageFileFilter);
        }

    }

}
