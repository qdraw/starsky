using Microsoft.AspNetCore.Http;
using starsky.foundation.platform.Models;

namespace starskycore.Models
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
	        if ( !string.IsNullOrWhiteSpace(request.Headers["ColorClass"]) )
	        {
		        int.TryParse(request.Headers["ColorClass"], out var colorClass);
		        ColorClass = colorClass;
	        }

            Structure = request.Headers["Structure"].ToString();

            // Always when importing using a request
            // otherwise it will stick in the temp folder
            DeleteAfter = true;
	        
	        // For the index Mode, false is always copy, true is check if exist in db, default true
	        IndexMode = true;
	        if(request.Headers["IndexMode"].ToString().ToLower() == "false")
		        IndexMode = false;
	        
        }


        // This is optinal, when not in use ignore this setting
        private string _structure;
        public string Structure
        {
            get => string.IsNullOrEmpty(_structure) ? string.Empty : _structure; // if null>stringEmpty
            set
            {
                // Changed this => value used te be without check
                if (string.IsNullOrEmpty(value)) return;
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
        public int ColorClass {
            get => _colorClass;
            set {
                if (value >= 0 && value <= 8) // hardcoded in FileIndexModel
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

	    /// <summary>
	    /// Default false, when Exiftool need to sync content
	    /// </summary>
	    public bool NeedExiftoolSync { get; set; } = false;
    }
}
