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
        public ImportDatabase()
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

            var exiftool = new ExifTool();
            var query = new Query(context);
            var isync = new SyncService(context,query);
            _importService = new ImportService(context,isync,exiftool);
        }

        private readonly ImportService _importService;

        public IEnumerable<string> Import(string inputFullPath, bool deleteAfter, bool ageFileFilter, bool recursiveDirectory = false)
        {
            return _importService.Import(inputFullPath, deleteAfter,ageFileFilter,recursiveDirectory);
        }

    }

}
