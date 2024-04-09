using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.Architecture;

/// <summary>
/// Fitness functions for software architecture are defined as a way of confirming that a particular
/// characteristic of a solution's architecture that is considered important to maintain over time, 
/// is in fact still the case.
/// Some of these can be confirmed via automated tests.
/// This class defines tests that ensure the project reference structure as defined by Sitecore
/// Helix principles are being maintained.
/// </summary>
/// <remarks>
/// References:
///   https://www.thoughtworks.com/insights/blog/microservices-evolutionary-architecture
///   http://shop.oreilly.com/product/0636920080237.do
///   http://helix.sitecore.net/introduction/index.html
/// </remarks>
[TestClass]
public class TheSolutionShould
{
	private const string SolutionName = "starsky.sln";

	private const string Foundation = "Foundation";
	private const string Feature = "Feature";
	private const string Project = "Project";
	private const string ProjectCli = "cli";
	private const string MainProject = "starsky";

	private const string TestProjectSuffix = "test";
	private const string BuildProject = "_build";
	private const string DocsProject = "documentation";

	/// <summary>
	/// Ensures that only appropriate inter-project references are in place within the solution.
	/// Specifically:
	///   - Foundation projects should only reference other Foundation projects
	///   - Feature projects should only reference Foundation projects
	///   - Project projects should only reference Foundation or Feature projects
	/// </summary>
	/// <remarks>
	/// Hat-tip for locating projects and references in solution: https://stackoverflow.com/a/17571223/489433
	/// </remarks>
	[TestMethod]
	public void ContainNoHelixInvalidProjectReferences()
	{
		// Arrange - get list of projects in the solution, along with a list of references for each project
		var projects = GetProjectsWithReferences();

		// Act - review project list for invalid references
		var invalidFoundationReferences = new List<string>();
		var invalidFeatureReferences = new List<string>();
		var invalidProjectReferences = new List<string>();
		foreach ( var project in projects )
		{
			if ( project.Key.Contains(Foundation, StringComparison.OrdinalIgnoreCase) )
			{
				GetInvalidReferences(project, new[] { Feature, Project },
					invalidFoundationReferences);
				continue;
			}

			if ( project.Key.Contains(Feature, StringComparison.OrdinalIgnoreCase) )
			{
				GetInvalidReferences(project, new[] { Feature, Project },
					invalidFeatureReferences);
				continue;
			}

			if ( project.Key.Contains(Project, StringComparison.OrdinalIgnoreCase) ||
			     project.Key == MainProject || project.Key.Contains(ProjectCli,
				     StringComparison.OrdinalIgnoreCase) )
			{
				GetInvalidReferences(project, new[] { Project }, invalidProjectReferences);
				continue;
			}

			if ( project.Key.Contains(TestProjectSuffix,
				     StringComparison.OrdinalIgnoreCase) || project.Key == BuildProject ||
			     project.Key == DocsProject )
			{
				continue;
			}

			throw new MissingFieldException($"Unknown project type {project}");
		}

		// Assert - check if we have any invalid refererences
		AssertLayerReferences(invalidFoundationReferences, Foundation);
		AssertLayerReferences(invalidFeatureReferences, Feature);
		AssertLayerReferences(invalidProjectReferences, Project);
	}

	private static Dictionary<string, IList<string>> GetProjectsWithReferences()
	{
		var solutionFilePath = GetSolutionFilePath();
		var solutionFileContents = File.ReadAllText(solutionFilePath);
		var matches = GetProjectsFromSolutionFileContents(solutionFileContents);
		return matches
			.Select(x => x.Groups[2].Value)
			.ToDictionary(GetProjectFileName,
				p => GetReferencedProjects(solutionFilePath, p));
	}

	private static string GetSolutionFilePath()
	{
		var parent = Directory.GetParent(BaseDirectoryProjectHelper.BaseDirectoryProject)
			?.Parent?.Parent?.Parent?.Parent?.FullName;
		if ( string.IsNullOrEmpty(parent) )
		{
			throw new DirectoryNotFoundException("Unable to locate solution file");
		}

		return Path.Combine(parent, SolutionName);
	}

	private static IList<string> GetPathParts(string directory)
	{
		return directory
			.Split(new[] { "\\" }, StringSplitOptions.None)
			.ToList();
	}

	private static IEnumerable<Match> GetProjectsFromSolutionFileContents(
		string solutionFileContents)
	{
		var regex = new Regex(
			"Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\"",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		return regex.Matches(solutionFileContents);
	}

	private static string GetProjectFileName(string value)
	{
		var result = GetPathParts(value).LastOrDefault()?.Replace(".csproj", string.Empty);
		result ??= string.Empty;
		return result;
	}

	private static IList<string> GetReferencedProjects(string solutionFilePath,
		string projectFilePath)
	{
		var rootedProjectFilePath =
			Path.Combine(solutionFilePath.Replace(SolutionName, string.Empty),
				projectFilePath).Replace("\\", Path.DirectorySeparatorChar.ToString());
		return GetReferencedProjects(rootedProjectFilePath);
	}

	private static List<string> GetReferencedProjects(string rootedProjectFilePath)
	{
		var result = new List<string>();

		// Load the XML document
		var xmlDoc = XDocument.Load(rootedProjectFilePath);

		// Get all ProjectReference elements
		var projectReferences = xmlDoc.Descendants("ProjectReference");

		// Extract the Include attribute value from each ProjectReference element
		foreach ( var reference in projectReferences )
		{
			var includeValue = reference.Attribute("Include")?.Value;
			if ( !string.IsNullOrEmpty(includeValue) )
			{
				result.Add(includeValue);
			}
		}

		return result;
	}

	private static void GetInvalidReferences(KeyValuePair<string, IList<string>> project,
		string[] invalidLayers, List<string> invalidReferences)
	{
		invalidReferences.AddRange(project.Value
			.Where(x => ContainsInvalidReference(project.Key, x, invalidLayers))
			.Select(x => $"{project.Key} references {x}"));
	}

	private static bool ContainsInvalidReference(string projectName,
		string referencedProjectName, IEnumerable<string> invalidLayers)
	{
		return invalidLayers
			.Any(x => ProjectReferencesInvalidLayer(referencedProjectName, x) &&
			          !ReferencedProjectIsFromDedicatedTestProject(projectName,
				          referencedProjectName) &&
			          !ReferencedProjectIsPartOfSubFeature(projectName,
				          referencedProjectName));
	}

	private static bool ProjectReferencesInvalidLayer(string referencedProject,
		string invalidLayer)
	{
		return referencedProject.Contains(invalidLayer,
			StringComparison.InvariantCultureIgnoreCase);
	}

	private static bool ReferencedProjectIsFromDedicatedTestProject(string projectName,
		string referencedProject)
	{
		return projectName == referencedProject + TestProjectSuffix;
	}

	private static bool ReferencedProjectIsPartOfSubFeature(string projectName,
		string referencedProject)
	{
		// We have some features that are broken into more than one project.  Via naming convention
		// we should allow these to be valid.
		// E.g. MySolution.Feature.FeatureName.Api references MySolution.Feature.FeatureName.Model
		if ( projectName.Split('.').Length <= 3 ||
		     referencedProject.Split('.').Length <= 3 )
		{
			// Not a sub-feature
			return false;
		}

		return projectName.Split('.')[2] == referencedProject.Split('.')[2];
	}

	private static void AssertLayerReferences(IReadOnlyCollection<string> invalidReferences,
		string layer)
	{
		Assert.AreEqual(0, invalidReferences.Count,
			$"There should be no Helix incompatible references in the {layer} layer. " +
			$"{invalidReferences.Count} invalid reference{( invalidReferences.Count == 1 ? " was" : "s were" )} " +
			$"found: {string.Join(", ", invalidReferences)}. Expected 0");
	}
}
