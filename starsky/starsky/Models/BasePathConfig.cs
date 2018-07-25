using starsky.Services;

namespace starsky.Models
{
    public class BasePathConfig
    {
        // todo: merge with: AppSettingsProvider.cs
        private string _structure;
        public string Structure
        {
            get
            {
                if (string.IsNullOrEmpty(_structure))
                {
                    //   - dd 	            The day of the month, from 01 through 31.
                    //   - MM 	            The month, from 01 through 12.
                    //   - yyyy 	        The year as a four-digit number.
                    //   - HH 	            The hour, using a 24-hour clock from 00 to 23.
                    //   - mm 	            The minute, from 00 through 59.
                    //   - ss 	            The second, from 00 through 59.
                    //   - https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
                    //   - \\               (double escape sign or double backslash); to escape dd use this: \\d\\d 
                    //   - /                (slash); is split in folder (Windows / Linux / Mac)
                    //   - .ext             (dot ext); extension for example: .jpg
                    //   - (nothing)        extension is forced
                    //   - {filenamebase}   use the orginal filename without extension
                    //   - *                (asterisk); match anything
                    //   - *starksy*        Match the folder match that contains the word 'starksy'
                    
                    
                    //    Please update /starskyimportercli/readme.md when this changes
                    
                    return "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext";
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