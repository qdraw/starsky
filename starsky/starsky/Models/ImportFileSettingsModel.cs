using System.ComponentModel.DataAnnotations;

namespace starsky.Models
{
    public class ImportSettingsModel
    {
        public ImportSettingsModel()
        {
            DeleteAfter = false;
            AgeFileFilter = true;
            RecursiveDirectory = false;
            // ColorClass defaults in prop
        }
        
        public bool DeleteAfter { get; set; }
        
        public bool AgeFileFilter { get; set; }
        
        public bool RecursiveDirectory { get; set; }

        private int _colorClass;
        public int ColorClass {
            get => _colorClass;
            set {
                if (value >= 0 && value <= 8) _colorClass = value;
                _colorClass = 0;
            }
        }

    }
}