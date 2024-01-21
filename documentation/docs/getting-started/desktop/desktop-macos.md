# Desktop Mac OS

Install on a Macbook or Mac OS

Do code signing yourself or and remove from quarantine with the following command:
````
codesign --force --deep -s - /Applications/Starsky.app && xattr -rd com.apple.quarantine /Applications/Starsky.app
````

_To add more instructions on how to_