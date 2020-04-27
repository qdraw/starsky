using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
			var preflightStructure = PreflightStructure(dateTime, fileNameBase = "");
			return "";
		}

		private void CheckStructureFormat()
		{
			if ( _structure.StartsWith("/") && _structure.EndsWith(".ext") && _structure != "/.ext" ) return;
			throw new FieldAccessException("use right format");
		}

		private bool PreflightStructure(DateTime dateTime, string fileNameBase = "")
		{
			var structureList = _structure.Split('/');
			
			foreach ( var structureItem in structureList )
			{
				if ( string.IsNullOrWhiteSpace(structureItem) ) continue;

				var matchCollection = new Regex(
						"d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3}|{filenamebase}|\\*")
					.Matches(structureItem);
				
				var matchList = new List<StructureRange>();
				foreach ( Match match in matchCollection )
				{
					matchList.Add(new StructureRange
					{
						Pattern = match.Value,
						Start = match.Index,
						End = match.Index + match.Length,
						Output = OutputDateTimeParse(match.Value,dateTime)
					});
				}

				for ( int i = 0; i < structureItem.Length; i++ )
				{
					var isParsed = matchList.Any(p => p.Start >= i && p.End <= i+1);
				}


				Console.WriteLine();
				// var item = dateTime.ToString(match.Value, CultureInfo.InvariantCulture);

			}

			return true;
		}

		private string OutputDateTimeParse(string pattern, DateTime dateTime)
		{
			if ( pattern == "{filenamebase}" || pattern == "*" )
			{
				return string.Empty;
			}
			return dateTime.ToString(pattern, CultureInfo.InvariantCulture);
		}
		

		// private List<KeyValuePair<string,List<StructurePreflightRange>>> PreflightStructure()
		// {
		// 	var parsedStructure = new List<KeyValuePair<string,List<StructurePreflightRange>>>();
		// 	
		// 	var structureList = _structure.Split('/');
		// 	foreach ( var structureItem in structureList )
		// 	{
		// 		AddItemToRangeKeyValuePairList(parsedStructure, structureItem, "{filenamebase}");
		// 		AddItemToRangeKeyValuePairList(parsedStructure, structureItem, "\\*");
		// 		AddItemToRangeKeyValuePairList(parsedStructure,structureItem, "d{1,4}|f{1,6}|F{1,6}|g{1,2}|h{1,2}|H{1,2}|K|m{1,2}|M{1,4}|s{1,2}|t{1,2}|y{1,5}|z{1,3}");
		//
		// 		// default situation
		// 		var existItem = parsedStructure.
		// 			FirstOrDefault(p => p.Key == structureItem);
		// 		if ( existItem.Key == null )
		// 		{
		// 			parsedStructure.Add(new KeyValuePair<string, List<StructurePreflightRange>>(structureItem,null));
		// 		}
		// 	}
		// 	return parsedStructure;
		// }
		//
		// private void AddItemToRangeKeyValuePairList(List<KeyValuePair<string,List<StructurePreflightRange>>> parsedStructure, string structureItem, string pattern)
		// {
		//
		// 	var matchCollection = new Regex(pattern).Matches(structureItem);
		// 	foreach ( Match match in matchCollection )
		// 	{
		// 		var existItem = parsedStructure
		// 			.FirstOrDefault(p => p.Key == structureItem);
		// 		var updateRange = new StructurePreflightRange
		// 		{
		// 			Start = match.Index,
		// 			End = match.Index + pattern.Length,
		// 			Pattern = pattern
		// 		};
		// 		
		// 		if ( existItem.Key != null )
		// 		{
		// 			existItem.Value.Add(updateRange);
		// 			return;
		// 		}
		// 		parsedStructure.Add(new KeyValuePair<string, List<StructurePreflightRange>>(structureItem, new List<StructurePreflightRange>{updateRange}));
		// 	}
		// }
	}


}
