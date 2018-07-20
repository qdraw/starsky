using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using starsky.Attributes;
using starsky.Helpers;
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
             // depends on platform
             ConfigRead.SetAppSettingsProvider(Path.DirectorySeparatorChar.ToString(), "defaultConnection","mysql","/","/exiftool","false","/yyyy.ext",new List<string>());
             Assert.AreEqual(Path.DirectorySeparatorChar.ToString(),AppSettingsProvider.BasePath);
         }

         [TestMethod]
         public void IsSettingEmptyTest()
         {
             var input = ConfigRead.IsSettingEmpty(string.Empty);
             Assert.AreEqual(input,true);
         }

         [TestMethod]
         public void SetAppSettingsProviderEnvTest()
         {
             // todo: this one
             
         }

         [TestMethod]
         public void ConfigRead_ReadTextFromObjOrEnvListOfItemsWithItems()
         {
             var jObject = JObject.Parse("{ \"ConnectionStrings\": {\"ReadOnlyFolders\":[\"2018\"]   } }");
             var returnlist = ConfigRead.ReadTextFromObjOrEnvListOfItems("ReadOnlyFolders",jObject);
             Assert.AreEqual("2018",returnlist.FirstOrDefault());
         }
         
         [TestMethod]
         public void ConfigRead_ReadTextFromObjOrEnvListOfItems_null_Items()
         {
             var jObject = JObject.Parse("{ \"ConnectionStrings\": {  } }");
             var returnlist = ConfigRead.ReadTextFromObjOrEnvListOfItems("ReadOnlyFolders",jObject,false);
             Assert.AreEqual(null,returnlist.FirstOrDefault());
         }
         
         [TestMethod]
         [ExpectedException(typeof(NullReferenceException))]
         public void ConfigRead_ReadTextFromObjOrEnvListOfItems_nullExpectedException_Items()
         {
             var jObject = JObject.Parse("{ \"ConnectionStrings\": {  } }");
              ConfigRead.ReadTextFromObjOrEnvListOfItems("ReadOnlyFolders",jObject);
              //    ExpectedException > NullReferenceException
         }

         [TestMethod]
         public void ConfigRead_ReadTextFromObjOrEnvListOfItems_zzEnv()
         {
             Environment.SetEnvironmentVariable("ReadOnlyFolders","[\"2018\"]");
             var returnlist = ConfigRead.ReadTextFromObjOrEnvListOfItems("ReadOnlyFolders");
             Assert.AreEqual("2018",returnlist.FirstOrDefault());
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void RemoveLatestBackslashTest()
         {
             var input = ConfigRead.RemoveLatestBackslash("/2018"+ Path.DirectorySeparatorChar.ToString());
             var output = "/2018";
             Assert.AreEqual(input, output);
         }

         [ExcludeFromCoverage]
         [TestMethod]
         public void PrefixDbslashTest()
         {
             var input = ConfigRead.PrefixDbSlash("2018/");
             var output = "/2018/";
             Assert.AreEqual(input, output);
         }
         
         [ExcludeFromCoverage]
         [TestMethod]
         public void AddBackslashTest()
         {
             var input = ConfigRead.AddBackslash("2018");
             var output = "2018" + Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
             
             input = ConfigRead.AddBackslash("2018" + Path.DirectorySeparatorChar.ToString());
             output = "2018"+ Path.DirectorySeparatorChar.ToString();
             Assert.AreEqual(input, output);
         }
     }
 }
