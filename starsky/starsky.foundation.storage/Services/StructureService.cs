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
	public class StructureService
	{
		private readonly IStorage _storage;
		private readonly string _structure;

		public StructureService(IStorage storage, string structure)
		{
			_storage = storage;
			_structure = structure;
		}

		public string GetSubPaths(DateTime dateTime, string fileNameBase = "")
		{
			CheckStructureFormat();
			var parsedStructuredList = ParseStructure(dateTime, fileNameBase = "");

			var parentFolderBuilder = new StringBuilder();
			foreach ( var subStructureItem in 
				parsedStructuredList.GetRange(0,parsedStructuredList.Count-1) )
			{
				
				var currentChildFolderBuilder = new StringBuilder();
				currentChildFolderBuilder.Append("/");

				foreach ( var structureItem in subStructureItem )
				{
					currentChildFolderBuilder.Append(structureItem.Output);
				}

				var parentFolderSubPath = FilenamesHelper.GetParentPath(parentFolderBuilder.ToString());
				var existParentFolder = _storage.ExistFolder(parentFolderSubPath);
				
				// default situation without asterisk or child directory is not found
				if ( ! currentChildFolderBuilder.ToString().Contains("*") || !existParentFolder)
				{
					var removedAsterisk = RemoveAsteriskFromString(currentChildFolderBuilder);
					parentFolderBuilder.Append(removedAsterisk);
					continue;
				}

				parentFolderBuilder =
					MatchChildDirectories(parentFolderBuilder, currentChildFolderBuilder);

			}
			return PathHelper.AddSlash(parentFolderBuilder.ToString());
		}

		/// <summary>
		/// Check if a currentChildFolderBuilder exist in the parentFolderBuilder
		/// </summary>
		/// <param name="parentFolderBuilder">parent folder (subPath style)</param>
		/// <param name="currentChildFolderBuilder">child folder with asterisk</param>
		/// <returns>SubPath without asterisk</returns>
		private StringBuilder MatchChildDirectories(StringBuilder parentFolderBuilder, StringBuilder currentChildFolderBuilder)
		{
			var childDirectories = _storage
				.GetDirectories(parentFolderBuilder.ToString()).ToList();

			var matchRegex = new Regex(
				currentChildFolderBuilder.ToString().Replace("*", ".+")
			);
				
			var matchingFolders= childDirectories.FirstOrDefault(p => matchRegex.IsMatch(p));
			
			// When a new folder with asterisk is created
			if ( matchingFolders == null )
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
			parentFolderBuilder.Append(matchingFolders);
			return parentFolderBuilder;
		}

		/// <summary>
		/// Replace
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private string RemoveAsteriskFromString(StringBuilder input )
		{
			return input.ToString().Replace("*", string.Empty);
		} 

		private void CheckStructureFormat()
		{
			if ( _structure.StartsWith("/") && _structure.EndsWith(".ext") && _structure != "/.ext" ) return;
			throw new FieldAccessException("use right format");
		}
		
		/// <summary>
		/// Find 'Custom date and time format strings'
		/// @see: https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
		/// Not escaped regex: \\?(d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3})
		/// </summary>
		const string DateRegexPattern = "\\\\?(d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3})";

		private List<List<StructureRange>> ParseStructure(DateTime dateTime, string fileNameBase = "")
		{
			var structureList = _structure.Split('/');

			var parsedStructuredList = new List<List<StructureRange>>();
			foreach ( var structureItem in structureList )
			{
				if ( string.IsNullOrWhiteSpace(structureItem) ) continue;

				var matchCollection = new 
						Regex(DateRegexPattern + "|{filenamebase}|\\*|.ext|.")
							.Matches(structureItem);
				
				var matchList = new List<StructureRange>();
				foreach ( Match match in matchCollection )
				{
					matchList.Add(new StructureRange
					{
						Pattern = match.Value,
						Start = match.Index,
						End = match.Index + match.Length,
						Output = OutputParser(match.Value,dateTime, fileNameBase)
					});
				}

				parsedStructuredList.Add( matchList.OrderBy(p => p.Start).ToList());
			}

			return parsedStructuredList;
		}

		private string OutputParser(string pattern, DateTime dateTime, string fileNameBase)
		{
			// allow only full word matches (so .ext is no match)
			MatchCollection matchCollection = new Regex(DateRegexPattern).Matches(pattern);
			foreach ( Match match in matchCollection )
			{
				if ( !match.Value.StartsWith("\\") && match.Index == 0 && match.Length == pattern.Length )
				{
					return dateTime.ToString(pattern, CultureInfo.InvariantCulture);
				}
			}
			
			// the other options
			switch ( pattern )
			{
				case "{filenamebase}":
					return fileNameBase;
				default:
					return pattern.Replace("\\",string.Empty);
			}
		}
	}
}
