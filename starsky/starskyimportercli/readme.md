
## Structure configuation

- dd 	 -   The day of the month, from 01 through 31.
- MM 	 -   The month, from 01 through 12.
- yyyy 	-    The year as a four-digit number.
- HH 	 -   The hour, using a 24-hour clock from 00 to 23.
- mm 	 -   The minute, from 00 through 59.
- ss 	 -   The second, from 00 through 59.
- \\\     -      (double escape sign or double backslash); to escape dd use this: \\\d\\\d 
- /     -       (slash); is split in folder (Windows / Linux / Mac)
- .ext   -       (dot ext); extension for example jpg
- (nothing)  -   extension is forced
- \*      -     (asterisk); match anything
- \*od\*    -    match 'de'-folder so for example the folder: good

Check for more date conversions:
https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
