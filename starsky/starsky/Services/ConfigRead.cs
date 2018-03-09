using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using starsky.Models;

namespace starsky.Services
{
    public class ConfigRead
    {
        public static void SetAppSettingsProvider()
        {
            string basePath;
            string defaultConnection;

            try
            {
                string text = System.IO.File.ReadAllText("../starsky/appsettings.json");

                JObject obj = JObject.Parse(text);

                basePath = (string)obj["ConnectionStrings"]["STARSKY_BASEPATH"];
                defaultConnection = (string)obj["ConnectionStrings"]["DefaultConnection"];

            }
            catch (FileNotFoundException e)
            {
                basePath = null;
                defaultConnection = null;
            }

            if (basePath != null || defaultConnection != null)
            {
                AppSettingsProvider.BasePath = basePath;
                AppSettingsProvider.DbConnectionString = defaultConnection;
                AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";

                //new SettingsCli().BasePath = basePath;
                //new SettingsCli().DefaultConnection = defaultConnection;
                //AppSettingsProvider.BasePath = "Z:\\data\\isight\\2018";
                //AppSettingsProvider.DbConnectionString = "Data Source=../starsky/data.db";
            }
            else
            {
                throw new System.ArgumentException("BasePath or ConnectionStrings not readed");
            }

        }
    }
}
