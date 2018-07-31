﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using starsky.Helpers;

namespace starsky.Models
{
    public class ImportSettingsModel
    {
        // Default constructor
        public ImportSettingsModel()
        {
            DeleteAfter = false;
            AgeFileFilter = true;
            RecursiveDirectory = false;
            // ColorClass defaults in prop
            // Structure defaults in appsettings
        }
        
        // Construct model using a request
        public ImportSettingsModel(HttpRequest request)
        {
            bool.TryParse(request.Headers["AgeFileFilter"], out var ageFileFilter);
            AgeFileFilter = ageFileFilter;
            
            int.TryParse(request.Headers["ColorClass"], out var colorClass);
            ColorClass = colorClass;

            Structure = request.Headers["Structure"].ToString();

            // Always when importing using a request
            // otherwise it will stick in the temp folder
            DeleteAfter = true;
        }

        
        // This is optinal, when not in use ignore this setting
        private string _structure;
        public string Structure
        {
            get { return _structure; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    AppSettings.StructureCheck(value);
                }
                _structure = value;
            }
        }

        public bool DeleteAfter { get; set; }
        
        public bool AgeFileFilter { get; set; }
        
        public bool RecursiveDirectory { get; set; }

        private int _colorClass;
        public int ColorClass {
            get => _colorClass;
            set {
                if (value >= 0 && value <= 8) // hardcoded in FileIndexModel
                {
                     _colorClass = value;
                    return;
                }
                _colorClass = 0;
            }
        }

    }
}