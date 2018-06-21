
## Structure configuation

- "dd" 	 -   The day of the month, from 01 through 31.
- "MM" 	 -   The month, from 01 through 12.
- "yyyy" 	-    The year as a four-digit number.
- "HH" 	 -   The hour, using a 24-hour clock from 00 to 23.
- "mm" 	 -   The minute, from 00 through 59.
- "ss" 	 -   The second, from 00 through 59.
- \\\     -      Double escape; to escape dd use this: \\\d\\\d 
- /     -       is split in folder (Windows / Linux / Mac)
- .ext   -       extension for example jpg
- (nothing)  -   extension is forced
- \*      -     match anything
- \*od\*    -    match 'de'-folder so for example the folder: good

Check for more date conversions:
https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
