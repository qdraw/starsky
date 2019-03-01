using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskycore.Helpers;

namespace starskytest.Services
 {
     [TestClass]
     public class ConfigReadTest
     {

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_RemoveLatestBackslashTest()
         {
             var input = PathHelper.RemoveLatestBackslash("/2018"+ Path.DirectorySeparatorChar.ToString());
             var output = "/2018";
             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_PrefixDbslashTest()
         {
             var input = PathHelper.PrefixDbSlash("2018/");
             var output = "/2018/";
             Assert.AreEqual(input, output);
         }
         
         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_AddBackslashTest()
         {
             var input = PathHelper.AddBackslash("2018");
             var output = "2018" + Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
             
             input = PathHelper.AddBackslash("2018" + Path.DirectorySeparatorChar.ToString());
             output = "2018"+ Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_RemovePrefixDbSlashTest()
         {
             Assert.AreEqual("2018",PathHelper.RemovePrefixDbSlash("/2018"));
         }
        
     }
 }
