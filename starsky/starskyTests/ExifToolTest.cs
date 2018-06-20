using System;
using System.Diagnostics;
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
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            
            psi.Arguments = "";
            Process p = Process.Start(psi);
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(strOutput);
            
        }

        
        
    }
}