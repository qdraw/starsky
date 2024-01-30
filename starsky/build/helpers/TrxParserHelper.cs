using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Serilog;

namespace helpers;

public static class TrxParserHelper
{
	public static void DisplayFileTests(string trxFullFilePath)
	{
		if ( ! File.Exists(trxFullFilePath) )
		{
			return;
		}
		
		var xmlString = File.ReadAllText(trxFullFilePath);
		
		var xmlDoc = XDocument.Parse(xmlString);
		// Needs to be XNamespace otherwise it fails on :
		XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

		var resultsElement = xmlDoc.Descendants(ns + "Results").FirstOrDefault();

		var unitTestResultsList = resultsElement?.Elements(ns + "UnitTestResult").ToList();
		if ( unitTestResultsList == null)
		{
			return;
		}
		
		var results = new List<Tuple<string, string, string>>();
		foreach ( var unitTestResult in unitTestResultsList )
		{
			if ( unitTestResult.Attribute("outcome")?.Value != "Failed" )
			{
				continue;
			}
			var testName = unitTestResult.Attribute("testName")?.Value;
			var message = unitTestResult.Element(ns + "Output")
				?.Element(ns + "ErrorInfo")?.Element(ns + "Message")?.Value;
					
			var stackTrace =unitTestResult.Element(ns + "Output")
				?.Element(ns + "ErrorInfo")?.Element(ns + "StackTrace")?.Value;
			
			results.Add(new Tuple<string, string, string>(testName, message, stackTrace));
		}

		if ( results.Count == 0 )
		{
			return;
		}
		
		Log.Error("\nFailed tests:\n");
		
		foreach ( var result in results )
		{
			Log.Error($"Test Name: {result.Item1}");
			Log.Error($"Message: {result.Item2}");
			Log.Error($"Stack Trace: {result.Item3}");
			Log.Error("------------------------");
		}
	}
}
