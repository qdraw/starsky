﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;
using starskycore.Helpers;

namespace starsky.foundation.writemeta.Helpers
{
    public class ExifToolCmdHelper
    {
        private readonly IExifTool _exifTool;
	    private readonly IStorage _iStorage;
	    private readonly IStorage _thumbnailStorage;
	    private readonly IReadMeta _readMeta;

	    /// <summary>
	    /// Run ExifTool 
	    /// </summary>
	    /// <param name="exifTool">ExifTool Abstraction</param>
	    /// <param name="iStorage">Source storage provider</param>
	    /// <param name="thumbnailStorage">Thumbnail Storage Abstraction provider</param>
	    /// <param name="readMeta">ReadMeta abstraction</param>
	    public ExifToolCmdHelper(IExifTool exifTool, IStorage iStorage, IStorage thumbnailStorage, IReadMeta readMeta)
        {
            _exifTool = exifTool;
	        _iStorage = iStorage;
	        _readMeta = readMeta;
	        _thumbnailStorage = thumbnailStorage;
        }

	    /// <summary>
	    /// To update ExifTool (both Thumbnail as Storage item)
	    /// </summary>
	    /// <param name="updateModel">update model</param>
	    /// <param name="comparedNames">list,string e.g. Tags</param>
	    /// <param name="includeSoftware">include software export</param>
	    /// <returns></returns>
	    public string Update(FileIndexItem updateModel, List<string> comparedNames, bool includeSoftware = true)
        {
            var exifUpdateFilePaths = new List<string>
            {
                updateModel.FilePath           
            };
	        return UpdateAsyncWrapperBoth(updateModel, exifUpdateFilePaths, comparedNames, includeSoftware).Result;
        }

	    /// <summary>
	    /// To update ExifTool (both Thumbnail as Storage item)
	    /// </summary>
	    /// <param name="updateModel"></param>
	    /// <param name="inputSubPaths"></param>
	    /// <param name="comparedNames"></param>
	    /// <param name="includeSoftware"></param>
	    /// <returns></returns>
	    public string Update(FileIndexItem updateModel, List<string> inputSubPaths,
		    List<string> comparedNames, bool includeSoftware = true)
	    {
		    return UpdateAsyncWrapperBoth(updateModel, inputSubPaths, comparedNames, includeSoftware).Result;
	    }

        /// <summary>
        /// For Raw files us an external .xmp sidecar file, and add this to the PathsList
        /// </summary>
        /// <param name="inputSubPaths">list of files to update</param>
        /// <returns>list of files, where needed for raw-files there are .xmp used</returns>
        private List<string> PathsListTagsFromFile(List<string> inputSubPaths)
        {
            var pathsList = new List<string>();
            foreach (var subPath in inputSubPaths)
            {
                if(ExtensionRolesHelper.IsExtensionForceXmp(subPath))
                {
	                var xmpPath = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);
                    pathsList.Add(xmpPath);
                    continue;
                }
                pathsList.Add(subPath);
            }
            return pathsList;
        }

        /// <summary>
        /// Wrapper to do Async tasks -- add variable to test make it in a unit test shorter
        /// </summary>
        /// <param name="updateModel"></param>
        /// <param name="inputSubPaths"></param>
        /// <param name="comparedNames"></param>
        /// <param name="includeSoftware"></param>
        /// <returns></returns>
#pragma warning disable 1998
        private async Task<string> UpdateAsyncWrapperBoth(FileIndexItem updateModel, List<string> inputSubPaths,
	        List<string> comparedNames, bool includeSoftware = true)
#pragma warning restore 1998
        {
		    var task = Task.Run(() => UpdateASyncBoth(updateModel,inputSubPaths,comparedNames,includeSoftware));
		    return task.Wait(TimeSpan.FromSeconds(20)) ? task.Result : string.Empty;
	    }
	    
        /// <summary>
        /// Get command line args for exifTool by updateModel as data, comparedNames
        /// </summary>
        /// <param name="updateModel">data</param>
        /// <param name="comparedNames">list of fields that are changed, other fields are ignored</param>
        /// <param name="includeSoftware">to include the original software name</param>
        /// <returns>command line args</returns>
	    private string ExifToolCommandLineArgs( FileIndexItem updateModel, List<string> comparedNames, bool includeSoftware )
	    {
		    var command = "-json -overwrite_original";
            var initCommand = command; // to check if nothing

            // Create an XMP File -> as those files don't support those tags
            // Check first if it is needed

            command = UpdateKeywordsCommand(command, comparedNames, updateModel);
            command = UpdateDescriptionCommand(command, comparedNames, updateModel);

            command = UpdateGpsLatitudeCommand(command, comparedNames, updateModel);
            command = UpdateGpsLongitudeCommand(command, comparedNames, updateModel);
            command = UpdateLocationAltitudeCommand(command, comparedNames, updateModel);

            command = UpdateLocationCountryCommand(command, comparedNames, updateModel);
            command = UpdateLocationStateCommand(command, comparedNames, updateModel);
            command = UpdateLocationCityCommand(command, comparedNames, updateModel);
		    
	        command = UpdateSoftwareCommand(command, comparedNames, updateModel, includeSoftware);

	        command = UpdateImageHeightCommand(command, comparedNames, updateModel);
		    command = UpdateImageWidthCommand(command, comparedNames, updateModel);
		        
			command = UpdateTitleCommand(command, comparedNames, updateModel);
		    command = UpdateColorClassCommand(command, comparedNames, updateModel);
		    command = UpdateOrientationCommand(command, comparedNames, updateModel);
		    command = UpdateDateTimeCommand(command, comparedNames, updateModel);

		    command = UpdateIsoSpeedCommand(command, comparedNames, updateModel);
		    command = UpdateApertureCommand(command, comparedNames, updateModel);
		    command = UpdateShutterSpeedCommand(command, comparedNames, updateModel);

		    command = UpdateFocalLengthCommand(command, comparedNames, updateModel);

		    command = UpdateMakeModelCommand(command, comparedNames, updateModel);

		    if ( command == initCommand ) return string.Empty;
		    
		    return command;
	    }

        /// <summary>
        /// Create a XMP file when it not exist
        /// </summary>
        /// <param name="updateModel">model</param>
        /// <param name="inputSubPaths">list of paths</param>
        /// <returns>void</returns>
	    private async Task CreateXmpFileIsNotExist(FileIndexItem updateModel, List<string> inputSubPaths)
	    {
		    foreach ( var subPath in inputSubPaths )
		    {
			    // only for raw files
			    if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) ) return;

			    var withXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);

			    if ( _iStorage.IsFolderOrFile(withXmp) !=
			         FolderOrFileModel.FolderOrFileTypeList.Deleted ) continue;
			    
			    new ExifCopy(_iStorage,_thumbnailStorage, _exifTool,_readMeta).XmpCreate(withXmp);
				    
			    var comparedNames = FileIndexCompareHelper.Compare(new FileIndexItem(), updateModel);
			    var command = ExifToolCommandLineArgs(updateModel, comparedNames, true);
				    
			    await _exifTool.WriteTagsAsync(withXmp, command);
		    }
	    }

        private async Task<string> UpdateASyncBoth(FileIndexItem updateModel, List<string> inputSubPaths, 
	        List<string> comparedNames, bool includeSoftware)
        {
	        // Creation and update .xmp file with all available content
	        await CreateXmpFileIsNotExist(updateModel, inputSubPaths);

	        // Rename .dng files .xmp to update in exifTool
	        var subPathsList = PathsListTagsFromFile(inputSubPaths);
 
	        var command = ExifToolCommandLineArgs(updateModel, comparedNames, includeSoftware);
        
	        foreach (var path in subPathsList)
	        {
		        if ( ! _iStorage.ExistFile(path) ) continue;
		        await _exifTool.WriteTagsAsync(path, command);
	        }

	        if (  _thumbnailStorage.ExistFile(updateModel.FileHash) )
	        {
		        Console.Write("thumbnail: -> ");
		        await _exifTool.WriteTagsThumbnailAsync(updateModel.FileHash, command);
	        }

	        return command;
        }

	    private string UpdateLocationAltitudeCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            // -GPSAltitude="+160" -GPSAltitudeRef=above
            if (comparedNames.Contains(nameof(FileIndexItem.LocationAltitude).ToLowerInvariant() ))
            {
                // 0 = "Above Sea Level"
                // 1 = Below Sea Level
                var gpsAltitudeRef = "0";
                var gpsAltitude = "+" + updateModel.LocationAltitude.ToString(CultureInfo.InvariantCulture);
                if (updateModel.LocationAltitude < 0)
                {
                    gpsAltitudeRef = "1";
                    gpsAltitude = "-" + (updateModel.LocationAltitude * -1).ToString(CultureInfo.InvariantCulture);
                } 
                command += $" -GPSAltitude=\"{gpsAltitude}\" -gpsaltituderef#=\"{gpsAltitudeRef}\" " +
                           $"-xmp-exif:GPSAltitude=\"{gpsAltitude}\" -xmp-exif:gpsaltituderef#=\"{gpsAltitudeRef}\" ";
            }
            return command;
        }

        private string UpdateGpsLatitudeCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
	        // To Reset Image:
	        // exiftool reset.jpg -gps:all= -xmp:geotag= -City= -xmp:City= -State= -xmp:State= -overwrite_original

	        // CultureInfo.InvariantCulture is used for systems where comma is the default seperator
            if (comparedNames.Contains( nameof(FileIndexItem.Latitude).ToLowerInvariant() ))
            {
	            var latitudeString = updateModel.Latitude.ToString(CultureInfo.InvariantCulture);
	            command +=
		            $" -GPSLatitude=\"{latitudeString}\" -GPSLatitudeRef=\"{latitudeString}\" "
		            + $" -xmp-exif:GPSLatitude={latitudeString} "
		            + $" -xmp-exif:GPSLatitudeRef={latitudeString} ";
            }
            return command;
        }
        
        private string UpdateGpsLongitudeCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.Longitude).ToLowerInvariant()))
            {
	            var longitudeString = updateModel.Longitude.ToString(CultureInfo.InvariantCulture);
	            command +=
		            $" -GPSLongitude=\"{longitudeString}\" -GPSLongitudeRef=\"{longitudeString}\" "
		            + $" -xmp-exif:GPSLongitude={longitudeString} "
		            + $" -xmp-exif:GPSLongitudeRef={longitudeString} ";
            }
            return command;
        }

        private static string UpdateKeywordsCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.Tags).ToLowerInvariant() ))
            {
	            command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags
	                                                          + $" \" -Keywords=\"{updateModel.Tags}\""; // space before
            }
            return command;
        }
        
        private static string UpdateLocationCityCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationCity).ToLowerInvariant() ) )
            {
                command += " -City=\"" + updateModel.LocationCity 
                                                   + "\" -xmp:City=\"" + updateModel.LocationCity + "\"";
            }
            return command;
        }
        
        /// <summary>
        /// Add state to ExifTool command
        /// to remove:
        /// -Country= -Country-PrimaryLocationName= -State= -Province-State=  -City= -xmp:City= -overwrite_original
        /// </summary>
        /// <param name="command">Command that is used</param>
        /// <param name="comparedNames">names lowercase</param>
        /// <param name="updateModel">the model with the data</param>
        /// <returns></returns>
        private static string UpdateLocationStateCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationState).ToLowerInvariant() ))
            {
                command += " -State=\"" + updateModel.LocationState 
                                       + "\" -Province-State=\"" + updateModel.LocationState + "\"";
            }
            return command;
        }
        
        private static string UpdateLocationCountryCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationCountry).ToLowerInvariant() ))
            {
                command += " -Country=\"" + updateModel.LocationCountry 
                                        + "\" -Country-PrimaryLocationName=\"" + updateModel.LocationCountry + "\"";
            }
            return command;
        }
        
        private static string UpdateDescriptionCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.Description).ToLowerInvariant()    ))
            {
                command += " -Caption-Abstract=\"" + updateModel.Description 
                                                   + "\" -Description=\"" + updateModel.Description + "\""
                                                   + $" \"-xmp-dc:description={updateModel.Description}\"";
            }
            return command;
        }
        
	    /// <summary>
	    /// Update Software field
	    /// </summary>
	    /// <param name="command">exiftool command to add it to</param>
	    /// <param name="comparedNames">list of all fields that are edited</param>
	    /// <param name="updateModel">the model that has the data</param>
	    /// <param name="includeSoftware">to include the original software name</param>
	    /// <returns></returns>
	    private static string UpdateSoftwareCommand(string command, List<string> comparedNames, FileIndexItem updateModel, bool includeSoftware)
	    {
		    if ( !comparedNames.Contains(nameof(FileIndexItem.Software).ToLowerInvariant()) )
			    return command;

		    if ( includeSoftware )
		    {
			    // add space before
			    command +=
				    $" -Software=\"{updateModel.Software}\" -CreatorTool=\"{updateModel.Software}\" -HistorySoftwareAgent=\"{updateModel.Software}\" " +
				    "-HistoryParameters=\"\" -PMVersion=\"\" ";
		    }
		    else
		    {
			    command +=
				    " -Software=\"Starsky\" -CreatorTool=\"Starsky\" -HistorySoftwareAgent=\"Starsky\" " +
				    "-HistoryParameters=\"\" -PMVersion=\"\" ";
		    }
		    
		    return command;
	    }
	    
	    /// <summary>
	    /// Update Meta Field that contains Image Height (DOES NOT change the actual size)
	    /// </summary>
	    /// <param name="command">exiftool command to add it to</param>
	    /// <param name="comparedNames">list of all fields that are edited</param>
	    /// <param name="updateModel">the model that has the data</param>
	    /// <returns></returns>
	    private static string UpdateImageHeightCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if ( !comparedNames.Contains(nameof(FileIndexItem.ImageHeight).ToLowerInvariant()) )
			    return command;

		    // add space before
		    command +=
			    $" -exifimageheight={updateModel.ImageHeight} ";
		    
		    return command;
	    }
	    
	    /// <summary>
	    /// Update Meta Field that contains Image Width (DOES NOT change the actual size)
	    /// </summary>
	    /// <param name="command">exiftool command to add it to</param>
	    /// <param name="comparedNames">list of all fields that are edited</param>
	    /// <param name="updateModel">the model that has the data</param>
	    /// <returns></returns>
	    private static string UpdateImageWidthCommand(string command, ICollection<string> comparedNames, FileIndexItem updateModel)
	    {
		    if ( !comparedNames.Contains(nameof(FileIndexItem.ImageWidth).ToLowerInvariant()) )
			    return command;

		    // add space before
		    command +=
			    $" -exifimagewidth={updateModel.ImageWidth} ";
		    
		    return command;
	    }
	    
	    private static string UpdateTitleCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if (comparedNames.Contains(nameof(FileIndexItem.Title).ToLowerInvariant()))
		    {
			    command += " -ObjectName=\"" + updateModel.Title + "\"" 
			               + " \"-title\"=" + "\"" + updateModel.Title  + "\""
						   + $" \"-xmp-dc:title={updateModel.Title}\"";

		    }
		    return command;
	    }

	    private static string UpdateColorClassCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
			if (comparedNames.Contains(nameof(FileIndexItem.ColorClass).ToLowerInvariant()) && 
			    updateModel.ColorClass != ColorClassParser.Color.DoNotChange)
			{
				var intColorClass = (int) updateModel.ColorClass;
	
				var colorDisplayName = EnumHelper.GetDisplayName(updateModel.ColorClass);
				command += " \"-xmp:Label\"=" + "\"" + colorDisplayName + "\"" + " -ColorClass=\""+ intColorClass + 
						   "\" -Prefs=\"Tagged:0 ColorClass:" + intColorClass + " Rating:0 FrameNum:0\" ";
			}
		    return command;
	    }

	    private static string UpdateOrientationCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // // exiftool -Orientation#=5
		    if (comparedNames.Contains( nameof(FileIndexItem.Orientation).ToLowerInvariant() ) && 
		        updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
		    {
			    var intOrientation = (int) updateModel.Orientation;
			    command += " \"-Orientation#="+ intOrientation +"\" ";
		    }
		    return command;
	    }

	    private static string UpdateDateTimeCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {

		    if ( comparedNames.Contains(nameof(FileIndexItem.DateTime).ToLowerInvariant()) &&
		         updateModel.DateTime.Year > 2 )
		    {
			    var exifToolDatetimeString = updateModel.DateTime.ToString(
				    "yyyy:MM:dd HH:mm:ss",
				    CultureInfo.InvariantCulture);
			    command += $" -AllDates=\"{exifToolDatetimeString}\" \"-xmp:datecreated={exifToolDatetimeString}\"";
		    }
		    
		    return command;
	    }

	    private static string UpdateIsoSpeedCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    if ( comparedNames.Contains(nameof(FileIndexItem.IsoSpeed).ToLowerInvariant()) )
		    {
			    command += $" -ISO=\"{updateModel.IsoSpeed}\" \"-xmp:ISO={updateModel.IsoSpeed}\" ";
		    }

		    return command;
	    }

	    private static string UpdateApertureCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // Warning: Sorry, Aperture is not writable => FNumber is writable
		    // XMP,http://ns.adobe.com/exif/1.0/,exif:FNumber,9/1
		    if ( !comparedNames.Contains(nameof(FileIndexItem.Aperture).ToLowerInvariant()) ) return command;
		    
		    var aperture = updateModel.Aperture.ToString(CultureInfo.InvariantCulture);
		    command += $" -FNumber=\"{aperture}\" \"-xmp:FNumber={aperture}\" ";
		    return command;
	    }
	    
	    private static string UpdateShutterSpeedCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // // -ExposureTime=1/31
		    // Warning: Sorry, ShutterSpeed is not writable => ExposureTime is writable
		    if ( !comparedNames.Contains(nameof(FileIndexItem.ShutterSpeed).ToLowerInvariant()) ) return command;
		    
		    command += $" -ExposureTime=\"{updateModel.ShutterSpeed}\" \"-xmp:ExposureTime={updateModel.ShutterSpeed}\" ";

		    return command;
	    }

	    private static string UpdateMakeModelCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // Make and Model are not writable so those never exist in this list
		    if ( !comparedNames.Contains(nameof(FileIndexItem.MakeModel).ToLowerInvariant()) ) return command;
		    
		    var make = updateModel.Make;
		    var model = updateModel.Model;
		    command += " -make=\"" + make + "\"" + " -model=\"" + model + "\"";
		    
		    if ( !string.IsNullOrWhiteSpace(updateModel.LensModel) )
		    {
			    // add space before
			    command += $" -lensmodel=\"{updateModel.LensModel}\" ";
		    }
		    
		    return command;
	    }

	    private string UpdateFocalLengthCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if ( !comparedNames.Contains(nameof(FileIndexItem.FocalLength).ToLowerInvariant()) ) return command;

		    var focalLength = $"{updateModel.FocalLength.ToString(CultureInfo.InvariantCulture)} mm";
		    command += $" -FocalLength=\"{focalLength}\" \"-xmp:FocalLength={focalLength}\" ";

		    return command;
	    }

    }
}
