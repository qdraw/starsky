using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ExifToolTest
    {
        [TestMethod]
        [ExcludeFromCoverage]
        public void  ExifToolFixTestIgnoreStringTest()
        {
            var input = "{\n\"Keywords\": [\"test\",\"word2\"], \n}"; // CamelCase!
            var output = ExifTool.FixingJsonKeywordString(input);
            Assert.AreEqual(input+"\n",output);
        }
        
        [TestMethod]
        public void ExifToolFixTestSingleWord()
        {
            var expetectedOutput = "{\n\"Keywords\": [\"test,\"],\n}\n"; // There is an comma inside "test,"
            var input2 = "{\n\"Keywords\": \"test\", \n}"; 
            var output2 = ExifTool.FixingJsonKeywordString(input2);
            Assert.AreEqual(expetectedOutput,output2);   
        }

        [TestMethod]
        public void ExifToolFakeApiTest()
        {
            var solutionDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var solutionTestsDir = solutionDir.Replace("starskyTests", "starskyTestsExifTool");

            var starskyTestsExifTool =
                Path.Combine(solutionTestsDir, "Debug", "netcoreapp2.0", "starskyTestsExifTool.dll");

            if (!File.Exists(starskyTestsExifTool))
                Path.Combine(solutionTestsDir, "Release", "netcoreapp2.0", "starskyTestsExifTool.dll");
            
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            psi.Arguments = starskyTestsExifTool;
            Process p = Process.Start(psi);
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(strOutput);
            Assert.AreEqual("Hello World!",strOutput.Trim());

        }

        
        
    }
}