using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace starsky.Models
{
    public static class AppSettingsProvider
    {
        public static string DbConnectionString { get; set; }
        public static string BasePath { get; set; }
    }
}
