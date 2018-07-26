using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ExifToolTest
    {
        private readonly AppSettings _appSettings;

        public ExifToolTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            var dict = new Dictionary<string, string>
            {
                {"App:MainWindow:Height", "11"},
            };
            var builder = new ConfigurationBuilder();                
            builder.AddInMemoryCollection(dict);
            var configuration = builder.Build();
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            var serviceProvider = services.BuildServiceProvider();
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
        }
        [TestMethod]
        [ExcludeFromCoverage]
        public void  ExifToolFixTestIgnoreStringTest()
        {
            var input = "{\n\"Keywords\": [\"test\",\"word2\"], \n}"; // CamelCase!
            var output = new ExifTool(_appSettings).FixingJsonKeywordString(input);
            Assert.AreEqual(input+"\n",output);
        }
        
        [TestMethod]
        public void ExifToolFixTestSingleWord()
        {
            var expetectedOutput = "{\n\"Keywords\": [\"test,\"],\n}\n"; // There is an comma inside "test,"
            var input2 = "{\n\"Keywords\": \"test\", \n}"; 
            var output2 = new ExifTool(_appSettings).FixingJsonKeywordString(input2);
            Assert.AreEqual(expetectedOutput,output2);   
        }
        
    }
}