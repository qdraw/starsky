using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

            if (AppSettingsProvider.DatabaseType == AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                builder.UseMySql(AppSettingsProvider.DbConnectionString);
            }
            else
            {
                builder.UseSqlite(AppSettingsProvider.DbConnectionString);
            }

            Console.WriteLine(AppSettingsProvider.DbConnectionString);

            var _options = builder.Options;

            _context = new ApplicationDbContext(_options);
            _query = new Query(_context);
            _syncservice = new SyncService(_context, _query);

        }

        private readonly ApplicationDbContext _context;
        private readonly Query _query;
        private readonly SyncService _syncservice;


        public IEnumerable<string> SyncFiles(string subPath = "")
        {
            return _syncservice.SyncFiles(subPath);
        }

        public IEnumerable<FileIndexItem> GetAll(string subPath = "")
        {
            return _query.GetAllFiles(subPath);
        }

    }

}
