using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

            var query = new Query(context);
            var isync = new SyncService(context,query);
            _importService = new ImportService(context,isync);

        }

        private readonly ImportService _importService;

        public IEnumerable<string> ImportFile(string inputFileFullPath)
        {
            return _importService.ImportFile(inputFileFullPath);
        }


    }

}
