using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Middleware;
using starsky.Models;

namespace starskytests
{
    [TestClass]
    public class Class1
    {
        private readonly IServiceProvider _serviceProvider;

        public Class1()
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            
//            // Depencency Injection for configuration
//            var services = new ServiceCollection();
//            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
//
//            var builder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .AddEnvironmentVariables();
//            var configuration = builder.Build();
//            services.ConfigurePOCO<AppSettings>(configuration.GetSection("App"));
//            _serviceProvider = services.BuildServiceProvider();
//            // End of Depencency Injection for configuration

        }

//        [TestMethod]
//        public void ConfigRead_PrefixDbslashTest()
//        {
//            var settings = _serviceProvider.GetRequiredService<AppSettings>();
//        }
    }
}

