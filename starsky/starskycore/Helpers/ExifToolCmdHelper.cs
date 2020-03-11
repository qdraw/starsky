using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskycore.Helpers
{
    public partial class ExifToolCmdHelper
    {
        private readonly IExifTool _exifTool;
	    private readonly IStorage _iStorage;
	    private readonly IStorage _thumbnailStorage;
	    private readonly IReadMeta _readMeta;

	    public ExifToolCmdHelper(IExifTool exifTool, IStorage iStorage, IStorage thumbnailStorage, IReadMeta readMeta)
        {
            _exifTool = exifTool;
	        _iStorage = iStorage;
	        _readMeta = readMeta;
	        _thumbnailStorage = thumbnailStorage;
        }

	    /// <summary>
	    /// To update Exiftool (both Thumbnail as Storage item)
	    /// </summary>
	    /// <param name="updateModel">update model</param>
	    /// <param name="comparedNames">list,string e.g. Tags</param>
	    /// <returns></returns>
        public string Update(FileIndexItem updateModel, List<string> comparedNames)
        {
            var exifUpdateFilePaths = new List<string>
            {
                updateModel.FilePath           
            };
	        return UpdateAsyncWrapperBoth(updateModel, exifUpdateFilePaths, comparedNames).Result;
        }

	    /// <summary>
	    /// To update Exiftool (both Thumbnail as Storage item)
	    /// </summary>
	    /// <param name="updateModel"></param>
	    /// <param name="inputSubPaths"></param>
	    /// <param name="comparedNames"></param>
	    /// <returns></returns>
	    public string Update(FileIndexItem updateModel, List<string> inputSubPaths,
		    List<string> comparedNames)
	    {
		    return UpdateAsyncWrapperBoth(updateModel, inputSubPaths, comparedNames).Result;
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
	    /// <returns></returns>
	    private async Task<string> UpdateAsyncWrapperBoth(FileIndexItem updateModel, List<string> inputSubPaths, List<string> comparedNames)
	    {
		    var task = Task.Run(() => UpdateASyncBoth(updateModel,inputSubPaths,comparedNames));
		    return task.Wait(TimeSpan.FromSeconds(20)) ? task.Result : string.Empty;
	    }
	    
	    public string ExifToolCommandLineArgs( FileIndexItem updateModel, List<string> comparedNames )
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
		    
	        command = UpdateSoftwareCommand(command, comparedNames, updateModel);

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
			    var command = ExifToolCommandLineArgs(updateModel, comparedNames);
				    
			    await _exifTool.WriteTagsAsync(withXmp, command);
		    }
	    }

        private async Task<string> UpdateASyncBoth(FileIndexItem updateModel, List<string> inputSubPaths, List<string> comparedNames )
        {
	        // Creation and update .xmp file with all availeble content
	        await CreateXmpFileIsNotExist(updateModel, inputSubPaths);

	        // Rename .dng files .xmp to update in exifTool
	        var subPathsList = PathsListTagsFromFile(inputSubPaths);

	        var command = ExifToolCommandLineArgs(updateModel, comparedNames);
        
	        foreach (var path in subPathsList)
	        {
		        if ( ! _iStorage.ExistFile(path) ) continue;
		        await _exifTool.WriteTagsAsync(path, command);
	        }

	        if (  _thumbnailStorage.ExistFile(updateModel.FileHash) )
	        {
		        await _exifTool.WriteTagsThumbnailAsync(updateModel.FileHash, command);
	        }

	        return command;
        }



	    private string UpdateLocationAltitudeCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            // -GPSAltitude="+160" -GPSAltitudeRef=above
            if (comparedNames.Contains(nameof(FileIndexItem.LocationAltitude)))
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
	        // exiftool reset.jpg -gps:all= -xmp:geotag= -City= -xmp:City= -State= -xmp:State=

	        // CultureInfo.InvariantCulture is used for systems where comma is the default seperator
            if (comparedNames.Contains( nameof(FileIndexItem.Latitude) ))
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
            if (comparedNames.Contains( nameof(FileIndexItem.Longitude)))
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
            if (comparedNames.Contains( nameof(FileIndexItem.Tags) ))
            {
	            command += " -sep \", \" \"-xmp:subject\"=\"" + updateModel.Tags
	                                                          + $" \" -Keywords=\"{updateModel.Tags}\""; // space before
            }
            return command;
        }
        
        private static string UpdateLocationCityCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationCity) ) )
            {
                command += " -City=\"" + updateModel.LocationCity 
                                                   + "\" -xmp:City=\"" + updateModel.LocationCity + "\"";
            }
            return command;
        }
        
        private static string UpdateLocationStateCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationState) ))
            {
                command += " -State=\"" + updateModel.LocationState 
                                       + "\" -Province-State=\"" + updateModel.LocationState + "\"";
            }
            return command;
        }
        
        private static string UpdateLocationCountryCommand(
	        string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.LocationCountry) ))
            {
                command += " -Country=\"" + updateModel.LocationCountry 
                                        + "\" -Country-PrimaryLocationName=\"" + updateModel.LocationCountry + "\"";
            }
            return command;
        }
        
        private static string UpdateDescriptionCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
        {
            if (comparedNames.Contains( nameof(FileIndexItem.Description)    ))
            {
                command += " -Caption-Abstract=\"" + updateModel.Description 
                                                   + "\" -Description=\"" + updateModel.Description + "\""
                                                   + $" \"-xmp-dc:description={updateModel.Description}\"";
            }
            return command;
        }
	    
	    private static string UpdateSoftwareCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if (comparedNames.Contains( nameof(FileIndexItem.Software) ))
		    {
				// add space before
			    command +=
				    " -Software=\"Qdraw 1.0\" -CreatorTool=\"Qdraw 1.0\" -HistorySoftwareAgent=\"Qdraw 1.0\" -HistoryParameters=\"\" -PMVersion=\"\" ";
		    }
		    return command;
	    }
	    
	    private static string UpdateTitleCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if (comparedNames.Contains(nameof(FileIndexItem.Title)))
		    {
			    command += " -ObjectName=\"" + updateModel.Title + "\"" 
			               + " \"-title\"=" + "\"" + updateModel.Title  + "\""
						   + $" \"-xmp-dc:title={updateModel.Title}\"";

		    }
		    return command;
	    }

	    private static string UpdateColorClassCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
			if (comparedNames.Contains(nameof(FileIndexItem.ColorClass)) && updateModel.ColorClass != FileIndexItem.Color.DoNotChange)
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
		    if (comparedNames.Contains( nameof(FileIndexItem.Orientation) ) && updateModel.Orientation != FileIndexItem.Rotation.DoNotChange)
		    {
			    var intOrientation = (int) updateModel.Orientation;
			    command += " \"-Orientation#="+ intOrientation +"\" ";
		    }
		    return command;
	    }


	    private static string UpdateDateTimeCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {

		    if ( comparedNames.Contains(nameof(FileIndexItem.DateTime)) &&
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
		    if ( comparedNames.Contains(nameof(FileIndexItem.IsoSpeed)) )
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
		    if ( !comparedNames.Contains(nameof(FileIndexItem.Aperture)) ) return command;
		    
		    var aperture = updateModel.Aperture.ToString(CultureInfo.InvariantCulture);
		    command += $" -FNumber=\"{aperture}\" \"-xmp:FNumber={aperture}\" ";
		    return command;
	    }

	    
	    private static string UpdateShutterSpeedCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // // -ExposureTime=1/31
		    // Warning: Sorry, ShutterSpeed is not writable => ExposureTime is writable
		    if ( !comparedNames.Contains(nameof(FileIndexItem.ShutterSpeed)) ) return command;
		    
		    command += $" -ExposureTime=\"{updateModel.ShutterSpeed}\" \"-xmp:ExposureTime={updateModel.ShutterSpeed}\" ";

		    return command;
	    }


	    private static string UpdateMakeModelCommand(string command, List<string> comparedNames,
		    FileIndexItem updateModel)
	    {
		    // Make and Model are not writable so those never exist in this list
		    if ( !comparedNames.Contains(nameof(FileIndexItem.MakeModel)) ) return command;
		    
		    var make = updateModel.Make;
		    var model = updateModel.Model;
		    command += " -make=\"" + make + "\"" + " -model=\"" + model + "\"";

		    return command;
	    }


	    private string UpdateFocalLengthCommand(string command, List<string> comparedNames, FileIndexItem updateModel)
	    {
		    if ( !comparedNames.Contains(nameof(FileIndexItem.FocalLength)) ) return command;

		    var focalLength = $"{updateModel.FocalLength} mm";
		    command += $" -FocalLength=\"{focalLength}\" \"-xmp:FocalLength={focalLength}\" ";

		    return command;
	    }

    }
}
