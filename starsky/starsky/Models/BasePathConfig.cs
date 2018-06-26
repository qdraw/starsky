using System.Collections.Generic;
using starsky.Services;

namespace starsky.Models
{
    // used for .config files
    public class BasePathConfig
    {
        private List<string> _readonly;
        public List<string> Readonly
        {
            get => _readonly;
            set
            {
                _readonly = new List<string>();

                if (value == null) return;

                foreach (var item in value)
                {
                    _readonly.Add(ConfigRead.RemoveLatestBackslash(item));
                }
            }
        }

        // todo: merge with: AppSettingsProvider.cs
        private string _structure;
        public string Structure
        {
            get
            {
                if (string.IsNullOrEmpty(_structure))
                {
                    //   - dd 	        The day of the month, from 01 through 31.
                    //   - MM 	        The month, from 01 through 12.
                    //   - yyyy 	    The year as a four-digit number.
                    //   - HH 	        The hour, using a 24-hour clock from 00 to 23.
                    //   - mm 	        The minute, from 00 through 59.
                    //   - ss 	        The second, from 00 through 59.
                    //   - https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
                    //   - \\           (double escape sign or double backslash); to escape dd use this: \\d\\d 
                    //   - /            (slash); is split in folder (Windows / Linux / Mac)
                    //   - .ext         (dot ext); extension for example: .jpg
                    //   - (nothing)    extension is forced
                    //   - *            (asterisk); match anything
                    //   - *od*         match 'de'-folder so for example the folder: good
                    
                    
                    //    Please update /starskyimportercli/readme.md when this changes
                    
                    return "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
                }
                return _structure;
            }
            set // using Json importer
            {
                if (string.IsNullOrEmpty(value) || value == "/") return;
                var structure = ConfigRead.PrefixDbSlash(value);
                _structure = ConfigRead.RemoveLatestBackslash(structure);
            }
        }
    }
}