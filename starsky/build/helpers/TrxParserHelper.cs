using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Serilog;

namespace helpers;

[SuppressMessage("Sonar",
	"S6664: Reduce the number of Information logging calls within this code block",
	Justification = "Not production code.")]
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

	private sealed class SlowTestResult
	{
		public string TestName { get; set; } = string.Empty;
		public TimeSpan Duration { get; set; }
	}

	public static void DisplaySlowestTests(string trxFullFilePath, int take = 25)
	{
		var resolvedTrxPath = ResolveTrxPath(trxFullFilePath);
		if ( resolvedTrxPath == null )
		{
			Log.Information("\nSlowest tests (top {Take}): no TRX file found\n", take);
			return;
		}

		var xmlString = File.ReadAllText(resolvedTrxPath);
		var xmlDoc = XDocument.Parse(xmlString);

		var resultsElement = xmlDoc.Descendants(TeamTestNamespace + "Results")
			.FirstOrDefault();
		var unitTestResultsList = resultsElement
			?.Elements(TeamTestNamespace + "UnitTestResult").ToList();

		if ( unitTestResultsList == null || unitTestResultsList.Count == 0 )
		{
			Log.Information("\nSlowest tests (top {Take}): no test result entries found\n", take);
			return;
		}

		var slowestTests = new List<SlowTestResult>();
		foreach ( var unitTestResult in unitTestResultsList )
		{
			var testName = unitTestResult.Attribute("testName")?.Value;
			if ( string.IsNullOrWhiteSpace(testName) )
			{
				continue;
			}

			if ( !TryParseDuration(unitTestResult, out var duration) )
			{
				continue;
			}

			slowestTests.Add(new SlowTestResult { TestName = testName, Duration = duration });
		}

		var topSlowest = slowestTests
			.OrderByDescending(p => p.Duration)
			.Take(take)
			.ToList();

		if ( topSlowest.Count == 0 )
		{
			Log.Information("\nSlowest tests (top {Take}): no durations found\n", take);
			return;
		}

		Log.Information("\n\nSlowest tests (top {Take}):\n", take);
		foreach ( var result in topSlowest )
		{
			Log.Information("{Duration} | {TestName}", result.Duration, result.TestName);
		}
		Log.Information("------------------------");
	}

	private static bool TryParseDuration(XElement unitTestResult, out TimeSpan duration)
	{
		var durationRaw = unitTestResult.Attribute("duration")?.Value;
		if ( TimeSpan.TryParse(durationRaw, CultureInfo.InvariantCulture, out duration) )
		{
			return true;
		}

		var startRaw = unitTestResult.Attribute("startTime")?.Value;
		var endRaw = unitTestResult.Attribute("endTime")?.Value;
		if ( DateTime.TryParse(startRaw, CultureInfo.InvariantCulture,
			     DateTimeStyles.RoundtripKind, out var startTime) &&
		     DateTime.TryParse(endRaw, CultureInfo.InvariantCulture,
			     DateTimeStyles.RoundtripKind, out var endTime) && endTime >= startTime )
		{
			duration = endTime - startTime;
			return true;
		}

		duration = TimeSpan.Zero;
		return false;
	}

	private static string? ResolveTrxPath(string trxFullFilePath)
	{
		if ( File.Exists(trxFullFilePath) )
		{
			return trxFullFilePath;
		}

		var testResultsFolder = Path.GetDirectoryName(trxFullFilePath);
		if ( string.IsNullOrWhiteSpace(testResultsFolder) || !Directory.Exists(testResultsFolder) )
		{
			return null;
		}

		var trxFiles = Directory.GetFiles(testResultsFolder, "*.trx", SearchOption.AllDirectories)
			.OrderByDescending(File.GetLastWriteTimeUtc)
			.ToList();

		return trxFiles.FirstOrDefault();
	}

	public static void DisplayFailedFileTests(string trxFullFilePath)
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

		var results = new List<Tuple<string?, string?, string?>>();
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
				new Tuple<string?, string?, string?>(testName, message,
					stackTrace));
		}

		if ( results.Count == 0 )
		{
			return;
		}

		Log.Information("\nFailed tests:\n");

		foreach ( var result in results )
		{
			Log.Error("Test Name: {TestName}", result.Item1);
			Log.Error("Message: {Message}", result.Item2);
			Log.Error("Stack Trace: {StackTrace}", result.Item3);
			Log.Information("------------------------");
		}
	}
}
