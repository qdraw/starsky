using System;
using System.IO;
using starsky.Models;

namespace starsky.Helpers
{
    public static class SqliteHelper
    {
        // Feature to check if a SQLite Database not is locked
        public static bool IsReady()
        {
            if(AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.sqlite) return true; // mysql does not need this
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