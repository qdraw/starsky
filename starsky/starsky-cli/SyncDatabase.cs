using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using starsky;
using starsky.Data;
using starsky.Services;
using starsky.Models;



namespace starskyCli
{
    public class SyncDatabase
    {

        public SyncDatabase()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            //AppSettingsProvider.BasePath = Startup._getBasePath();
            //AppSettingsProvider.DbConnectionString = Startup.GetConnectionString();

            //AppSettingsProvider.BasePath = "Z:\\data\\isight\\2018";
            //AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";


            if (AppSettingsProvider.DatabaseType == AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                builder.UseMySql(AppSettingsProvider.DbConnectionString);
            }
            else
            {
                builder.UseSqlite(AppSettingsProvider.DbConnectionString);
            }

            Console.WriteLine(AppSettingsProvider.DbConnectionString);
            //builder.UseMySql(AppSettingsProvider.DbConnectionString);

            //builder.UseSqlite(AppSettingsProvider.DbConnectionString);
            //builder.UseInMemoryDatabase();
            var _options = builder.Options;

            _context = new ApplicationDbContext(_options);
            _sqlStatus = new SqlUpdateStatus(_context);
        }

        private readonly ApplicationDbContext _context;
        private readonly SqlUpdateStatus _sqlStatus;

        //public IEnumerable<string> GetAll()
        //{
        //   return _sqlStatus.GetAll();
        //}

        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            return _sqlStatus.SyncFiles(subPath);
        }

        public IEnumerable<FileIndexItem> GetAll(string subPath = "")
        {
            return _sqlStatus.GetAll(subPath);
        }

    }

}
