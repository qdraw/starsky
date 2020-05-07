#if SYSTEM_TEXT_ENABLED
using System.Text.Json.Serialization;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Models
{
	/// <summary>
	/// Used to display file status (eg. NotFoundNotInIndex, Ok)
	/// </summary>
	public enum ImportStatus
	{
		Default,
		Ok,
		IgnoredAlreadyImported,
		AgeToOld,
		FileError,
		NotFound
	}
	
    public class ImportIndexItem
    {
        private readonly AppSettings _appSettings;

        /// <summary>
        /// In order to create an instance of 'ImportIndexItem'
        /// EF requires that a parameter-less constructor be declared.
        /// </summary>
        public ImportIndexItem()
        {
        }
        
        public ImportIndexItem(AppSettings appSettings)
        {
            _appSettings = appSettings;
            Structure = _appSettings.Structure;
        }

        /// <summary>
        /// Database Number (isn't used anywhere)
        /// </summary>
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// FileHash before importing
        /// When using a -ColorClass=1 overwrite the fileHash changes during the import process
        /// </summary>
        public string FileHash { get; set; }
        
        /// <summary>
        /// The location where the image should be stored.
        /// When the user move an item this field is NOT updated
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// UTC DateTime when the file is imported
        /// </summary>
        public DateTime AddToDatabase { get; set; }

        /// <summary>
        /// DateTime of the photo/or when it is originally is made
        /// </summary>
        public DateTime DateTime{ get; set; }
	    
	    [NotMapped]
#if SYSTEM_TEXT_ENABLED
		[JsonConverter(typeof(JsonStringEnumConverter))]
#else
	    [JsonConverter(typeof(StringEnumConverter))]
#endif
	    public ImportStatus Status { get; set; }
	    
	    [NotMapped]
		public FileIndexItem FileIndexItem { get; set; }
        
        [NotMapped]
        [JsonIgnore]
        public string SourceFullFilePath { get; set; }

        // Defaults to _appSettings.Structure
        // Feature to overwrite system structure by request
        [NotMapped] 
        [JsonIgnore]
        public string Structure { get; set; }

        public DateTime ParseDateTimeFromFileName()
        {
            // Depends on 'AppSettingsProvider.Structure'
            // depends on SourceFullFilePath
            if(string.IsNullOrEmpty(SourceFullFilePath)) {return new DateTime();}

            var fileName = Path.GetFileNameWithoutExtension(SourceFullFilePath);
            
            // Replace Astriks > escape all options
            var structuredFileName = Structure.Split("/".ToCharArray()).LastOrDefault();
            structuredFileName = structuredFileName.Replace("*", "");
            structuredFileName = structuredFileName.Replace(".ext", string.Empty);
            structuredFileName = structuredFileName.Replace("{filenamebase}", string.Empty);
            
            DateTime.TryParseExact(fileName, 
                structuredFileName, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out var dateTime);

            if (dateTime.Year >= 2)
            {
                DateTime = dateTime;
                return dateTime;
            }
                            
            // Now retry it and replace special charaters from string
            // For parsing files like: '2018-08-31 18.50.35' > '20180831185035'
            Regex pattern = new Regex("-|_| |;|\\.|:");
            fileName = pattern.Replace(fileName,string.Empty);
            structuredFileName = pattern.Replace(structuredFileName,string.Empty);
                
            DateTime.TryParseExact(fileName, 
                structuredFileName, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out dateTime);
            
            if (dateTime.Year >= 2)
            {
                DateTime = dateTime;
                return dateTime;
            }

            // when using /yyyymmhhss_{filenamebase}.jpg
            // For the situation that the image has no exif date and there is an appendix used (in the config)
            if(structuredFileName.Length >= fileName.Length)  {
                
                structuredFileName = structuredFileName.Substring(0, fileName.Length-1);
                
                DateTime.TryParseExact(fileName, 
                    structuredFileName, 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, 
                    out dateTime);
            }
	        
	        if (dateTime.Year >= 2)
	        {
		        DateTime = dateTime;
		        return dateTime;
	        }

	        // For the situation that the image has no exif date and there is an appendix
	        // used in the source filename AND the config
	        if ( fileName.Length >= structuredFileName.Length )
	        {
		        structuredFileName = RemoveEscapedCharacters(structuredFileName);
		        
		        // short the filename with structuredFileName
		        fileName = fileName.Substring(0, structuredFileName.Length);
		        
		        DateTime.TryParseExact(fileName, 
			        structuredFileName, 
			        CultureInfo.InvariantCulture, 
			        DateTimeStyles.None, 
			        out dateTime);
	        }
        
            // Return 0001-01-01 if everything fails
            DateTime = dateTime;
            return dateTime;
        }

	    /// <summary>
	    /// Removes the escaped characters and the first character after the backslash
	    /// </summary>
	    /// <param name="inputString">to input</param>
	    /// <returns>the input string without those characters</returns>
	    public string RemoveEscapedCharacters(string inputString)
	    {
		    var newString = new StringBuilder();
		    for ( int i = 0; i < inputString.ToCharArray().Length; i++ )
		    {
			    var structuredCharArray = inputString[i];
			    var escapeChar = "\\"[0];
			    if ( i != 0 && structuredCharArray != escapeChar && inputString[i - 1] != escapeChar )
			    {
				    newString.Append(structuredCharArray);
			    }

			    // add the first one
			    if ( i == 0 && structuredCharArray != escapeChar) newString.Append(structuredCharArray);
			    
		    }
		    return newString.ToString();
	    }

        public List<string> SearchSubDirInDirectory(string parentItem, string parsedItem)
        {
            if (_appSettings == null) throw new FieldAccessException("use with _appsettings");
            var childDirectories = Directory.GetDirectories(
                _appSettings.DatabasePathToFilePath(parentItem), parsedItem).ToList();
            childDirectories = childDirectories.Where(p => p[0].ToString() != ".").OrderBy(s => s).ToList();
            return childDirectories;
        }
    }
}
