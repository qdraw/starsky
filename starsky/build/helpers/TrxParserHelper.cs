using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Serilog;

namespace helpers;

public static class TrxParserHelper
{
	/// <summary>
	/// Needs to be XNamespace otherwise it fails on ':' in the name
	/// Needs to be http://microsoft.com/schemas/VisualStudio/TeamTest/2010
	/// </summary>
	[SuppressMessage("Usage",
		"S5332:Using http protocol is insecure. Use https instead")]
	static readonly XNamespace TeamTestNamespace =
		"http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

	public static void DisplayFileTests(string trxFullFilePath)
	{
		if ( !File.Exists(trxFullFilePath) )
		{
			return;
		}

		var xmlString = File.ReadAllText(trxFullFilePath);

		var xmlDoc = XDocument.Parse(xmlString);

		var resultsElement = xmlDoc.Descendants(TeamTestNamespace + "Results")
			.FirstOrDefault();

		var unitTestResultsList = resultsElement
			?.Elements(TeamTestNamespace + "UnitTestResult").ToList();
		if ( unitTestResultsList == null )
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
			var message = unitTestResult.Element(TeamTestNamespace + "Output")
				?.Element(TeamTestNamespace + "ErrorInfo")
				?.Element(TeamTestNamespace + "Message")?.Value;

			var stackTrace = unitTestResult
				.Element(TeamTestNamespace + "Output")
				?.Element(TeamTestNamespace + "ErrorInfo")
				?.Element(TeamTestNamespace + "StackTrace")?.Value;

			results.Add(
				new Tuple<string, string, string>(testName, message,
					stackTrace));
		}

		if ( results.Count == 0 )
		{
			return;
		}

		Log.Information("\nFailed tests:\n");

		foreach ( var result in results )
		{
			Log.Error($"Test Name: {result.Item1}");
			Log.Error($"Message: {result.Item2}");
			Log.Error($"Stack Trace: {result.Item3}");
			Log.Information("------------------------");
		}
	}
}
