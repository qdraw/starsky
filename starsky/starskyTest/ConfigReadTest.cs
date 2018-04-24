using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;

namespace starskyTest
 {
     [TestClass]
     public class ConfigReadTest
     {
         
         [TestMethod]
         public void BasePathTest()
         {
             ConfigRead.SetAppSettingsProvider("/","defaultConnection","mysql","/","/exiftool");
             Assert.AreEqual("/",AppSettingsProvider.BasePath);
         }

         [TestMethod]
         public void RemoveLatestBackslashTest()
         {
             var input = ConfigRead.RemoveLatestBackslash("/2018/");
             var output = "/2018";
             Assert.AreEqual(input, output);
         }
         
         public void AddBackslashTest()
         {
             var input = ConfigRead.RemoveLatestBackslash("/2018/");
             var output = "/2018";
             Assert.AreEqual(input, output);
         }       
         
     }
 }