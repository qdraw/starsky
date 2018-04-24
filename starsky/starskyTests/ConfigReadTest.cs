using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;

namespace starskytest
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
             var input = ConfigRead.RemoveLatestBackslash("/2018" + Path.DirectorySeparatorChar);
             var output = "/2018";
             Assert.AreEqual(input, output);
         }

         [TestMethod]
         public void AddBackslashTest()
         {
             var input = ConfigRead.RemoveLatestBackslash("/2018/");
             var output = "/2018";
             Assert.AreEqual(input, output);
         }

         [TestMethod]
         public void PrefixBackslashTest()
         {
             var input = ConfigRead.PrefixBackslash("2018/");
             var output = "/2018/";
             Assert.AreEqual(input, output);
         }


     }
 }
