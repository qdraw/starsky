using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;

namespace starskytest.Services
{
    [TestClass]
    public class ExifToolTest
    {
        private readonly AppSettings _appSettings;

        public ExifToolTest()
        {
            // Add a dependency injection feature
            var services = new ServiceCollection();
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
        }
        
        [TestMethod]
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

	    [TestMethod]
	    public void ExifToolParseJsonKeyword()
	    {
		    var input = "[{\n\"Keywords\": [\"test\"],\n}]"; // There is an comma inside "test,"

			var output2 = new ExifTool(_appSettings).ParseJson(input);
		    Assert.AreEqual("test", output2.Keywords.FirstOrDefault());

	    }



	}
}
