using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Services
{
	public sealed class StructureService
	{
		private readonly IStorage _storage;
		private readonly string _structure;

		public StructureService(IStorage storage, string structure)
		{
			_storage = storage;
			_structure = structure;
		}

		/// <summary>
		/// Get the fileName from the structure and ignore the parent folders
		/// Does NOT check if the file already exist
		/// </summary>
		/// <param name="dateTime">DateTime to parse</param>
		/// <param name="fileNameBase">include fileName if requested in structure</param>
		/// <param name="extensionWithoutDot">fileExtension without dot</param>
		/// <returns>filename without starting slash</returns>
		public string ParseFileName(DateTime dateTime,
			string fileNameBase = "",
			string extensionWithoutDot = "")
		{
			CheckStructureFormat();
			var fileName = FilenamesHelper.GetFileName(_structure);
			var fileNameStructure = PathHelper.PrefixDbSlash(fileName);
			var parsedStructuredList = ParseStructure(fileNameStructure, dateTime, fileNameBase,
				extensionWithoutDot);
			return PathHelper.RemovePrefixDbSlash(
				ApplyStructureRangeToStorage(parsedStructuredList));
		}

		/// <summary>
		/// Parse the parent folder and ignore the filename
		/// </summary>
		/// <param name="getSubPathRelative">datetime relative to now</param>
		/// <param name="fileNameBase">include fileName if requested in structure</param>
		/// <param name="extensionWithoutDot">include parentFolder if requested in structure (not recommend)</param>
		/// <returns>sub Path including folders</returns>
		public string? ParseSubfolders(int? getSubPathRelative, string fileNameBase = "",
			string extensionWithoutDot = "")
		{
			if ( getSubPathRelative == null ) return null;
			var dateTime = DateTime.Now.AddDays(( double )getSubPathRelative);
			return ParseSubfolders(dateTime, fileNameBase, extensionWithoutDot);
		}

		/// <summary>
		/// Parse the parent folder and ignore the filename
		/// </summary>
		/// <param name="dateTime">DateTime to parse</param>
		/// <param name="fileNameBase">include fileName if requested in structure</param>
		/// <param name="extensionWithoutDot">include parentFolder if requested in structure (not recommend)</param>
		/// <returns>sub Path including folders</returns>
		public string ParseSubfolders(DateTime dateTime, string fileNameBase = "",
			string extensionWithoutDot = "")
		{
			CheckStructureFormat();
			var parsedStructuredList =
				ParseStructure(_structure, dateTime, fileNameBase, extensionWithoutDot);

			return ApplyStructureRangeToStorage(
				parsedStructuredList.GetRange(0, parsedStructuredList.Count - 1));
		}

		/// <summary>
		/// Get the output of structure applied on the storage
		/// With the DateTime and fileNameBase applied
		/// </summary>
		/// <param name="parsedStructuredList">parsed object, only needed to apply on the storage</param>
		/// <returns>string with subPath</returns>
		private string ApplyStructureRangeToStorage(List<List<StructureRange>> parsedStructuredList)
		{
			var parentFolderBuilder = new StringBuilder();
			foreach ( var subStructureItem in parsedStructuredList )
			{
				var currentChildFolderBuilder = new StringBuilder();
				currentChildFolderBuilder.Append('/');

				foreach ( var structureItem in subStructureItem )
				{
					currentChildFolderBuilder.Append(structureItem.Output);
				}

				var parentFolderSubPath =
					FilenamesHelper.GetParentPath(parentFolderBuilder.ToString());
				var existParentFolder = _storage.ExistFolder(parentFolderSubPath);

				// default situation without asterisk or child directory is NOT found
				if ( !currentChildFolderBuilder.ToString().Contains('*') || !existParentFolder )
				{
					var currentChildFolderRemovedAsterisk =
						RemoveAsteriskFromString(currentChildFolderBuilder);
					parentFolderBuilder.Append(currentChildFolderRemovedAsterisk);
					continue;
				}

				parentFolderBuilder =
					MatchChildDirectories(parentFolderBuilder, currentChildFolderBuilder);
			}

			return parentFolderBuilder.ToString();
		}

		/// <summary>
		/// Match Direct name first if available and then check on regex
		/// So: 2020_01_01 will be first matched and later will be checked for 2020_01_01_test
		/// </summary>
		/// <param name="parentFolderBuilder">Parent Directory</param>
		/// <param name="currentChildFolderBuilder">the current folder name with asterisk </param>
		/// <param name="p">other child folder items (item in loop of childDirectories)</param>
		/// <returns>is match</returns>
		private static bool MatchChildFolderSearch(StringBuilder parentFolderBuilder,
			StringBuilder currentChildFolderBuilder, string p)
		{
			var matchDirectFolderName = RemoveAsteriskFromString(currentChildFolderBuilder);
			if ( matchDirectFolderName != "/" && p == parentFolderBuilder + matchDirectFolderName )
				return true;

			var matchRegex = new Regex(
				parentFolderBuilder + currentChildFolderBuilder.ToString().Replace("*", ".+"),
				RegexOptions.None, TimeSpan.FromMilliseconds(100)
			);
			return matchRegex.IsMatch(p);
		}

		/// <summary>
		/// Check if a currentChildFolderBuilder exist in the parentFolderBuilder
		/// </summary>
		/// <param name="parentFolderBuilder">parent folder (subPath style)</param>
		/// <param name="currentChildFolderBuilder">child folder with asterisk</param>
		/// <returns>SubPath without asterisk</returns>
		private StringBuilder MatchChildDirectories(StringBuilder parentFolderBuilder,
			StringBuilder currentChildFolderBuilder)
		{
			// should return a list of: </2019/10/2019_10_08>
			var childDirectories = _storage.GetDirectories(parentFolderBuilder.ToString()).ToList();

			var matchingFoldersPath = childDirectories.Find(p =>
				MatchChildFolderSearch(parentFolderBuilder, currentChildFolderBuilder, p)
			);

			// When a new folder with asterisk is created
			if ( matchingFoldersPath == null )
			{
				var defaultValue = RemoveAsteriskFromString(currentChildFolderBuilder);
				// When only using Asterisk in structure
				if ( defaultValue == "/" )
				{
					defaultValue = "/default";
				}

				parentFolderBuilder.Append(defaultValue);
				return parentFolderBuilder;
			}

			// When a regex folder is matched
			var childFolderName =
				PathHelper.PrefixDbSlash(FilenamesHelper.GetFileName(matchingFoldersPath));

			parentFolderBuilder.Append(childFolderName);
			return parentFolderBuilder;
		}

		/// <summary>
		/// Replace * with empty string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static string RemoveAsteriskFromString(StringBuilder input)
		{
			return input.ToString().Replace("*", string.Empty);
		}

		/// <summary>
		/// Check if the structure is right formatted
		/// </summary>
		/// <exception cref="FieldAccessException">When not start with / or is /.ext or not end with .ext</exception>
		private void CheckStructureFormat()
		{
			if ( !string.IsNullOrEmpty(_structure) &&
			     _structure.StartsWith('/') && _structure.EndsWith(".ext") &&
			     _structure != "/.ext" )
			{
				return;
			}

			throw new FieldAccessException(
				"Structure is not right formatted, please read the documentation");
		}

		/// <summary>
		/// Find 'Custom date and time format strings'
		/// @see: https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
		/// Not escaped regex: \\?(d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3})
		/// </summary>
		const string DateRegexPattern =
			"\\\\?(d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3})";

		/// <summary>
		/// Parse the dateTime structure input to the dateTime provided
		/// </summary>
		/// <param name="structure">Structure</param>
		/// <param name="dateTime">Where to parse to</param>
		/// <param name="fileNameBase">source name, can be used in the options</param>
		/// <param name="extensionWithoutDot">fileExtension without dot</param>
		/// <returns>Object with Structure Range output</returns>
		private static List<List<StructureRange>> ParseStructure(string structure,
			DateTime dateTime,
			string fileNameBase = "", string extensionWithoutDot = "")
		{
			var structureList = structure.Split('/');

			var parsedStructuredList = new List<List<StructureRange>>();
			foreach ( var structureItem in structureList )
			{
				if ( string.IsNullOrWhiteSpace(structureItem) ) continue;

				var matchCollection = new
						Regex(DateRegexPattern + "|{filenamebase}|\\*|.ext|.",
							RegexOptions.None, TimeSpan.FromMilliseconds(1000))
					.Matches(structureItem);

				var matchList = new List<StructureRange>();
				foreach ( Match match in matchCollection )
				{
					matchList.Add(new StructureRange
					{
						Pattern = match.Value,
						Start = match.Index,
						End = match.Index + match.Length,
						Output = OutputStructureRangeItemParser(match.Value, dateTime,
							fileNameBase, extensionWithoutDot)
					});
				}

				parsedStructuredList.Add(matchList.OrderBy(p => p.Start).ToList());
			}

			return parsedStructuredList;
		}

		/// <summary>
		/// Parse the name of item to the set DateTime
		/// </summary>
		/// <param name="pattern">Split pattern item. For example yyyy or dd</param>
		/// <param name="dateTime">Date and Time</param>
		/// <param name="fileNameBase">source file name without extension</param>
		/// <param name="extensionWithoutDot">fileExtension without dot</param>
		/// <returns>Current item name, with parsed DateTime and without escape signs</returns>
		private static string OutputStructureRangeItemParser(string pattern, DateTime dateTime,
			string fileNameBase, string extensionWithoutDot = "")
		{
			// allow only full word matches (so .ext is no match)
			var matchCollection = new Regex(DateRegexPattern,
				RegexOptions.None, TimeSpan.FromMilliseconds(100)).Matches(pattern);

			foreach ( Match match in matchCollection )
			{
				// Ignore escaped items
				if ( !match.Value.StartsWith('\\') && match.Index == 0 &&
				     match.Length == pattern.Length )
				{
					return dateTime.ToString(pattern, CultureInfo.InvariantCulture);
				}
			}

			// the other options
			switch ( pattern )
			{
				case "{filenamebase}":
					return fileNameBase;
				case ".ext":
					return string.IsNullOrEmpty(extensionWithoutDot)
						? ".unknown"
						: $".{extensionWithoutDot}";
				default:
					return pattern.Replace("\\", string.Empty);
			}
		}
	}
}
