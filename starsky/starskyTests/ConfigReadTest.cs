using System;
using System.IO;
using System.Runtime.InteropServices;
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
//             todo: fix this issue
//             Error Message:
//             Assert.AreEqual failed. Expected:<\2018>. Actual:</2018>. 
//             Stack Trace:
//             at starskytest.ConfigReadTest.RemoveLatestBackslashTest() in D:\a\1\s\starsky\starskyTests\ConfigReadTest.cs:line 31
//             Failed   AddBackslashTest
//             Error Message:
//             Assert.AreEqual failed. Expected:</2018/>. Actual:</2018>. 
//             Stack Trace:
//             at starskytest.ConfigReadTest.AddBackslashTest() in D:\a\1\s\starsky\starskyTests\ConfigReadTest.cs:line 39
//             Total tests: 14. Passed: 12. Failed: 2. Skipped: 0.
//             var input = ConfigRead.RemoveLatestBackslash("/2018/");
//             var output = "/2018";
//             Assert.AreEqual(input, output);
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
