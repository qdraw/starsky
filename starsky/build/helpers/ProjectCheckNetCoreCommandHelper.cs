using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common.Tooling;
using Serilog;
using static SimpleExec.Command;
using static build.Build;

namespace helpers
{
	public static partial class ProjectCheckNetCoreCommandHelper
	{
		static string GetBuildToolsFolder()
		{
			var baseDirectory = AppDomain.CurrentDomain?
				.BaseDirectory;
			if ( baseDirectory == null )
				throw new DirectoryNotFoundException("base directory is null, this is wrong");
			var slnRootDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent
				?.Parent?.FullName;
			if ( slnRootDirectory == null )
				throw new DirectoryNotFoundException("slnRootDirectory is null, this is wrong");
			return Path.Combine(slnRootDirectory, BuildToolsPath);
		}

		static void CheckForNullable()
		{
			var projects = GetFilesHelper.GetFiles("**.csproj");
			var missingProjects = new List<string>();
			foreach ( var project in projects )
			{
				var projectFullPath =
					Path.Combine(WorkingDirectory.GetSolutionParentFolder(), project);
				var projectContent = File.ReadAllText(projectFullPath);

				if ( !projectContent.Contains("<Nullable>enable</Nullable>") )
				{
					missingProjects.Add(project);
				}
			}

			if ( missingProjects.Count > 0 )
			{
				throw new ArgumentException("Missing <Nullable>enable</Nullable> in: " +
				                            string.Join(" , ", missingProjects) + " projects  " +
				                            "Please add <Nullable>enable</Nullable> to the .csproj files");
			}
		}

		static void ProjectGuid()
		{
			var projects = GetFilesHelper.GetFiles("**.csproj");
			var uniqueGuids = new List<string>();

			foreach ( var project in projects )
			{
				var projectFullPath =
					Path.Combine(WorkingDirectory.GetSolutionParentFolder(), project);
				var projectContent = File.ReadAllText(projectFullPath);

				var fileXmlMatch = ProjectGuidRegex().Match(projectContent);

				if ( !fileXmlMatch.Success )
				{
					throw new NotSupportedException($"✖ {project} - No ProjectGuid in file");
				}

				if ( uniqueGuids.Contains(fileXmlMatch.Groups[0].Value) )
				{
					throw new NotSupportedException($"✖ {project} - ProjectGuid is not Unique");
				}

				uniqueGuids.Add(fileXmlMatch.Groups[0].Value);
				Log.Information($"✓ {project} - Is Ok");
			}
		}

		public static void ProjectCheckNetCoreCommand()
		{
			CheckForNullable();

			ProjectGuid();

			ClientHelper.NpmPreflight();

			// check branch names on CI
			// release-version-check.js triggers app-version-update.js to update the csproj and package.json files
			Run(NpmBaseCommand, "run release-version-check", GetBuildToolsFolder());

			/* List of nuget packages */
			Run(NpmBaseCommand, "run nuget-package-list", GetBuildToolsFolder());
		}

		const string BuildToolsPath = "starsky-tools/build-tools/";

		[GeneratedRegex(
			"(<ProjectGuid>){(([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}))}(<\\/ProjectGuid>)",
			RegexOptions.IgnoreCase, "nl-NL")]
		private static partial Regex ProjectGuidRegex();
	}
}
