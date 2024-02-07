﻿using Microsoft.AspNetCore.Http;
using starsky.foundation.platform.Models;

namespace starsky.feature.import.Models
{
	public class ImportSettingsModel
	{
		// Default constructor
		public ImportSettingsModel()
		{
			DeleteAfter = false;
			RecursiveDirectory = false;
			IndexMode = true;
			// ColorClass defaults in prop
			// Structure defaults in appSettings
		}

		/// <summary>
		/// Construct model using a request
		/// </summary>
		/// <param name="request"></param>
		public ImportSettingsModel(HttpRequest request)
		{
			// the header defaults to zero, and that's not the correct default value
			if ( !string.IsNullOrWhiteSpace(request.Headers["ColorClass"]) &&
			     int.TryParse(request.Headers["ColorClass"], out var colorClassNumber) )
			{
				ColorClass = colorClassNumber;
			}

			Structure = request.Headers["Structure"].ToString();

			// Always when importing using a request
			// otherwise it will stick in the temp folder
			DeleteAfter = true;

			// For the index Mode, false is always copy, true is check if exist in db, default true
			IndexMode = true;

			if ( request.Headers["IndexMode"].ToString().Equals("false",
				    System.StringComparison.CurrentCultureIgnoreCase) )
			{
				IndexMode = false;
			}
		}


		// This is optional, when not in use ignore this setting
		private string _structure = string.Empty;

		public string Structure
		{
			get => string.IsNullOrEmpty(_structure)
				? string.Empty
				: _structure; // if null>stringEmpty
			set
			{
				// Changed this => value used te be without check
				if ( string.IsNullOrEmpty(value) ) return;
				AppSettings.StructureCheck(value);
				_structure = value;
			}
		}

		public bool DeleteAfter { get; set; }

		public bool RecursiveDirectory { get; set; }

		/// <summary>
		/// -1 is ignore
		/// </summary>
		private int _colorClass = -1;

		/// <summary>
		/// Overwrite ColorClass settings
		/// Int value between 0 and 8
		/// </summary>
		public int ColorClass
		{
			get => _colorClass;
			set
			{
				if ( value is >= 0 and <= 8 ) // hardcoded in FileIndexModel
				{
					_colorClass = value;
					return;
				}

				_colorClass = -1;
			}
		}

		/// <summary>
		/// indexing, false is always copy, true is check if exist in db,
		///         default true
		/// </summary>
		public bool IndexMode { get; set; }

		public ConsoleOutputMode ConsoleOutputMode { get; set; }

		public bool IsConsoleOutputModeDefault()
		{
			return ConsoleOutputMode.Default == ConsoleOutputMode;
		}
	}
}
