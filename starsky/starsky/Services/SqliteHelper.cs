using System;
using System.IO;
using System.Reflection;
using starsky.Models;

namespace starsky.Services
{
    public static class SqliteHelper
    {
        
        public static bool IsReady()
        {
            if(AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.Sqlite) return true; // mysql does not need this
            //     "DefaultConnection": "Data Source=data.db",
            // locked db:  data.db-journal

            var fullDbPath = AppSettingsProvider.DbConnectionString.Replace("Data Source=","");
            
            if (!File.Exists(fullDbPath))
            {
                Console.WriteLine("Database does not exist"); 
                throw new ArgumentException("Database does not exist");
            }

            if (!File.Exists(fullDbPath + "-journal"))
            {
                return true; // Good to go!
            }

            var journalCreationTime = File.GetCreationTime(fullDbPath+ "-journal");

            TimeSpan time = DateTime.Now - journalCreationTime;
            
            var startTime = DateTime.Now;
            int i = 0;
            while (i < 10000000)
            {
                if (!File.Exists(fullDbPath + "-journal"))
                {
                    return true;
                }

                Console.Write(".");

                if ((DateTime.Now - startTime) >= new TimeSpan(0, 0, 0, 10, 0))
                {
                    throw new TimeoutException("waiting for SQLite Database for 10 seconds");
                }

                i++;
            }
            return true;

        }
    }
}