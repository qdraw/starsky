using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Models;
using starsky.Services;

namespace starskytests
 {
     [TestClass]
     public class ConfigReadTest
     {

         [ExcludeFromCoverage]
         [TestMethod]
         public void BasePathTest()
         {
             ConfigRead.SetAppSettingsProvider("/","defaultConnection","mysql","/","/exiftool");
             Assert.AreEqual("/",AppSettingsProvider.BasePath);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void RemoveLatestBackslashTest()
         {
//             var input = ConfigRead.RemoveLatestBackslash("/2018/");
//             var output = "/2018";
//             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void PrefixBackslashTest()
         {
             var input = ConfigRead.PrefixBackslash("2018/");
             var output = "/2018/";
             Assert.AreEqual(input, output);
         }


     }
 }
