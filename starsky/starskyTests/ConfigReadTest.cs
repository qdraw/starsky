using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Services;

namespace starskytests
 {
     [TestClass]
     public class ConfigReadTest
     {

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_RemoveLatestBackslashTest()
         {
             var input = ConfigRead.RemoveLatestBackslash("/2018"+ Path.DirectorySeparatorChar.ToString());
             var output = "/2018";
             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_PrefixDbslashTest()
         {
             var input = ConfigRead.PrefixDbSlash("2018/");
             var output = "/2018/";
             Assert.AreEqual(input, output);
         }
         
         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_AddBackslashTest()
         {
             var input = ConfigRead.AddBackslash("2018");
             var output = "2018" + Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
             
             input = ConfigRead.AddBackslash("2018" + Path.DirectorySeparatorChar.ToString());
             output = "2018"+ Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void ConfigRead_RemovePrefixDbSlashTest()
         {
             Assert.AreEqual("2018",ConfigRead.RemovePrefixDbSlash("/2018"));
         }
        
     }
 }
